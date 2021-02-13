using Godot;
using System.Collections.Generic;

namespace ParticleSystem2DPlugin
{
    [Tool]
    public class ParticleSystemInterface : Control
    {
        public EditorInterface editorInterface { get; set; }
        public Plugin plugin { get; set; }
        public UndoRedo undoRedo { get; set; }

        private EditorSelection sceneTreeSelection { get; set; }
        private Control noParticleSystem { get; set; }
        private Control rootList { get; set; }
        private VBoxContainer moduleList { get; set; }
        private Button newModuleButton { get; set; }
        private PackedScene moduleInterfaceScene { get; set; }
        private PackedScene modulesContextMenuScene { get; set; }
        private PackedScene valueFieldScene { get; set; }
        public PackedScene randomNumberEditorScene { get; set; }

        private ParticleSystem2D editedParticleSystem;

        public override void _EnterTree()
        {
            if (editorInterface == null) return;

            rootList = GetNode<Control>("Scroll/VBox");

            noParticleSystem = GetNode<Control>("NoParticleSystem");
            moduleList = rootList.GetNode<VBoxContainer>("ModuleList");
            newModuleButton = rootList.GetNode<Button>("AddModule");
            moduleInterfaceScene = GD.Load<PackedScene>("res://addons/ParticleSystem2D/scenes/ModuleInterface.tscn");
            modulesContextMenuScene = GD.Load<PackedScene>("res://addons/ParticleSystem2D/scenes/ModulesContextMenu.tscn");
            valueFieldScene = GD.Load<PackedScene>("res://addons/ParticleSystem2D/scenes/ValueField.tscn");
            randomNumberEditorScene = GD.Load<PackedScene>("res://addons/ParticleSystem2D/scenes/RandomNumberEditor.tscn");

            sceneTreeSelection = editorInterface.GetSelection();
            if (!sceneTreeSelection.IsConnected("selection_changed", this, "OnSelectionChanged"))
            {
                sceneTreeSelection.Connect("selection_changed", this, "OnSelectionChanged");
            }
            if (!newModuleButton.IsConnected("pressed", this, "ModulesContextMenu"))
            {
                newModuleButton.Connect("pressed", this, "ModulesContextMenu");
            }

            OnSelectionChanged();
        }

        public void OnSelectionChanged()
        {
            var selected = sceneTreeSelection.GetSelectedNodes();
            editedParticleSystem = null;

            if (selected.Count == 1) editedParticleSystem = selected[0] as ParticleSystem2D;

            if (editedParticleSystem == null)
            {
                noParticleSystem.Show();
                rootList.Hide();
            }
            else
            {
                noParticleSystem.Hide();
                rootList.Show();
            }

            UpdateUI();
        }

        public void OnModulesContextMenuClosed()
        {
            newModuleButton.Disabled = false;
        }

        public void OnModulesContextMenuModuleChoosed(string typeName)
        {
            System.Type moduleType = System.Type.GetType(typeName);
            ParticleSystem2DModule module = (ParticleSystem2DModule)System.Activator.CreateInstance(moduleType);
            AddModule(module);
        }

        public void ModulesContextMenu()
        {
            newModuleButton.Disabled = true;
            ParticleModulesContextMenu contextMenu = modulesContextMenuScene.Instance() as ParticleModulesContextMenu;
            contextMenu.particleModules = new List<string>();

            foreach (ParticleSystem2DModule m in editedParticleSystem.modules)
            {
                contextMenu.particleModules.Add(m.GetType().ToString());
            }

            contextMenu.Connect("Close", this, "OnModulesContextMenuClosed");
            contextMenu.Connect("ModuleChoosed", this, "OnModulesContextMenuModuleChoosed");
            GetNode<Control>("ContextMenus").AddChild(contextMenu);
            contextMenu.RectGlobalPosition = GetGlobalMousePosition();
        }

        public void AddModule(ParticleSystem2DModule module) {
            editedParticleSystem.AddModule(module);
            editedParticleSystem.WriteModulesData();
            UpdateUI();
        }

        public void RemoveModule(ParticleSystem2DModule module)
        {
            editedParticleSystem.RemoveModule(module);
            UpdateUI();
        }

        public void OrderModule(ParticleSystem2DModule module, int add)
        {
            editedParticleSystem.ChangeModuleIndex(module, add);
            UpdateUI();
        }

        private void UpdateUI()
        {
            foreach (Node c in moduleList.GetChildren())
            {
                c.QueueFree();
            }

            if (editedParticleSystem == null) return;

            if (editedParticleSystem.modules.Count == 0)
            {
                moduleList.Hide();
            }
            else
            {
                moduleList.Show();
            }

            foreach (ParticleSystem2DModule module in editedParticleSystem.modules)
            {
                InstantiateModuleInterface(module);
            }
        }

        private void InstantiateModuleInterface(ParticleSystem2DModule module)
        {
            ParticleModuleInterface moduleInterface = moduleInterfaceScene.Instance() as ParticleModuleInterface;

            moduleInterface.module = module;
            moduleInterface.undoRedo = undoRedo;
            moduleInterface.Connect("QueryRemove", this, "RemoveModule");
            moduleInterface.Connect("QueryOrder", this, "OrderModule");

            moduleList.AddChild(moduleInterface);
        }
    }
}