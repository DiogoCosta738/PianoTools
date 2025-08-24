using Godot;
using System;
using System.Collections.Generic;

public partial class NoteHeadUI : Control
{
    [Export] TextureRect flatTexture;
    [Export] TextureRect sharpTexture;
    [Export] TextureRect headTexture;

    ColorRect debugRect1, debugRect2, debugRect3, debugRect4;

    public Vector2 flatPivot, sharpPivot, headPivot;
    Dictionary<string, TextureRect> accidentals;
    Dictionary<string, Vector2> accidentalPivots;

    public void InitTemplate()
    {
        flatPivot = flatTexture.PivotOffset / flatTexture.Size;
        sharpPivot = sharpTexture.PivotOffset / sharpTexture.Size;
        headPivot = headTexture.PivotOffset / headTexture.Size;

        flatTexture.PivotOffset = Vector2.Zero;
        sharpTexture.PivotOffset = Vector2.Zero;
        headTexture.PivotOffset = Vector2.Zero;
    }

    public override void _Ready()
    {
        base._Ready();
        accidentals = new Dictionary<string, TextureRect>()
        {
            { "", null },
            { "b", flatTexture } ,
            { "#", sharpTexture },
        };

        accidentalPivots = new Dictionary<string, Vector2>()
        {
            { "", Vector2.Zero },
            { "b", flatPivot } ,
            { "#", sharpPivot },
        };
    }

    public void CopyFrom(NoteHeadUI source)
    {
        flatPivot = source.flatPivot;
        sharpPivot = source.sharpPivot;
        headPivot = source.headPivot;

        accidentalPivots = new Dictionary<string, Vector2>()
        {
            { "", Vector2.Zero },
            { "b", flatPivot } ,
            { "#", sharpPivot },
        };
    }

    public float GetWidth()
    {
        return headTexture.Scale.X * headTexture.Size.X;
    }

    void SetPositionByPivot(Control control, Vector2 pos, Vector2 pivot)
    {
        control.Position = pos - new Vector2(pivot.X * control.Size.X * control.Scale.X, pivot.Y * control.Size.Y * control.Scale.Y);
    }

    public void Setup(Note note, bool outward = false)
    {
        // hide both accidentals
        foreach (var acc in accidentals.Values)
            acc.Visible = false;

        TextureRect accidentalTex = accidentals[note.GetAccidental()];

        float headWidth = headTexture.Size.X * headTexture.Scale.X;
        float headHeight = headTexture.Size.Y * headTexture.Scale.Y;

        float accidentalWidth = accidentalTex is not null ? accidentalTex.Size.X * accidentalTex.Scale.X : 0;
        float headX = headWidth / 2;
        float spacingX = 2;
        float accidentalX = headX + headWidth / 2 + accidentalWidth / 2 + spacingX;
        if (!outward)
        {
            headX *= -1;
            accidentalX *= -1;
        }

        SetPositionByPivot(headTexture, new Vector2(headX, 0), headPivot);
        if (accidentalTex is not null)
        {
            // reveal only the relevant accidental
            accidentalTex.Visible = true;
            SetPositionByPivot(accidentalTex, new Vector2(accidentalX, 0), accidentalPivots[note.GetAccidental()]);
        }

        // DebugPositions(headX, accidentalX);
        Size = new Vector2(0, 0);
    }

    void DebugPositions(float headX, float accidentalX)
    { 
        if (debugRect1 is null)
        {
            debugRect1 = new ColorRect();
            debugRect2 = new ColorRect();
            debugRect3 = new ColorRect();
            debugRect4 = new ColorRect();

            debugRect1.Size = new Vector2(6, 6);
            debugRect2.Size = new Vector2(6, 6);
            debugRect3.Size = new Vector2(6, 6);
            debugRect4.Size = new Vector2(2, 60);

            debugRect1.Modulate = Colors.Red;
            debugRect2.Modulate = Colors.Green;
            debugRect3.Modulate = Colors.Blue;
            debugRect4.Modulate = Colors.Orange;

            sharpTexture.GetParent().AddChild(debugRect1);
            sharpTexture.GetParent().AddChild(debugRect2);
            sharpTexture.GetParent().AddChild(debugRect3);
            sharpTexture.GetParent().AddChild(debugRect4);
        }

        debugRect1.Position = new Vector2(accidentalX, 0) - debugRect1.Size / 2;
        debugRect2.Position = new Vector2(accidentalX, 0) - debugRect2.Size / 2;
        debugRect3.Position = new Vector2(headX, 0) - debugRect3.Size / 2;
    }
}
