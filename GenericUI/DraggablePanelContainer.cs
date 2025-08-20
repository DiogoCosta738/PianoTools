using Godot;
using System;

public partial class DraggablePanelContainer : Panel
{
    [Export] Control parent;
    bool mouseIn, mouseInStrict;
    bool isDragging;
    Vector2 prevMouse;
    StyleBoxFlat sbNormal, sbActive;

    public override void _Ready()
    {
        base._Ready();

        float grayVal = 0.15f;
        sbNormal = new StyleBoxFlat();
        sbNormal.BgColor = new Color(0, 0, 0, 0.5f);

        sbActive = new StyleBoxFlat();
        sbActive.BgColor = new Color(grayVal, grayVal, grayVal, 0.95f);

        MouseEntered += () => { mouseInStrict = true; UpdateVisuals(); };
        MouseExited += () => { mouseInStrict = false; UpdateVisuals(); };
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseInStrict && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                isDragging = true;
                prevMouse = GetViewport().GetMousePosition();
                GetViewport().SetInputAsHandled();
                UpdateVisuals();
            }

            if (!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left && isDragging)
            {
                isDragging = false;
                GetViewport().SetInputAsHandled();
                UpdateVisuals();
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var hovered = GetViewport().GuiGetHoveredControl();
        if (mouseIn != (hovered != null && IsAncestorOf(hovered)))
        {
            mouseIn ^= true;
            UpdateVisuals();
        }

        if (isDragging)
        {
            parent.Position += GetViewport().GetMousePosition() - prevMouse;
        }
        prevMouse = GetViewport().GetMousePosition();
    }

    public virtual void UpdateVisuals()
    {
        if (isDragging || mouseIn)
        {
            AddThemeStyleboxOverride("panel", sbActive);
        }
        else
        {
            AddThemeStyleboxOverride("panel", sbNormal);
        }
    }
}
