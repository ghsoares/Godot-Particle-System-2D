using Godot;

namespace ParticleSystem2DPlugin
{
    [Tool]
    [ParticleModulePath("Draw/Draw Batch")]
    public class ParticleSystem2DDrawBatchModule : ParticleSystem2DModule, IParticleBatchDrawable
    {
        private MultiMesh multimesh { get; set; }
        private Transform2D ZERO_TRANSFORM = Transform2D.Identity.Scaled(Vector2.Zero);
        private DrawMode _drawMode = DrawMode.SingleTriangle;

        public enum DrawMode
        {
            Quad,
            SingleTriangle
        }

        public Texture tex;
        public Texture normalMap;
        public Mesh mesh;
        public Material material;
        public DrawMode drawMode
        {
            get
            {
                return _drawMode;
            }
            set
            {
                if (_drawMode != value)
                {
                    _drawMode = value;
                    Reset();
                }
            }
        }

        public override void InitModule()
        {
            Reset();
        }

        public override Godot.Collections.Dictionary SerializeModule()
        {
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();

            data["tex"] = tex;
            data["normalMap"] = normalMap;
            data["mesh"] = mesh;
            data["material"] = material;

            return data;
        }

        public override void UnSerializeModule(Godot.Collections.Dictionary data)
        {
            tex = (Texture)data["tex"];
            normalMap = (Texture)data["normalMap"];
            mesh = (Mesh)data["mesh"];
            material = (Material)data["material"];
        }

        private void Reset()
        {
            Mesh usedMesh = mesh;

            if (usedMesh == null)
            {
                SurfaceTool st = new SurfaceTool();

                st.Begin(Mesh.PrimitiveType.Triangles);

                switch (drawMode)
                {
                    case DrawMode.SingleTriangle:
                        {
                            st.AddUv(new Vector2(0, 0));
                            st.AddVertex(new Vector3(-.5f, -.5f, 0));

                            st.AddUv(new Vector2(0, 2));
                            st.AddVertex(new Vector3(-.5f, 1.5f, 0));

                            st.AddUv(new Vector2(2, 0));
                            st.AddVertex(new Vector3(1.5f, -.5f, 0));
                            break;
                        }
                    case DrawMode.Quad:
                        {
                            st.AddUv(new Vector2(0, 0));
                            st.AddVertex(new Vector3(-.5f, -.5f, 0));

                            st.AddUv(new Vector2(0, 1));
                            st.AddVertex(new Vector3(-.5f, .5f, 0));

                            st.AddUv(new Vector2(1, 0));
                            st.AddVertex(new Vector3(.5f, .51f, 0));

                            st.AddUv(new Vector2(1, 0));
                            st.AddVertex(new Vector3(.5f, -.5f, 0));

                            st.AddUv(new Vector2(0, 1));
                            st.AddVertex(new Vector3(-.5f, .5f, 0));

                            st.AddUv(new Vector2(1, 1));
                            st.AddVertex(new Vector3(.5f, .5f, 0));
                            break;
                        }
                }

                //st.SetMaterial(particleSystem.testMaterial);
                st.Index();

                usedMesh = st.Commit();
            }

            multimesh = new MultiMesh();
            multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform2d;
            multimesh.ColorFormat = MultiMesh.ColorFormatEnum.Float;
            //multimesh.CustomDataFormat = MultiMesh.CustomDataFormatEnum.Float;
            multimesh.Mesh = usedMesh;
            multimesh.InstanceCount = particleSystem.maxParticles;
        }

        private void InitMultiMeshIfNeeded()
        {
            if (multimesh == null || multimesh.InstanceCount != particleSystem.maxParticles) Reset();
        }

        public void DrawBatch(Particle[] particles)
        {
            InitMultiMeshIfNeeded();

            foreach (Particle p in particles)
            {
                if (p.alive)
                {
                    Transform2D t = new Transform2D();
                    t.origin = p.position;

                    t.x = Vector2.Right.Rotated(Mathf.Deg2Rad(p.rotation)) * p.size;
                    t.y = Vector2.Up.Rotated(Mathf.Deg2Rad(p.rotation)) * p.size;

                    multimesh.SetInstanceTransform2d(p.idx, t);
                    multimesh.SetInstanceColor(p.idx, p.color);
                }
                else
                {
                    multimesh.SetInstanceTransform2d(p.idx, ZERO_TRANSFORM);
                }
            }

            particleSystem.DrawRect(new Rect2(Vector2.Zero, Vector2.Zero), Colors.White); // Fixes weird mesh color
            particleSystem.DrawMultimesh(multimesh, tex, normalMap);
        }

        public override string GetModuleName()
        {
            return "Draw Batch";
        }
    }
}