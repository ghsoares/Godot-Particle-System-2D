#if TOOLS
using Godot;
using System;

namespace ParticleSystem2DPlugin {
    [Tool]
    public class Plugin : EditorPlugin
    {
        const string ParticleSystemInterfacePath = "res://addons/ParticleSystem2D/scenes/ParticleSystemInterface.tscn";
        const string ParticleSystem2DScriptPath = "res://addons/ParticleSystem2D/scripts/nodes/ParticleSystem2D.cs";
        const string ParticleSystem2DIconPath = "res://addons/ParticleSystem2D/icons/ParticleSystem2DIcon.svg";

        ParticleSystemInterface dock;
        ParticleSystemInspector inspector;

        public override void _EnterTree() {
            Script particleSystem2DScript = GD.Load<Script>(ParticleSystem2DScriptPath);
            Texture particleSystem2DTexture = GD.Load<Texture>(ParticleSystem2DIconPath);
            inspector = new ParticleSystemInspector();

            /*
            plugin = preload("res://addons/MyPlugin/MyInspectorPlugin.gd").new()
            add_inspector_plugin(plugin)
            */

            dock = GD.Load<PackedScene>(ParticleSystemInterfacePath).Instance() as ParticleSystemInterface;
            dock.plugin = this;
            dock.editorInterface = GetEditorInterface();
            dock.undoRedo = GetUndoRedo();

            AddCustomType("ParticleSystem2D", "Node2D", particleSystem2DScript, particleSystem2DTexture);
            AddControlToDock(DockSlot.LeftUl, dock);
            AddInspectorPlugin(inspector);
        }

        public override void _ExitTree() {
            RemoveCustomType("ParticleSystem2D");
            RemoveControlFromDocks(dock);
            RemoveInspectorPlugin(inspector);
            dock.Free();
        }
    }
}
#endif