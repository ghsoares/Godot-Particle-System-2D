using Godot;
using System;

namespace ParticleSystem2DPlugin
{
    [Tool]
    public class CurveEditor : Panel
    {
        const float MARGIN = 12f;
        const float HANDLE_SIZE = 8f;
        const float TANGENT_HANDLE_SPACING = 24f;
        const float TANGENT_HANDLE_SIZE = 6f;
        const int CURVE_RESOLUTION = 32;
        Font defaultFont;

        int hoverIdx = -1;
        int selectedIdx = -1;
        int dragIdx = -1;

        int hoverTangentIdx = 0;
        int dragTangentIdx = 0;

        public Curve curve;

        [Signal]
        delegate void CurveChanged(Curve curve);

        public override void _EnterTree()
        {
            if (curve == null)
            {
                curve = new Curve();
                curve.AddPoint(Vector2.Zero, 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
                curve.AddPoint(Vector2.One, 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
                EmitSignal("CurveChanged", curve);
            }
            curve.Connect("changed", this, "OnCurveChanged");
            defaultFont = new Control().GetFont("font");
        }

        public void OnCurveChanged()
        {
            Update();
            EmitSignal("CurveChanged", curve);
        }

        private Vector2 GetHandlePos(int i)
        {
            Vector2 handlePos = curve.GetPointPosition(i);
            handlePos.x = Mathf.Lerp(MARGIN, RectSize.x - MARGIN, handlePos.x);
            handlePos.y = Mathf.Lerp(MARGIN, RectSize.y - MARGIN, 1f - handlePos.y);
            return handlePos;
        }

        private Vector2 GetHandleLeftTangent(int i)
        {
            Vector2 handlePos = GetHandlePos(i);
            Vector2 dir = -new Vector2(1, -curve.GetPointLeftTangent(i));
            Vector2 tanPos = handlePos + dir.Normalized() * TANGENT_HANDLE_SPACING;
            return tanPos;
        }

        private Vector2 GetHandleRightTangent(int i)
        {
            Vector2 handlePos = GetHandlePos(i);
            Vector2 dir = new Vector2(1, -curve.GetPointRightTangent(i));
            Vector2 tanPos = handlePos + dir.Normalized() * TANGENT_HANDLE_SPACING;
            return tanPos;
        }

        public override void _GuiInput(InputEvent ev)
        {
            if (!(ev is InputEventMouse)) return;

            InputEventMouse evM = (InputEventMouse)ev;

            Vector2 mousePos = evM.Position;
            Vector2 relCurvePos = mousePos;
            relCurvePos.x = Mathf.InverseLerp(MARGIN, RectSize.x - MARGIN, relCurvePos.x);
            relCurvePos.y = Mathf.InverseLerp(RectSize.y - MARGIN, MARGIN, relCurvePos.y);

            relCurvePos.x = Mathf.Clamp(relCurvePos.x, 0, 1);
            relCurvePos.y = Mathf.Clamp(relCurvePos.y, 0, 1);

            int handleIdx = -1;
            int handleTangentIdx = 0;

            int pointCount = curve.GetPointCount();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 handlePos = GetHandlePos(i);
                Rect2 r = new Rect2();

                r.Position = handlePos - Vector2.One * (HANDLE_SIZE / 2f);
                r.Size = Vector2.One * HANDLE_SIZE;

                if (r.HasPoint(mousePos))
                {
                    handleIdx = i;
                }

                if (i > 0)
                {
                    Vector2 tangentPos = GetHandleLeftTangent(i);
                    r.Position = tangentPos - Vector2.One * (TANGENT_HANDLE_SIZE / 2f);
                    r.Size = Vector2.One * TANGENT_HANDLE_SIZE;

                    if (r.HasPoint(mousePos)) handleTangentIdx = -1;
                }
                if (i < pointCount - 1)
                {
                    Vector2 tangentPos = GetHandleRightTangent(i);
                    r.Position = tangentPos - Vector2.One * (TANGENT_HANDLE_SIZE / 2f);
                    r.Size = Vector2.One * TANGENT_HANDLE_SIZE;

                    if (r.HasPoint(mousePos)) handleTangentIdx = 1;
                }
            }

            if (ev is InputEventMouseButton)
            {
                InputEventMouseButton evMB = ev as InputEventMouseButton;

                if (evMB.ButtonIndex == (int)ButtonList.Left)
                {
                    if (evMB.Pressed)
                    {
                        if (handleTangentIdx != 0)
                        {
                            dragTangentIdx = handleTangentIdx;
                        }
                        else
                        {
                            if (handleIdx == -1)
                            {
                                handleIdx = curve.AddPoint(relCurvePos);
                                selectedIdx = handleIdx;
                                dragIdx = handleIdx;
                            }
                            else
                            {
                                selectedIdx = handleIdx;
                                dragIdx = handleIdx;
                            }
                        }
                    }
                    else
                    {
                        dragTangentIdx = 0;
                        dragIdx = -1;
                    }
                }
                if (evMB.ButtonIndex == (int)ButtonList.Right)
                {
                    if (evMB.Pressed)
                    {
                        if (handleIdx != -1)
                        {
                            curve.RemovePoint(handleIdx);
                            if (selectedIdx == handleIdx) selectedIdx = -1;
                            handleIdx = -1;
                        }
                    }
                }
            }
            if (ev is InputEventMouseMotion)
            {
                if (dragIdx != -1)
                {
                    curve.SetPointOffset(dragIdx, relCurvePos.x);
                    curve.SetPointValue(dragIdx, relCurvePos.y);
                }
                if (dragTangentIdx != 0)
                {
                    Vector2 handlePos = GetHandlePos(selectedIdx);
                    Vector2 dir = (mousePos - handlePos).Normalized();

                    float tangent;
                    if (!Mathf.IsZeroApprox(dir.x))
                    {
                        tangent = dir.y / dir.x;
                    }
                    else
                    {
                        tangent = 9999f * (dir.y >= 0 ? 1 : -1);
                    }

                    bool link = !Input.IsKeyPressed((int)KeyList.Shift);

                    if (dragTangentIdx == 1)
                    {
                        curve.SetPointRightTangent(selectedIdx, -tangent);
                        link &= selectedIdx > 0 && curve.GetPointLeftMode(selectedIdx) != Curve.TangentMode.Linear;
                        if (link)
                        {
                            curve.SetPointLeftTangent(selectedIdx, -tangent);
                        }
                    }
                    else
                    {
                        curve.SetPointLeftTangent(selectedIdx, -tangent);
                        link &= selectedIdx < pointCount - 1 && curve.GetPointRightMode(selectedIdx) != Curve.TangentMode.Linear;
                        if (link)
                        {
                            curve.SetPointRightTangent(selectedIdx, -tangent);
                        }
                    }
                }
            }

            if (dragIdx != -1)
            {
                hoverIdx = dragIdx;
            }

            hoverIdx = handleIdx;
            hoverTangentIdx = handleTangentIdx;
            Update();
        }

        public override void _Draw()
        {
            // ---- Draws Grid -----
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Vector2 origin = new Vector2(
                    Mathf.Lerp(MARGIN, RectSize.x - MARGIN, t), 0
                );
                Vector2 to = origin + RectSize * Vector2.Down;
                DrawLine(origin, to, new Color(1f, 1f, 1f, .25f));
                DrawString(defaultFont, to, t.ToString(), new Color(1f, 1f, 1f, .25f));
            }
            for (int i = 0; i < 3; i++)
            {
                float t = i / 2f;
                Vector2 origin = new Vector2(
                    0, Mathf.Lerp(MARGIN, RectSize.y - MARGIN, t)
                );
                Vector2 to = origin + RectSize * Vector2.Right;
                DrawLine(origin, to, new Color(1f, 1f, 1f, .25f));
                DrawString(defaultFont, origin + Vector2.Right * MARGIN, (1f - t).ToString(), new Color(1f, 1f, 1f, .25f));
            }
            // ---------------------
            // ---- Draws Curve ----
            Vector2[] linePoints = new Vector2[CURVE_RESOLUTION];
            for (int i = 0; i < CURVE_RESOLUTION; i++)
            {
                float t = i / (float)(CURVE_RESOLUTION - 1);
                float x = Mathf.Lerp(MARGIN, RectSize.x - MARGIN, t);
                float y = Mathf.Lerp(MARGIN, RectSize.y - MARGIN, 1f - curve.Interpolate(t));

                linePoints[i] = new Vector2(x, y);
            }
            DrawPolyline(linePoints, new Color(1f, 1f, 1f, .5f));
            // ---------------------
            // --- Draws Handles ---
            int pointCount = curve.GetPointCount();
            for (int i = 0; i < pointCount; i++)
            {
                float t = i / (float)(pointCount - 1);
                Rect2 r = new Rect2();
                Vector2 handlePos = GetHandlePos(i);

                if (i == selectedIdx)
                {
                    if (i > 0)
                    {
                        Vector2 tanPos = GetHandleLeftTangent(i);
                        r.Position = tanPos - Vector2.One * (TANGENT_HANDLE_SIZE / 2f);
                        r.Size = Vector2.One * TANGENT_HANDLE_SIZE;
                        DrawLine(handlePos, tanPos, Colors.SkyBlue);
                        DrawRect(r, Colors.SkyBlue);
                    }
                    if (i < pointCount - 1)
                    {
                        Vector2 tanPos = GetHandleRightTangent(i);
                        r.Position = tanPos - Vector2.One * (TANGENT_HANDLE_SIZE / 2f);
                        r.Size = Vector2.One * TANGENT_HANDLE_SIZE;
                        DrawLine(handlePos, tanPos, Colors.SkyBlue);
                        DrawRect(r, Colors.SkyBlue);
                    }
                }

                r.Position = handlePos - Vector2.One * (HANDLE_SIZE / 2f);
                r.Size = Vector2.One * HANDLE_SIZE;
                if (i == selectedIdx)
                {
                    DrawRect(r, Colors.SkyBlue);
                }
                else
                {
                    DrawRect(r, Colors.White);
                }

                if (i == hoverIdx)
                {
                    r = r.Grow(3);
                    DrawRect(r, Colors.White, false);
                }
            }
            // ---------------------
        }
    }
}