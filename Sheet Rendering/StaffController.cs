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
    [Export] Control notesContainer;
    [Export] Control labelsContainer;
    [Export] TextureRect noteTexture;
    [Export] Label noteLabel;

    List<ColorRect> staffLines;

    List<TextureRect> noteTextures;
    List<Label> noteLabels;

    const float thickness = 4; // thickness of a sheet line
    const float spacing = 20; // distance between two sheet lines
    const float firstStaffTopMargin = spacing * 2 + spacing / 2; // two full lines plus one half life to account for note head size
    const float secondStaffTopMargin = firstStaffTopMargin + 5 * spacing + 2 * spacing; // 5 lines from first staff plus two full lines

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
            ColorRect line = new ColorRect();
            line.Visible = true;
            line.Color = Colors.Black;
            line.Size = new Vector2(notesContainer.Size.X, thickness);
            line.Position = new Vector2(0, topOffset + spacing * i - thickness / 2);

            staffLines.Add(line);
            notesContainer.AddChild(line);
        }
    }

    public void BuildStaff()
    {
        staffLines = new List<ColorRect>();

        float height = firstStaffTopMargin; // top offset with room for 2 notes
        height += spacing * 5;    // staff height with 5 lines
        height += 2 * spacing;    // bottom offset with room for 2 notes

        AddStaff(firstStaffTopMargin);

        if (staffType == StaffType.Grand)
        {
            AddStaff(secondStaffTopMargin);

            // height += 3 * spacing; // spacing in between staves
            height += spacing * 5; // staff height
            height += 2 * spacing; // more room at the bottom
        }
        notesContainer.CustomMinimumSize = new Vector2(notesContainer.Size.X, height);
    }

    public float GetNoteHeight(int noteNameIndex, int octave)
    {
        float firstStaffHeight = spacing / 2;
        float secondStaffHeight = spacing * 3 + spacing * 5 + spacing / 2; // the first note c4 is where?
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
        UpdateNote(noteIndex, 1);
    }

    public void UpdateNote(int noteIndex, int idx, bool hideLabel = false, bool hideNote = false)
    {
        if (noteIndex == -1)
        {
            noteLabels[idx].Visible = false;
            noteTextures[idx].Visible = false;
            return;
        }

        noteLabels[idx].Visible = !hideLabel;
        noteLabels[idx].Text = PianoUIController.GetNoteNameShort(noteIndex);
        noteTextures[idx].Visible = !hideNote;
        int octave = noteIndex < 0 ? 0 : noteIndex / 12;
        int noteNameIndex = SemitoneToTone(noteIndex % 12);
        GD.Print("Note index: ", noteIndex, " Note name index: ", noteNameIndex, " Octave: ", octave, "Height: ", GetNoteHeight(noteNameIndex, octave));

        float xx = noteTextures[idx].Position.X;
        float yy = GetNoteHeight(noteNameIndex, octave) - noteTextures[idx].Size.Y / 2;

        noteTextures[idx].Position = new Vector2(xx, yy);
    }

    public override void _Ready()
    {
        base._Ready();

        noteLabels = new List<Label>();
        noteTextures = new List<TextureRect>();

        noteLabels.Add(noteLabel);
        noteLabels.Add((Label)noteLabel.Duplicate());

        noteTextures.Add(noteTexture);
        noteTextures.Add((TextureRect)noteTexture.Duplicate());
        
        labelsContainer.AddChild(noteLabels[1]);
        notesContainer.AddChild(noteTextures[1]);

        noteLabels[0].Position = new Vector2(100 - noteLabel.Size.X / 2, noteLabel.Position.Y);
        noteLabels[1].Position = new Vector2(250 - noteLabel.Size.X / 2, noteLabel.Position.Y);

        noteTextures[0].Position = new Vector2(100 - noteTexture.Size.X / 2, noteTexture.Position.Y);
        noteTextures[1].Position = new Vector2(250 - noteTexture.Size.X / 2, noteTexture.Position.Y);

        BuildStaff();
    }

}
