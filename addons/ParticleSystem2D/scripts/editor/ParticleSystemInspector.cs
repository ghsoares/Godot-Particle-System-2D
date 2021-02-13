using Godot;

namespace ParticleSystem2DPlugin {
    [Tool]
    public class ParticleSystemInspector : EditorInspectorPlugin {
        public override bool CanHandle(Object obj) {
            return obj is ParticleSystem2D;
        }

        public override bool ParseProperty(Object obj, int type, string path, int hint, string hintText, int usage) {
            if (path == "modulesData") return true;
            return false;
        }
    }
}