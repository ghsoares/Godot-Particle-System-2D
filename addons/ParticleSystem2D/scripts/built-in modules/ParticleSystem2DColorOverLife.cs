using Godot;

namespace ParticleSystem2DPlugin {
    [Tool]
    [ParticleModulePath("Update/Over Life/Color")]
    public class ParticleSystem2DColorOverLife : ParticleSystem2DModule, IParticleUpdateable {
        public Gradient gradient;

        public override void InitModule() {
            if (gradient == null) {
                gradient = new Gradient();
            }
        }

        public override void DrawInterface(Control parent) {
            Control gradientField = moduleInterface.AddField("Gradient");

            GradientEditor gradientEditor = moduleInterface.AddGradientEditor(gradientField);

            gradientEditor.gradient = gradient;

            gradientEditor.Connect("GradientChanged", this, "OnGradientChanged");
        }

        public override string GetModuleName() {
            return "Color Over Life";
        }

        public override Godot.Collections.Dictionary SerializeModule() {
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();

            data["gradient"] = gradient;

            return data;
        }

        public override void UnSerializeModule(Godot.Collections.Dictionary data) {
            Gradient newGradient = (Gradient)data["gradient"];
            gradient = newGradient.Duplicate() as Gradient;
        }

        public void OnGradientChanged(Gradient newGradient) {
            this.gradient = newGradient;
            particleSystem.WriteModulesData();
        }

        public void UpdateParticle(ref Particle particle, float delta) {
            if (gradient == null) return;
            particle.color = particle.baseColor * gradient.Interpolate(1f - particle.life);
        }
    }
}