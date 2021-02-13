using Godot;

namespace ParticleSystem2DPlugin
{
    [Tool]
    [ParticleModulePath("Emit/Rate")]
    public class ParticleSystem2DEmitRateModule : ParticleSystem2DModule
    {
        private float currentRate { get; set; }

        public float rate = 8f;

        public override void DrawInterface(Control parent)
        {
            Control field = moduleInterface.AddField("Rate");

            SpinBox rateEditor = moduleInterface.AddSpinBox(field);

            rateEditor.MinValue = 0f;
            rateEditor.MaxValue = 100f;
            rateEditor.Step = .01f;
            rateEditor.AllowGreater = true;
            rateEditor.Value = rate;

            rateEditor.Connect("value_changed", this, "OnRateValueChanged");
        }

        public override Godot.Collections.Dictionary SerializeModule()
        {
            Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();

            data["rate"] = rate;

            return data;
        }

        public override void UnSerializeModule(Godot.Collections.Dictionary data)
        {
            rate = (float)data["rate"];
        }

        public void OnRateValueChanged(float value)
        {
            moduleInterface.undoRedo.CreateAction("Set Rate");
            moduleInterface.undoRedo.AddDoProperty(this, "rate", value);
            moduleInterface.undoRedo.AddDoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoProperty(this, "rate", rate);
            moduleInterface.undoRedo.AddUndoMethod(particleSystem, "WriteModulesData");
            moduleInterface.undoRedo.AddUndoMethod(moduleInterface, "UpdateInterface");
            moduleInterface.undoRedo.CommitAction();
        }

        public override void UpdateModule(float delta)
        {
            if (!particleSystem.emitting)
            {
                currentRate = 0f;
                return;
            }
            currentRate += delta * rate;
            while (currentRate >= 1f)
            {
                particleSystem.Emit(1);
                currentRate -= 1f;
            }
        }

        public override string GetModuleName()
        {
            return "Emit Rate";
        }
    }
}