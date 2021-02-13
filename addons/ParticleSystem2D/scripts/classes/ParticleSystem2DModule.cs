using Godot;
using System;

namespace ParticleSystem2DPlugin
{
    [System.AttributeUsage(System.AttributeTargets.Class |
                           System.AttributeTargets.Struct)
    ]
    public class ParticleModulePathAttribute : System.Attribute
    {
        public string path;

        public ParticleModulePathAttribute(string path)
        {
            this.path = path;
        }
    }

    public interface IParticleInitializable
    {
        void InitParticle(ref Particle particle, ParticleSystem2D.EmitParams emitparams);
    }

    public interface IParticleUpdateable
    {
        void UpdateParticle(ref Particle particle, float delta);
    }

    public interface IParticleDestroyable
    {
        void DestroyParticle(ref Particle particle);
    }

    public interface IParticleDrawable
    {
        void DrawParticle(Particle particle);
    }

    public interface IParticleBatchDrawable
    {
        void DrawBatch(Particle[] particles);
    }

    [Tool]
    public class ParticleSystem2DModule : Godot.Object
    {
        public bool enabled { get; set; }
        public bool uiHidden { get; set; }
        public ParticleModuleInterface moduleInterface { get; set; }

        public ParticleSystem2D particleSystem { get; set; }
        public World2D world { get; set; }
        public Physics2DDirectSpaceState spaceState { get; set; }

        public ParticleSystem2DModule()
        {
            enabled = true;
            uiHidden = true;
        }

        public virtual void InitModule() { }
        public virtual void EmitQuery() { }
        public virtual void UpdateModule(float delta) { }
        public virtual void DrawModule() { }
        public virtual Godot.Collections.Dictionary SerializeModule() { return new Godot.Collections.Dictionary(); }
        public virtual void UnSerializeModule(Godot.Collections.Dictionary data) { }

        public virtual void DrawInterface(Control parent)
        {
            Label label = new Label();
            parent.AddChild(label);
            label.Text = "Add the module properties here";
        }

        public virtual string GetModuleName()
        {
            return "Particle System 2D Module";
        }
    }
}