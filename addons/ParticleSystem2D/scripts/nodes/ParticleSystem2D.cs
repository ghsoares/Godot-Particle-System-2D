using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ParticleSystem2DPlugin
{
    [Tool]
    public class ParticleSystem2D : Node2D
    {
        public struct EmitParams
        {
            public Transform2D shapeTransform;
        }

        public enum UpdateMode
        {
            Process,
            PhysicsProcess
        }

        public enum SpaceMode
        {
            World,
            Local
        }

        private List<ParticleSystem2DModule> _modules { get; set; }
        private List<Godot.Collections.Dictionary> _modulesData { get; set; }
        private Particle[] _particles { get; set; }
        private int _maxParticles { get; set; }
        private int _seed { get; set; }
        private bool _emitting { get; set; }
        private bool _emitOnStart { get; set; }
        private float currentFrameDelta { get; set; }

        public Vector2 prevPos { get; set; }
        public Vector2 currentVelocity { get; set; }

        public bool forceModulesSerializationInGame = false;

        public List<ParticleSystem2DModule> modules
        {
            get
            {
                if (_modules == null)
                {
                    _modules = new List<ParticleSystem2DModule>();
                }
                return _modules;
            }
            set
            {
                _modules = value;
            }
        }
        public Particle[] particles
        {
            get
            {
                if (_particles == null || _particles.Length != maxParticles)
                {
                    _particles = new Particle[maxParticles];
                    for (int i = 0; i < maxParticles; i++)
                    {
                        _particles[i] = new Particle
                        {
                            idx = i,
                            alive = false
                        };
                    }
                }
                return _particles;
            }
        }
        public Vector2 origin
        {
            get
            {
                switch (spaceMode)
                {
                    case SpaceMode.World:
                        {
                            return GlobalPosition;
                        }
                }
                return Vector2.Zero;
            }
        }

        [Export]
        public List<Godot.Collections.Dictionary> modulesData
        {
            get
            {
                if (_modulesData == null)
                {
                    _modulesData = new List<Godot.Collections.Dictionary>();
                }
                return _modulesData;
            }
            set
            {
                _modulesData = value;
            }
        }

        [Export]
        public UpdateMode updateMode = UpdateMode.PhysicsProcess;
        [Export]
        public SpaceMode spaceMode = SpaceMode.World;
        [Export]
        public int maxParticles
        {
            get
            {
                return _maxParticles;
            }
            set
            {
                if (_maxParticles != value)
                {
                    _maxParticles = value;
                }
            }
        }
        [Export]
        public int seed
        {
            get
            {
                return _seed;
            }
            set
            {
                if (_seed != value)
                {
                    _seed = value;
                }
            }
        }
        [Export]
        public bool emitting
        {
            get
            {
                return _emitting;
            }
            set
            {
                if (_emitting != value)
                {
                    _emitting = value;
                }
            }
        }
        [Export]
        public bool emitOnStart
        {
            get
            {
                return _emitOnStart;
            }
            set
            {
                if (_emitOnStart != value)
                {
                    _emitOnStart = value;
                }
            }
        }
        [Export]
        public Vector2 gravity = new Vector2(0, 98f);
        [Export]
        public float timeScale = 1f;
        [Export]
        public int onEditorDrawFps = 24;
        [Export]
        public bool debugMode = false;

        public override void _EnterTree()
        {
            World2D world = GetWorld2d();
            Physics2DDirectSpaceState spaceState = world.DirectSpaceState;

            if (modulesData.Count == 0)
            {
                modules.Clear();
                maxParticles = 256;
                seed = new Random().Next();
                emitting = true;
                emitOnStart = true;

                AddModule<ParticleSystem2DEmitOptionsModule>(false);
                AddModule<ParticleSystem2DEmitRateModule>(false);
                AddModule<ParticleSystem2DUpdateModule>(false);
                AddModule<ParticleSystem2DDrawBatchModule>(false);

                WriteModulesData();
            }
            else
            {
                if (modules.Count != modulesData.Count)
                {
                    ReadModulesData();
                }
            }

            foreach (ParticleSystem2DModule module in modules)
            {
                module.particleSystem = this;
                module.InitModule();
            }

            prevPos = GlobalPosition;
        }

        public override void _ExitTree()
        {
            WriteModulesData();
        }

        public void WriteModulesData()
        {
            if (Engine.EditorHint || forceModulesSerializationInGame)
            {
                modulesData.Clear();
                foreach (ParticleSystem2DModule module in modules)
                {
                    Godot.Collections.Dictionary serializedDictionary = module.SerializeModule();
                    if (serializedDictionary != null)
                    {
                        serializedDictionary.Add("Type", module.GetType().ToString());
                        modulesData.Add(serializedDictionary);
                    }
                }
            }
        }

        public void ReadModulesData()
        {
            if (Engine.EditorHint || forceModulesSerializationInGame)
            {
                modules.Clear();
                foreach (Godot.Collections.Dictionary serialized in modulesData)
                {
                    Type t = Type.GetType((string)serialized["Type"]);
                    ParticleSystem2DModule module = (ParticleSystem2DModule)Activator.CreateInstance(t);
                    AddModule(module);
                    module.UnSerializeModule(serialized);
                }
            }
        }

        public T AddModule<T>(bool write = true) where T : ParticleSystem2DModule, new()
        {
            T newModule = new T();
            AddModule(newModule);
            if (write) WriteModulesData();
            return newModule;
        }

        public void AddModule(ParticleSystem2DModule module)
        {
            this.modules.Add(module);
            module.particleSystem = this;
            module.InitModule();
        }

        public T GetModule<T>() where T : ParticleSystem2DModule
        {
            return modules.OfType<T>().FirstOrDefault();
        }

        public void RemoveModule<T>() where T : ParticleSystem2DModule
        {
            T m = this.modules.OfType<T>().FirstOrDefault();
            if (m == null)
            {
                GD.PushWarning("Can't remove module of type " + typeof(T) + " because there isn't any.");
                return;
            }
            this.modules.Remove(m);
        }

        public void RemoveModule(ParticleSystem2DModule module)
        {
            this.modules.Remove(module);
        }

        public void ChangeModuleIndex(ParticleSystem2DModule module, int add)
        {
            int oldIdx = this.modules.IndexOf(module);
            int newIdx = oldIdx + add;
            newIdx = Mathf.Clamp(newIdx, 0, modules.Count - 1);

            ParticleSystem2DModule otherModule = modules[newIdx];

            modules[newIdx] = module;
            modules[oldIdx] = otherModule;
        }

        public override void _Process(float delta)
        {
            delta = Mathf.Abs(delta);

            if (updateMode == UpdateMode.Process)
            {
                UpdateSystem(delta);
            }

            if (Engine.EditorHint)
            {
                currentFrameDelta += onEditorDrawFps * delta;
                if (currentFrameDelta >= 1f)
                {
                    Update();
                    currentFrameDelta -= Mathf.Floor(currentFrameDelta);
                }
            }
            else
            {
                Update();
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            delta = Mathf.Abs(delta);

            if (updateMode == UpdateMode.PhysicsProcess)
            {
                UpdateSystem(delta);
            }
        }

        private void UpdateSystem(float delta)
        {
            try
            {
                if (timeScale < 0f) timeScale = 0f;
                delta *= timeScale;
                Vector2 deltaPos = GlobalPosition - prevPos;
                currentVelocity = deltaPos / delta;

                World2D world = GetWorld2d();
                Physics2DDirectSpaceState spaceState = world.DirectSpaceState;

                foreach (ParticleSystem2DModule module in modules)
                {
                    if (!module.enabled) continue;
                    module.particleSystem = this;
                    module.world = world;
                    module.spaceState = spaceState;
                    module.UpdateModule(delta);
                    for (int i = 0; i < maxParticles; i++)
                    {
                        IParticleUpdateable IUpdate = module as IParticleUpdateable;
                        if (particles[i].alive)
                        {
                            if (IUpdate != null)
                            {
                                IUpdate.UpdateParticle(ref particles[i], delta);
                            }
                            if (particles[i].currentLife <= 0f)
                            {
                                particles[i].currentLife = 0f;
                                DestroyParticle(i);
                                particles[i].alive = false;
                            }
                        }
                    }
                }
                prevPos = GlobalPosition;
            }
            catch (Exception e)
            {
                if (Engine.EditorHint) emitting = false;
                var st = new System.Diagnostics.StackTrace(e, true);
                GD.PrintErr(st);
            }
        }

        private void DestroyParticle(int idx)
        {
            foreach (ParticleSystem2DModule module in modules)
            {
                if (!module.enabled) continue;
                IParticleDestroyable IDestroy = module as IParticleDestroyable;
                if (IDestroy == null) continue;
                IDestroy.DestroyParticle(ref particles[idx]);
            }
        }

        private void InternalEmit(int idx, EmitParams emitParams)
        {
            Particle p = particles[idx];

            World2D world = GetWorld2d();
            Physics2DDirectSpaceState spaceState = world.DirectSpaceState;

            foreach (ParticleSystem2DModule module in modules)
            {
                if (!module.enabled) continue;
                IParticleInitializable IInit = module as IParticleInitializable;

                if (IInit == null) continue;

                module.world = world;
                module.spaceState = spaceState;
                IInit.InitParticle(ref p, emitParams);
            }

            p.alive = true;

            particles[idx] = p;
        }

        public void Emit()
        {
            foreach (ParticleSystem2DModule module in modules)
            {
                if (!module.enabled) continue;
                module.EmitQuery();
            }
        }

        public void Emit(int amount)
        {
            EmitParams emitParams = new EmitParams();

            Transform2D t = Transform2D.Identity;
            switch (spaceMode)
            {
                case SpaceMode.World:
                    {
                        t = GlobalTransform;
                        break;
                    }
            }

            emitParams.shapeTransform = t;
            Emit(emitParams, amount);
        }

        public void Emit(EmitParams emitParams, int amount)
        {
            if (amount <= 0) return;
            for (int i = 0; i < maxParticles; i++)
            {
                if (!particles[i].alive)
                {
                    InternalEmit(i, emitParams);
                    amount--;
                    if (amount == 0) break;
                }
            }
        }

        public override void _Draw()
        {
            if (!Visible) return;
            try
            {
                if (spaceMode == SpaceMode.World) DrawSetTransformMatrix(GetGlobalTransform().AffineInverse()); // FIXME, move this to _process, to enable global drawing

                foreach (ParticleSystem2DModule module in modules)
                {
                    if (!module.enabled) continue;

                    module.DrawModule();

                    IParticleDrawable IDrawSingle = module as IParticleDrawable;
                    IParticleBatchDrawable IDrawBatch = module as IParticleBatchDrawable;

                    if (IDrawSingle != null)
                    {
                        foreach (Particle p in particles)
                        {
                            if (p.alive)
                            {
                                IDrawSingle.DrawParticle(p);
                            }
                        }
                    }
                    if (IDrawBatch != null)
                    {
                        IDrawBatch.DrawBatch(particles);
                    }
                }
            }
            catch (Exception e)
            {
                if (Engine.EditorHint) emitting = false;
                var st = new System.Diagnostics.StackTrace(e, true);
                GD.PrintErr(st);
            }
        }
    }
}