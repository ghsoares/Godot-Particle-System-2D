using Godot;

namespace ParticleSystem2DPlugin
{
    [Tool]
    [ParticleModulePath("Update/Over Life/Size")]
    public class ParticleSystem2DSizeOverLifeModule : ParticleSystem2DModule, IParticleUpdateable {
        public Curve curve;
        public float sizeMultiplier = 1f;

        public override void InitModule() {
            if (curve == null) {
                curve = new Curve();
                curve.AddPoint(Vector2.Zero, 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
                curve.AddPoint(Vector2.One, 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
            }
        }

        public override void DrawInterface(Control parent) {
            Control sizeCurveField = moduleInterface.AddField("Curve");
            Control sizeMultiplierField = moduleInterface.AddField("Size Multiplier");

            CurveEditor sizeCurveEditor = moduleInterface.AddCurveEditor(sizeCurveField);
            SpinBox sizeMultiplierEditor = moduleInterface.AddSpinBox(sizeMultiplierField);

            sizeCurveEditor.curve = curve;

            sizeMultiplierEditor.MinValue = 0f;
            sizeMultiplierEditor.MaxValue = 2f;
            sizeMultiplierEditor.Step = 0.01f;
            sizeMultiplierEditor.AllowGreater = true;
            sizeMultiplierEditor.Value = sizeMultiplier;

            sizeCurveEditor.Connect("CurveChanged", this, "OnCurveChanged");
            sizeMultiplierEditor.Connect("value_changed", this, "OnSizeMultiplierValueChanged");
        }

        public override string GetModuleName() {
            return "Size Over Life";
        }

        public override Godot.Collections.Dictionary SerializeModule() {
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();

            data["curve"] = curve;
            data["sizeMultiplier"] = sizeMultiplier;

            return data;
        }

        public override void UnSerializeModule(Godot.Collections.Dictionary data) {
            Curve newCurve = (Curve)data["curve"];
            curve = new Curve();
            for (int i = 0; i < newCurve.GetPointCount(); i++) {
                curve.AddPoint(
                    newCurve.GetPointPosition(i), newCurve.GetPointLeftTangent(i),
                    newCurve.GetPointRightTangent(i), newCurve.GetPointLeftMode(i), newCurve.GetPointRightMode(i)
                );
            }
            sizeMultiplier = (float)data["sizeMultiplier"];
        }

        public void OnCurveChanged(Curve newCurve) {
            this.curve = newCurve;
            particleSystem.WriteModulesData();
        }

        public void OnSizeMultiplierValueChanged(float newValue) {
            moduleInterface.undoRedo.CreateAction("Set Option Size Multiplier");
            moduleInterface.undoRedo.AddDoProperty(this, "sizeMultiplier", newValue);
            moduleInterface.undoRedo.AddDoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoProperty(this, "sizeMultiplier", sizeMultiplier);
            moduleInterface.undoRedo.AddUndoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoMethod(moduleInterface, "UpdateInterface");
            moduleInterface.undoRedo.CommitAction();
        }

        public void UpdateParticle(ref Particle particle, float delta) {
            if (curve == null) return;
            float s = curve.Interpolate(1f - particle.life) * sizeMultiplier;
            particle.size = particle.baseSize * s;
        }
    }
}