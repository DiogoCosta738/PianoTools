using Godot;
using System;
using System.Collections.Generic;

public enum StaffType
{
    Treble,
    Bass,
    Grand
}

public partial class StaffController : Node
{
    [Export] Control container;
    [Export] ColorRect lineTemplate;
    [Export] TextureRect noteTexture;

    List<ColorRect> staffLines;

    float thickness = 4;
    float spacing = 20;

    StaffType staffType = StaffType.Grand;

    public void ClearStaff()
    {
        foreach (var line in staffLines)
        {
            line.QueueFree();
        }
        staffLines.Clear();
    }

    public void AddStaff(float topOffset)
    {
        for (int i = 0; i < 5; i++)
        {
            ColorRect line = (ColorRect)lineTemplate.Duplicate();
            line.Visible = true;
            Color[] colors = new Color[] {
                Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Black, Colors.Gray
            };
            line.Color = colors[i];
            line.Size = new Vector2(container.Size.X, thickness);
            line.Position = new Vector2(0, topOffset + spacing * i + thickness / 2);

            staffLines.Add(line);
            container.AddChild(line);
        }
    }

    public void BuildStaff()
    {
        staffLines = new List<ColorRect>();

        float topOffset = spacing * 3;
        float height = topOffset; // top offset with room for 2 notes
        height += spacing * 5;    // staff height with 5 lines
        height += 3 * spacing;    // bottom offset with room for 2 notes

        lineTemplate.Visible = false;
        AddStaff(topOffset);

        if (staffType == StaffType.Grand)
        {
            topOffset += 5 * spacing + 3 * spacing;
            AddStaff(topOffset);

            // height += 3 * spacing; // spacing in between staves
            height += spacing * 5; // staff height
            height += 3 * spacing; // more room at the bottom
        }
        container.CustomMinimumSize = new Vector2(container.Size.X, height);
    }

    public float GetNoteHeight(int noteNameIndex, int octave)
    {
        float firstStaffHeight = spacing;
        float secondStaffHeight = spacing * 3 + spacing * 5 + spacing * 3;
        switch (staffType)
        {
            case StaffType.Treble:
                return firstStaffHeight - spacing / 2 * noteNameIndex - spacing / 2 * 7 * (octave - 6);
            case StaffType.Bass:
                return firstStaffHeight - spacing / 2 * noteNameIndex - spacing / 2 * 7 * (octave - 4);
            case StaffType.Grand:
                if (octave >= 4)
                {
                    return firstStaffHeight - spacing / 2 * noteNameIndex - spacing / 2 * 7 * (octave - 6);
                }
                else
                {
                    return secondStaffHeight - spacing / 2 * noteNameIndex - spacing / 2 * 7 * (octave - 4);
                }
        }
        return 0;
    }

    // from 0-11 to 0-6
    // !TODO: should add a "preferSharp" or flat
    public int SemitoneToTone(int semitone)
    {
        switch (semitone)
        {
            case 0:
            case 1:
                return 0; // c
            case 2:
            case 3:
                return 1; // d
            case 4:
                return 2; // e
            case 5:
            case 6:
                return 3; // f
            case 7:
            case 8:
                return 4; // g
            case 9:
            case 10:
                return 5; // a
            case 11:
                return 6; // b
        }
        return -1;
    }

    public void UpdateNote(int noteIndex)
    {
        int octave = noteIndex < 0 ? 0 : noteIndex / 12;
        int noteNameIndex = SemitoneToTone(noteIndex % 12);
        GD.Print("Note index: ", noteIndex, " Note name index: ", noteNameIndex, " Octave: ", octave, "Height: ", GetNoteHeight(noteNameIndex, octave));
        noteTexture.Position = new Vector2(noteTexture.Position.X, GetNoteHeight(noteNameIndex, octave) - noteTexture.Size.Y / 2);
    }

    public override void _Ready()
    {
        base._Ready();
        BuildStaff();
    }

}
