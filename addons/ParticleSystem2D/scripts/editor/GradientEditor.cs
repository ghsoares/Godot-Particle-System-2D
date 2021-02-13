using Godot;
using System;

namespace ParticleSystem2DPlugin
{
    [Tool]
    public class GradientEditor : Control
    {
        [Export]
        public Gradient gradient { get; set; }

        const float HANDLE_SIZE = 6f;
        const int TEXTURE_SIZE = 256;

        private ColorRect preview { get; set; }
        private ColorPickerButton selectedColor { get; set; }
        private ShaderMaterial previewMaterial { get; set; }
        private int selectedIdx { get; set; }
        private int dragIdx { get; set; }

        [Signal]
        delegate void GradientChanged(Gradient gradient);

        public override void _EnterTree()
        {
            if (gradient == null)
            {
                gradient = new Gradient();
            }

            Shader shader = GD.Load<Shader>("res://addons/ParticleSystem2D/shaders/Gradient.shader");
            previewMaterial = new ShaderMaterial();
            previewMaterial.Shader = shader;

            preview = GetNode<ColorRect>("Preview");
            selectedColor = GetNode<ColorPickerButton>("Color");
            preview.Material = previewMaterial;

            dragIdx = -1;

            selectedColor.Connect("color_changed", this, "ColorChanged");
            gradient.Connect("changed", this, "OnGradientChanged");

            UpdateMaterial();
        }

        public override void _GuiInput(InputEvent ev)
        {
            if (!(ev is InputEventMouse)) return;

            if (ev is InputEventMouseMotion)
            {
                InputEventMouseMotion evM = ev as InputEventMouseMotion;

                if (dragIdx != -1)
                {
                    Vector2 mousePos = evM.Position;
                    Vector2 relMousePos = (mousePos - preview.RectPosition) / preview.RectSize;

                    relMousePos.x = Mathf.Clamp(relMousePos.x, 0, 1);

                    gradient.SetOffset(dragIdx, relMousePos.x);
                }
            }

            if (ev is InputEventMouseButton)
            {
                InputEventMouseButton evMB = ev as InputEventMouseButton;

                Vector2 mousePos = evMB.Position;
                Vector2 relMousePos = (mousePos - preview.RectPosition) / preview.RectSize;

                int handleIdx = -1;

                for (int i = 0; i < gradient.Colors.Length; i++)
                {
                    float off = gradient.Offsets[i] * preview.RectSize.x;
                    float min = off - HANDLE_SIZE / 2f;
                    float max = off + HANDLE_SIZE / 2f;
                    if (mousePos.x >= min && mousePos.x <= max)
                    {
                        handleIdx = i;
                        break;
                    }
                }

                if (evMB.ButtonIndex == (int)ButtonList.Left)
                {
                    if (evMB.Pressed)
                    {
                        if (handleIdx == -1)
                        {
                            float off = relMousePos.x;
                            Color c = gradient.Interpolate(off);
                            gradient.AddPoint(off, c);
                            selectedIdx = gradient.GetPointCount() - 1;
                            dragIdx = selectedIdx;
                        }
                        else
                        {
                            selectedIdx = handleIdx;
                            dragIdx = handleIdx;
                        }
                    }
                    else
                    {
                        dragIdx = -1;
                    }
                }
                if (evMB.ButtonIndex == (int)ButtonList.Right)
                {
                    if (evMB.Pressed)
                    {
                        if (handleIdx != -1 && gradient.Colors.Length > 2)
                        {
                            gradient.RemovePoint(handleIdx);
                            if (handleIdx == selectedIdx) {
                                selectedIdx = 0;
                            }
                        }
                    }
                }
            }

            UpdateMaterial();
        }

        public void OnGradientChanged() {
            UpdateMaterial();
            EmitSignal("GradientChanged", gradient);
        }

        private void UpdateMaterial()
        {
            selectedColor.Color = gradient.Colors[selectedIdx];

            Image img = new Image();
            img.Create(TEXTURE_SIZE, 1, false, Image.Format.Rgbaf);

            img.Lock();
            for (int i = 0; i < TEXTURE_SIZE; i++)
            {
                float t = i / (float)(TEXTURE_SIZE - 1);

                img.SetPixel(i, 0, gradient.Interpolate(t));
            }
            img.Unlock();

            ImageTexture imgTex = new ImageTexture();
            imgTex.CreateFromImage(img, 0);

            previewMaterial.SetShaderParam("gradient", imgTex);
        }

        public override void _Process(float delta)
        {
            Update();
        }

        public override void _Draw()
        {
            for (int i = 0; i < gradient.Colors.Length; i++)
            {
                float off = gradient.Offsets[i];

                Rect2 r = new Rect2();
                r.Position = new Vector2(
                    preview.RectSize.x * off - HANDLE_SIZE / 2f, 0f
                );
                r.Size = new Vector2(
                    HANDLE_SIZE, preview.RectSize.y
                );


                if (i == selectedIdx)
                {
                    DrawRect(r, Colors.White, true);
                }
                else
                {
                    DrawRect(r, new Color(.5f, .5f, .5f), true);
                }
                DrawRect(r, Colors.Black, false, 1f);
            }
        }

        public void ColorChanged(Color newColor)
        {
            gradient.SetColor(selectedIdx, newColor);
        }
    }
}