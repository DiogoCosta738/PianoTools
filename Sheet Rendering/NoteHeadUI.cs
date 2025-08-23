using Godot;
using System;

public partial class NoteHeadUI : Control
{
    [Export] TextureRect flatTexture;
    [Export] TextureRect sharpTexture;
    [Export] TextureRect headTexture;

    public void Setup(Note note)
    {
        flatTexture.Visible = note.GetAccidental() == "b";
        sharpTexture.Visible = note.GetAccidental() == "#";

        float width =
            // (flatTexture.Visible ? flatTexture.Size.X * flatTexture.Scale.X : 0) +
            // (sharpTexture.Visible ? sharpTexture.Size.X * sharpTexture.Scale.X : 0) +
            headTexture.Size.X * headTexture.Scale.X;
        float height = headTexture.Size.Y * headTexture.Scale.Y;
        /*
        Mathf.Max(Mathf.Max(
            flatTexture.Visible ? flatTexture.Size.Y * flatTexture.Scale.Y : 0,
            sharpTexture.Visible ? sharpTexture.Size.Y * sharpTexture.Scale.Y : 0),
            headTexture.Size.Y * headTexture.Scale.Y);
            */

        float spacingX = 10;

        flatTexture.Position = new Vector2(- flatTexture.PivotOffset.X - spacingX, height / 2 - flatTexture.PivotOffset.Y);
        sharpTexture.Position = new Vector2(- sharpTexture.PivotOffset.X - spacingX, height / 2 - sharpTexture.PivotOffset.Y);
        headTexture.Position = new Vector2(- headTexture.PivotOffset.X / 2, height / 2 - headTexture.PivotOffset.Y);

        Size = new Vector2(width, height);
    }
}
