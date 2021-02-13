using Godot;

namespace ParticleSystem2DPlugin
{
    [Tool]
    [ParticleModulePath("Update/Update Particle")]
    public class ParticleSystem2DUpdateModule : ParticleSystem2DModule, IParticleUpdateable
    {
        public void UpdateParticle(ref Particle particle, float delta)
        {
            particle.currentLife -= delta;
            particle.currentLife = Mathf.Max(particle.currentLife, 0);
            particle.velocity += particleSystem.gravity * delta;
            particle.position += particle.velocity * delta;
        }

        public override string GetModuleName()
        {
            return "Update Particle";
        }
    }
}