using Godot;
using System;

namespace ParticleSystem2DPlugin
{
    [Tool]
    public class RandomNumberEditor : HBoxContainer
    {
        public enum Mode
        {
            Constant,
            Random
        }

        private float _step { get; set; }
        private Mode _mode { get; set; }

        private float _minValue, _maxValue;
        private bool _allowGreater, _allowLesser;

        private Control random { get; set; }
        private SpinBox constant { get; set; }
        private SpinBox min { get; set; }
        private SpinBox max { get; set; }
        private OptionButton modeButton { get; set; }

        [Export]
        public float minValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                if (_minValue != value)
                {
                    _minValue = value;
                    UpdateNodes();
                }
            }
        }
        [Export]
        public float maxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                if (_maxValue != value)
                {
                    _maxValue = value;
                    UpdateNodes();
                }
            }
        }
        [Export]
        public float step
        {
            get
            {
                return _step;
            }
            set
            {
                if (_step != value)
                {
                    _step = value;
                    UpdateNodes();
                }
            }
        }
        [Export]
        public Mode mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    UpdateNodes();
                }
            }
        }
        [Export]
        public bool allowGreater {
            get {
                return _allowGreater;
            }
            set {
                if (_allowGreater != value) {
                    _allowGreater = value;
                    UpdateNodes();
                }
            }
        }
        [Export]
        public bool allowLesser {
            get {
                return _allowLesser;
            }
            set {
                if (_allowLesser != value) {
                    _allowLesser = value;
                    UpdateNodes();
                }
            }
        }

        [Signal]
        delegate void Changed(float min, float max, Mode mode);

        public RandomNumberEditor()
        {
            _step = .01f;
        }

        public override void _EnterTree()
        {
            random = GetNode<Control>("Random");
            constant = GetNode<SpinBox>("Constant");
            min = GetNode<SpinBox>("Random/Min");
            max = GetNode<SpinBox>("Random/Max");
            modeButton = GetNode<OptionButton>("Mode");

            constant.Connect("value_changed", this, "OnMinValueChanged");
            constant.Connect("value_changed", this, "OnMaxValueChanged");
            min.Connect("value_changed", this, "OnMinValueChanged");
            max.Connect("value_changed", this, "OnMaxValueChanged");
            modeButton.Connect("item_selected", this, "OnModeSelected");

            UpdateNodes();
        }

        public void OnMinValueChanged(float value)
        {
            _minValue = value;
            if (_minValue > _maxValue)
            {
                _maxValue = _minValue;
                UpdateNodes();
            }
            EmitSignal("Changed", _minValue, _maxValue, mode);
        }

        public void OnMaxValueChanged(float value)
        {
            _maxValue = value;
            if (_maxValue < _minValue)
            {
                _minValue = _maxValue;
                UpdateNodes();
            }
            EmitSignal("Changed", _minValue, _maxValue, mode);
        }

        public void OnModeSelected(int mode)
        {
            this.mode = (Mode)mode;
            if (mode == (int)Mode.Constant)
            {
                _maxValue = _minValue;
            }
            UpdateNodes();
            EmitSignal("Changed", _minValue, _maxValue, mode);
        }

        public void UpdateNodes()
        {
            if (constant == null || min == null || max == null || random == null) return;

            constant.AllowGreater = allowGreater;
            min.AllowGreater = allowGreater;
            max.AllowGreater = allowGreater;

            constant.AllowLesser = allowLesser;
            min.AllowLesser = allowLesser;
            max.AllowLesser = allowLesser;

            constant.Value = _minValue;
            min.Value = _minValue;
            max.Value = _maxValue;
            modeButton.Selected = (int)mode;

            constant.Step = step;
            min.Step = step;
            max.Step = step;

            switch (mode)
            {
                case Mode.Constant:
                    {
                        random.Hide();
                        constant.Show();
                        break;
                    }
                case Mode.Random:
                    {
                        random.Show();
                        constant.Hide();
                        break;
                    }
            }
        }
    }
}