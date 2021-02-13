using Godot;
using System;

namespace ParticleSystem2DPlugin
{
    [Tool]
    [ParticleModulePath("Update/Collision")]
    public class ParticleSystem2DCollisionModule : ParticleSystem2DModule, IParticleUpdateable, IParticleDrawable
    {
        public Godot.Collections.Array excludeBodies { get; private set; }

        public float sizeScale = .5f;
        public float bounciness = 0f;
        public float margin = 0.08f;

        public override void InitModule()
        {
            if (excludeBodies == null) excludeBodies = new Godot.Collections.Array();
        }

        public override void UpdateModule(float delta)
        {
            if (excludeBodies == null) excludeBodies = new Godot.Collections.Array();
        }

        public override void DrawInterface(Control parent)
        {
            Control sizeScaleField = moduleInterface.AddField("Size Scale");
            Control bouncinessField = moduleInterface.AddField("Bounciness");
            Control marginField = moduleInterface.AddField("Margin");

            SpinBox sizeScaleSpinBox = moduleInterface.AddSpinBox(sizeScaleField);
            SpinBox bouncinessSpinBox = moduleInterface.AddSpinBox(bouncinessField);
            SpinBox marginSpinBox = moduleInterface.AddSpinBox(marginField);

            sizeScaleSpinBox.MinValue = 0f;
            sizeScaleSpinBox.MaxValue = 1f;
            sizeScaleSpinBox.Step = 0.01f;
            sizeScaleSpinBox.Value = sizeScale;

            bouncinessSpinBox.MinValue = 0f;
            bouncinessSpinBox.MaxValue = 1f;
            bouncinessSpinBox.Step = 0.01f;
            bouncinessSpinBox.Value = bounciness;

            marginSpinBox.MinValue = 0f;
            marginSpinBox.MaxValue = 1f;
            marginSpinBox.Step = 0.01f;
            marginSpinBox.Value = margin;

            sizeScaleSpinBox.Connect("value_changed", this, "OnSizeScaleValueChanged");
            bouncinessSpinBox.Connect("value_changed", this, "OnBouncinessValueChanged");
            marginSpinBox.Connect("value_changed", this, "OnMarginValueChanged");
        }

        public override string GetModuleName()
        {
            return "Collision";
        }

        public override Godot.Collections.Dictionary SerializeModule()
        {
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();
            data.Add("sizeScale", sizeScale);
            data.Add("bounciness", bounciness);
            data.Add("margin", margin);

            return data;
        }

        public override void UnSerializeModule(Godot.Collections.Dictionary data)
        {
            sizeScale = (float)data["sizeScale"];
            bounciness = (float)data["bounciness"];
            margin = (float)data["margin"];
        }

        public void OnSizeScaleValueChanged(float value)
        {
            moduleInterface.undoRedo.CreateAction("Set Option Size Scale");
            moduleInterface.undoRedo.AddDoProperty(this, "sizeScale", value);
            moduleInterface.undoRedo.AddDoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoProperty(this, "sizeScale", sizeScale);
            moduleInterface.undoRedo.AddUndoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoMethod(moduleInterface, "UpdateInterface");
            moduleInterface.undoRedo.CommitAction();
        }

        public void OnBouncinessValueChanged(float value)
        {
            moduleInterface.undoRedo.CreateAction("Set Option Bounciness");
            moduleInterface.undoRedo.AddDoProperty(this, "bounciness", value);
            moduleInterface.undoRedo.AddDoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoProperty(this, "bounciness", bounciness);
            moduleInterface.undoRedo.AddUndoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoMethod(moduleInterface, "UpdateInterface");
            moduleInterface.undoRedo.CommitAction();
        }

        public void OnMarginValueChanged(float value)
        {
            moduleInterface.undoRedo.CreateAction("Set Option Margin");
            moduleInterface.undoRedo.AddDoProperty(this, "margin", value);
            moduleInterface.undoRedo.AddDoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoProperty(this, "margin", margin);
            moduleInterface.undoRedo.AddUndoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoMethod(moduleInterface, "UpdateInterface");
            moduleInterface.undoRedo.CommitAction();
        }

        public void UpdateParticle(ref Particle particle, float delta)
        {
            Vector2 dir = particle.velocity.Normalized();
            float l = particle.velocity.Length() * delta + (particle.size * .5f * sizeScale + 0.08f);
            var res = spaceState.IntersectRay(particle.position, particle.position + dir * l, excludeBodies);

            if (res.Count == 0) return;

            Vector2 point = (Vector2)res["position"];
            Vector2 normal = (Vector2)res["normal"];
            normal = normal.Normalized();

            if (!normal.IsNormalized()) return;

            particle.position = point + normal * (particle.size * .5f * sizeScale + 0.08f);
            Vector2 slide = particle.velocity.Slide(normal);
            Vector2 bounce = particle.velocity.Bounce(normal);
            particle.velocity = slide.LinearInterpolate(bounce, bounciness);
        }

        public void DrawParticle(Particle particle)
        {
            if (particleSystem.debugMode)
            {
                Vector2 from = particle.position;
                Vector2 to = from + particle.velocity * (1f / 60f);
                particleSystem.DrawLine(from, to, Colors.Green);
            }
        }
    }
}