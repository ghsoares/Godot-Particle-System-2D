using Godot;

namespace ParticleSystem2DPlugin
{
    [Tool]
    [ParticleModulePath("Init/Options")]
    public class ParticleSystem2DEmitOptionsModule : ParticleSystem2DModule, IParticleInitializable
    {
        public RandomNumberEditor.Mode lifetimeMode;
        public float minLifetime = 1f, maxLifetime = 1f;
        public RandomNumberEditor.Mode speedMode;
        public float minSpeed = 5f, maxSpeed = 5f;
        public RandomNumberEditor.Mode sizeMode;
        public float minSize = 1f, maxSize = 1f;
        public RandomNumberEditor.Mode rotationMode;
        public float minRotation = 0f, maxRotation = 0f;
        public Color startColor = Colors.White;

        public override void DrawInterface(Control parent)
        {
            Control lifetimeField = moduleInterface.AddField("Lifetime");
            Control speedField = moduleInterface.AddField("Speed");
            Control sizeField = moduleInterface.AddField("Size");
            Control rotationField = moduleInterface.AddField("Rotation");
            Control startColorField = moduleInterface.AddField("Color");

            RandomNumberEditor randomLifetimeEditor = moduleInterface.AddRandomNumberEditor(lifetimeField);
            RandomNumberEditor randomSpeedEditor = moduleInterface.AddRandomNumberEditor(speedField);
            RandomNumberEditor randomSizeEditor = moduleInterface.AddRandomNumberEditor(sizeField);
            RandomNumberEditor randomRotationEditor = moduleInterface.AddRandomNumberEditor(rotationField);
            ColorPickerButton startColorEditor = moduleInterface.AddColorEditor(startColorField);

            randomLifetimeEditor.minValue = minLifetime;
            randomLifetimeEditor.maxValue = maxLifetime;
            randomLifetimeEditor.mode = lifetimeMode;
            randomLifetimeEditor.allowGreater = true;
            randomLifetimeEditor.allowLesser = true;
            randomLifetimeEditor.Connect("Changed", this, "OnRandomLifetimeChanged");

            randomSpeedEditor.minValue = minSpeed;
            randomSpeedEditor.maxValue = maxSpeed;
            randomSpeedEditor.mode = speedMode;
            randomSpeedEditor.allowGreater = true;
            randomSpeedEditor.allowLesser = true;
            randomSpeedEditor.Connect("Changed", this, "OnRandomSpeedChanged");

            randomSizeEditor.minValue = minSize;
            randomSizeEditor.maxValue = maxSize;
            randomSizeEditor.mode = sizeMode;
            randomSpeedEditor.allowGreater = true;
            randomSpeedEditor.allowLesser = true;
            randomSizeEditor.Connect("Changed", this, "OnRandomSizeChanged");

            randomRotationEditor.minValue = minRotation;
            randomRotationEditor.maxValue = maxRotation;
            randomRotationEditor.mode = rotationMode;
            randomSpeedEditor.allowGreater = true;
            randomSpeedEditor.allowLesser = true;
            randomRotationEditor.Connect("Changed", this, "OnRandomRotationChanged");

            startColorEditor.Color = startColor;
            startColorEditor.Connect("color_changed", this, "OnStartColorChanged");
        }

        public override Godot.Collections.Dictionary SerializeModule()
        {
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();

            data["lifetimeMode"] = (int)lifetimeMode;
            data["minLifetime"] = minLifetime;
            data["maxLifetime"] = maxLifetime;

            data["speedMode"] = (int)speedMode;
            data["minSpeed"] = minSpeed;
            data["maxSpeed"] = maxSpeed;

            data["sizeMode"] = (int)sizeMode;
            data["minSize"] = minSize;
            data["maxSize"] = maxSize;

            data["rotationMode"] = (int)rotationMode;
            data["minRotation"] = minRotation;
            data["maxRotation"] = maxRotation;

            data["startColor"] = startColor;

            return data;
        }

        public override void UnSerializeModule(Godot.Collections.Dictionary data)
        {
            lifetimeMode = (RandomNumberEditor.Mode)data["lifetimeMode"];
            minLifetime = (float)data["minLifetime"];
            maxLifetime = (float)data["maxLifetime"];

            speedMode = (RandomNumberEditor.Mode)data["speedMode"];
            minSpeed = (float)data["minSpeed"];
            maxSpeed = (float)data["maxSpeed"];

            sizeMode = (RandomNumberEditor.Mode)data["sizeMode"];
            minSize = (float)data["minSize"];
            maxSize = (float)data["maxSize"];

            rotationMode = (RandomNumberEditor.Mode)data["rotationMode"];
            minRotation = (float)data["minRotation"];
            maxRotation = (float)data["maxRotation"];

            startColor = (Color)data["startColor"];
        }

        public void OnRandomLifetimeChanged(float min, float max, RandomNumberEditor.Mode mode)
        {
            moduleInterface.undoRedo.CreateAction("Set Option Lifetime");
            moduleInterface.undoRedo.AddDoProperty(this, "minLifetime", min);
            moduleInterface.undoRedo.AddDoProperty(this, "maxLifetime", max);
            moduleInterface.undoRedo.AddDoProperty(this, "lifetimeMode", mode);
            moduleInterface.undoRedo.AddDoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoProperty(this, "minLifetime", minLifetime);
            moduleInterface.undoRedo.AddUndoProperty(this, "maxLifetime", maxLifetime);
            moduleInterface.undoRedo.AddUndoProperty(this, "lifetimeMode", lifetimeMode);
            moduleInterface.undoRedo.AddUndoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoMethod(moduleInterface, "UpdateInterface");
            moduleInterface.undoRedo.CommitAction();
        }

        public void OnRandomSpeedChanged(float min, float max, RandomNumberEditor.Mode mode)
        {
            moduleInterface.undoRedo.CreateAction("Set Option Speed");
            moduleInterface.undoRedo.AddDoProperty(this, "minSpeed", min);
            moduleInterface.undoRedo.AddDoProperty(this, "maxSpeed", max);
            moduleInterface.undoRedo.AddDoProperty(this, "speedMode", mode);
            moduleInterface.undoRedo.AddDoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoProperty(this, "minSpeed", minSpeed);
            moduleInterface.undoRedo.AddUndoProperty(this, "maxSpeed", maxSpeed);
            moduleInterface.undoRedo.AddUndoProperty(this, "speedMode", speedMode);
            moduleInterface.undoRedo.AddUndoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoMethod(moduleInterface, "UpdateInterface");
            moduleInterface.undoRedo.CommitAction();
        }

        public void OnRandomSizeChanged(float min, float max, RandomNumberEditor.Mode mode)
        {
            moduleInterface.undoRedo.CreateAction("Set Option Size");
            moduleInterface.undoRedo.AddDoProperty(this, "minSize", min);
            moduleInterface.undoRedo.AddDoProperty(this, "maxSize", max);
            moduleInterface.undoRedo.AddDoProperty(this, "sizeMode", mode);
            moduleInterface.undoRedo.AddDoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoProperty(this, "minSize", minSize);
            moduleInterface.undoRedo.AddUndoProperty(this, "maxSize", maxSize);
            moduleInterface.undoRedo.AddUndoProperty(this, "sizeMode", sizeMode);
            moduleInterface.undoRedo.AddUndoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoMethod(moduleInterface, "UpdateInterface");
            moduleInterface.undoRedo.CommitAction();
        }

        public void OnRandomRotationChanged(float min, float max, RandomNumberEditor.Mode mode)
        {
            moduleInterface.undoRedo.CreateAction("Set Option Rotation");
            moduleInterface.undoRedo.AddDoProperty(this, "minRotation", min);
            moduleInterface.undoRedo.AddDoProperty(this, "maxRotation", max);
            moduleInterface.undoRedo.AddDoProperty(this, "rotationMode", mode);
            moduleInterface.undoRedo.AddDoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoProperty(this, "minRotation", minRotation);
            moduleInterface.undoRedo.AddUndoProperty(this, "maxRotation", maxRotation);
            moduleInterface.undoRedo.AddUndoProperty(this, "rotationMode", rotationMode);
            moduleInterface.undoRedo.AddUndoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoMethod(moduleInterface, "UpdateInterface");
            moduleInterface.undoRedo.CommitAction();
        }

        public void OnStartColorChanged(Color newColor)
        {
            moduleInterface.undoRedo.CreateAction("Set Option Color");
            moduleInterface.undoRedo.AddDoProperty(this, "startColor", newColor);
            moduleInterface.undoRedo.AddDoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoProperty(this, "startColor", startColor);
            moduleInterface.undoRedo.AddUndoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoMethod(moduleInterface, "UpdateInterface");
            moduleInterface.undoRedo.CommitAction();
        }

        public void InitParticle(ref Particle particle, ParticleSystem2D.EmitParams emitparams)
        {
            float speed = Mathf.Lerp(
                minSpeed, maxSpeed, GD.Randf()
            );
            float lifetime = Mathf.Lerp(
                minLifetime, maxLifetime, GD.Randf()
            );
            float size = Mathf.Lerp(
                minSize, maxSize, GD.Randf()
            );
            float rotation = Mathf.Lerp(
                minRotation, maxRotation, GD.Randf()
            );

            Color color = startColor;

            particle.position = emitparams.shapeTransform.origin;

            particle.velocity = emitparams.shapeTransform.x * speed;

            particle.lifetime = lifetime;
            particle.currentLife = lifetime;

            particle.rotation = rotation;

            particle.baseSize = size;
            particle.size = size;

            particle.baseColor = color;
            particle.color = color;
        }

        public override string GetModuleName()
        {
            return "Emit Options";
        }
    }
}