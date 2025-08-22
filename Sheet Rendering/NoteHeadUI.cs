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
        float height = Mathf.Max(Mathf.Max(
            flatTexture.Visible ? flatTexture.Size.Y * flatTexture.Scale.Y : 0,
            sharpTexture.Visible ? sharpTexture.Size.Y * sharpTexture.Scale.Y : 0),
            headTexture.Size.Y * headTexture.Scale.Y);

        float spacingX = headTexture.Size.X * headTexture.Scale.X / 4;

        flatTexture.Position = new Vector2(- flatTexture.Size.X / 2 * flatTexture.Scale.X - spacingX, height / 2 - flatTexture.Size.Y / 2 * flatTexture.Scale.Y);
        sharpTexture.Position = new Vector2(- sharpTexture.Size.X / 2 * sharpTexture.Scale.X - spacingX, height / 2 - sharpTexture.Size.Y / 2 * sharpTexture.Scale.Y);
        headTexture.Position = new Vector2(0, height / 2 - headTexture.Size.Y / 2 * headTexture.Scale.Y);

        Size = new Vector2(width, height);
    }
}
