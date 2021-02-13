using Godot;
using System.Collections.Generic;

namespace ParticleSystem2DPlugin
{
    [Tool]
    public class ParticleModuleInterface : VBoxContainer
    {
        private Button openCloseButton { get; set; }
        private TextureButton orderUpButton { get; set; }
        private TextureButton orderDownButton { get; set; }
        private Button removeButton { get; set; }
        private Control contents { get; set; }
        private Control contentsList { get; set; }
        private CheckBox enabledCheckbox { get; set; }
        private Label moduleNameLabel { get; set; }
        private Control disabledControl { get; set; }
        private List<Node> contentsToAdd { get; set; }

        private PackedScene valueFieldScene { get; set; }
        private PackedScene randomNumberScene { get; set; }
        private PackedScene gradientScene { get; set; }
        private PackedScene curveScene { get; set; }

        public ParticleSystem2DModule module { get; set; }
        public UndoRedo undoRedo { get; set; }

        [Signal]
        delegate void QueryRemove(ParticleSystem2DModule module);
        [Signal]
        delegate void QueryOrder(ParticleSystem2DModule module, int add);

        public override void _EnterTree()
        {
            if (module == null) return;

            valueFieldScene = GD.Load<PackedScene>("res://addons/ParticleSystem2D/scenes/ValueField.tscn");
            randomNumberScene = GD.Load<PackedScene>("res://addons/ParticleSystem2D/scenes/RandomNumberEditor.tscn");
            gradientScene = GD.Load<PackedScene>("res://addons/ParticleSystem2D/scenes/GradientEditor.tscn");
            curveScene = GD.Load<PackedScene>("res://addons/ParticleSystem2D/scenes/CurveEditor.tscn");

            openCloseButton = GetNode<Button>("Top");
            orderUpButton = GetNode<TextureButton>("Top/HBox/Order/Up");
            orderDownButton = GetNode<TextureButton>("Top/HBox/Order/Down");
            removeButton = GetNode<Button>("Top/HBox/Remove");
            contents = GetNode<Control>("Contents");
            contentsList = GetNode<Control>("Contents/List");
            enabledCheckbox = GetNode<CheckBox>("Top/HBox/Enabled");
            moduleNameLabel = GetNode<Label>("Top/HBox/ModuleName");
            disabledControl = GetNode<Control>("Contents/Disabled");

            openCloseButton.Connect("pressed", this, "OnOpenClosePressed");
            orderUpButton.Connect("pressed", this, "OnOrderPressed", new Godot.Collections.Array() { -1 });
            orderDownButton.Connect("pressed", this, "OnOrderPressed", new Godot.Collections.Array() { 1 });
            removeButton.Connect("pressed", this, "OnRemovePressed");
            enabledCheckbox.Connect("toggled", this, "OnEnabledToggled");

            UpdateInterface();
        }

        public void UpdateInterface()
        {
            moduleNameLabel.Text = module.GetModuleName();

            enabledCheckbox.Pressed = module.enabled;
            disabledControl.Visible = !module.enabled;

            if (module.enabled)
            {
                contents.Modulate = Colors.White;
            }
            else
            {
                contents.Modulate = new Color(.5f, .5f, .5f);
            }

            if (module.uiHidden)
            {
                contents.Hide();
                return;
            }
            else
            {
                contents.Show();
            }

            foreach (Node c in contentsList.GetChildren())
            {
                c.QueueFree();
            }

            module.moduleInterface = this;

            contentsToAdd = new List<Node>();

            module.DrawInterface(contentsList);

            foreach (Node node in contentsToAdd)
            {
                contentsList.AddChild(node);
            }
        }

        public Control AddField(string name)
        {
            Control newField = valueFieldScene.Instance() as Control;

            newField.GetNode<Label>("Label").Text = name;

            contentsToAdd.Add(newField);
            return newField;
        }

        public RandomNumberEditor AddRandomNumberEditor(Control field)
        {
            RandomNumberEditor editor = randomNumberScene.Instance() as RandomNumberEditor;
            AddEditor(field, editor);
            return editor;
        }

        public GradientEditor AddGradientEditor(Control field)
        {
            GradientEditor editor = gradientScene.Instance() as GradientEditor;
            AddEditor(field, editor);
            return editor;
        }

        public CurveEditor AddCurveEditor(Control field)
        {
            CurveEditor editor = curveScene.Instance() as CurveEditor;
            AddEditor(field, editor);
            return editor;
        }

        public ColorPickerButton AddColorEditor(Control field)
        {
            ColorPickerButton editor = new ColorPickerButton();
            AddEditor(field, editor);
            return editor;
        }

        public SpinBox AddSpinBox(Control field)
        {
            SpinBox editor = new SpinBox();
            AddEditor(field, editor);
            return editor;
        }

        public void AddEditor(Control field, Control editor)
        {
            field.GetNode<Control>("VBox").AddChild(editor);
            editor.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
        }

        public void OnOpenClosePressed()
        {
            module.uiHidden = !module.uiHidden;
            UpdateInterface();
        }

        public void OnOrderPressed(int add)
        {
            EmitSignal("QueryOrder", module, add);
        }

        public void OnRemovePressed()
        {
            EmitSignal("QueryRemove", module);
        }

        public void OnEnabledToggled(bool toggled)
        {
            module.enabled = toggled;
            UpdateInterface();
        }
    }
}