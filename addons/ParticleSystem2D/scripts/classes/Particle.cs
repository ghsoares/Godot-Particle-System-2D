using Godot;

namespace ParticleSystem2DPlugin
{
    public struct Particle
    {
        public int idx { get; set; }

        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }
        public float lifetime { get; set; }
        public float currentLife { get; set; }
        public bool alive { get; set; }

        public Color baseColor { get; set; }
        public Color color { get; set; }

        public float baseSize { get; set; }
        public float size { get; set; }
        public float rotation { get; set; }

        public Rect2 drawRect
        {
            get
            {
                Rect2 r = new Rect2();
                r.Position = position - Vector2.One * size;
                r.Size = Vector2.One * size * 2f;
                return r;
            }
        }
        public float life
        {
            get
            {
                return Mathf.Clamp(currentLife / lifetime, 0, 1);
            }
        }
    }
}