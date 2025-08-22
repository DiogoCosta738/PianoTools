using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;

public enum StaffType
{
    Treble,
    Bass,
    Grand
}

public partial class StaffController : Control
{
    [Export] Control notesContainer;
    [Export] Control labelsContainer;
    [Export] TextureRect noteTexture;
    [Export] Label noteLabel;

    List<ColorRect> staffLines;

    List<TextureRect> noteTextures;
    List<Label> noteLabels;
    List<List<ColorRect>> notePartialLines;

    List<int> notesDown;

    const float thickness = 4; // thickness of a sheet line
    const float spacing = 20; // distance between two sheet lines
    const float firstStaffTopMargin = spacing * 2 + spacing / 2; // two full lines plus one half life to account for note head size
    const float secondStaffTopMargin = firstStaffTopMargin + 5 * spacing + 2 * spacing; // 5 lines from first staff plus two full lines

    int columnCount = 2;
    int leftMargin = 50;
    int columnWidth = 100;

    StaffType staffType = StaffType.Grand;

    public void ClearStaff()
    {
        foreach (var line in staffLines)
        {
            line.QueueFree();
        }
        staffLines.Clear();
    }

    ColorRect CreateLine(float x, float y, float width)
    {
        ColorRect line = new ColorRect();
        line.Visible = true;
        line.Color = Colors.Black;
        line.CustomMinimumSize = new Vector2(width, thickness);
        line.Position = new Vector2(x - width / 2, y - thickness / 2);
        notesContainer.AddChild(line);
        return line;
    }

    public void AddStaff(float topOffset)
    {
        for (int i = 0; i < 5; i++)
        {
            ColorRect line = CreateLine(GetSheetWidth() / 2, topOffset + spacing * i, GetSheetWidth());
            staffLines.Add(line);
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
        notesContainer.CustomMinimumSize = new Vector2(GetSheetWidth(), height);
    }

    public float GetNoteHeight(int noteTone, int octave)
    {
        float firstStaffHeight = spacing / 2;
        float secondStaffHeight = spacing * 3 + spacing * 5 + spacing / 2; // the first note c4 is where?
        switch (staffType)
        {
            case StaffType.Treble:
                return firstStaffHeight - spacing / 2 * noteTone - spacing / 2 * 7 * (octave - 6);
            case StaffType.Bass:
                // E4 is two lines above, which is noteTone 2
                return firstStaffHeight - spacing / 2 * (noteTone - 2) - spacing / 2 * 7 * (octave - 4);
            case StaffType.Grand:
                if (octave >= 4)
                {
                    return firstStaffHeight - spacing / 2 * noteTone - spacing / 2 * 7 * (octave - 6);
                }
                else
                {
                    return secondStaffHeight - spacing / 2 * noteTone - spacing / 2 * 7 * (octave - 4);
                }
        }
        return 0;
    }

    public void UpdateNote(int noteTone, int octave, string accidental)
    { 
        UpdateNote(new Note(noteTone, octave, accidental), 1);
    }

    public void UpdateNote()
    {
        UpdateNote(null, 1);
    }

    public void UpdateNote(Note note)
    {
        UpdateNote(note, 1);
    }

    const int A5 = 69; // first (lowest) note requiring ledger line above treble staff
    const int E2 = 28; // first (highest) note requiring ledger line below bass staff
    const int C4 = 48; // first (highest) note requiring ledger line below treble staff and first (lowest) above bass staff

    // as a list of y positions
    List<float> GetPartialLinesRequired(int midiIndex)
    {
        List<float> lines = new List<float>();
        void local_handle_semitone(int midiIndex, ref bool include, List<float> extraLines)
        {
            if (NoteUtils.HasAccidental(midiIndex))
                return;
            else
            {
                if (include)
                {
                    Note note = NoteUtils.FromMidiNote(midiIndex);
                    extraLines.Add(GetNoteHeight(note.GetToneIndex(), note.GetOctave()));
                }
                include ^= true;
            }
        }
        bool include = true;
        if ((staffType == StaffType.Treble || staffType == StaffType.Grand) && midiIndex >= A5)
        {
            for (int i = A5; i <= midiIndex; i++)
                local_handle_semitone(i, ref include, lines);
        }
        else if ((staffType == StaffType.Bass || staffType == StaffType.Grand) && midiIndex <= E2)
        {
            for (int i = E2; i >= midiIndex; i--)
                local_handle_semitone(i, ref include, lines);
        }
        else if (staffType == StaffType.Treble && midiIndex <= C4)
        {
            for (int i = C4; i >= midiIndex; i--)
                local_handle_semitone(i, ref include, lines);
        }
        else if (staffType == StaffType.Bass && midiIndex >= C4)
        {
            for (int i = C4; i <= midiIndex; i++)
                local_handle_semitone(i, ref include, lines);
        }
        else if (staffType == StaffType.Grand && midiIndex == C4)
        {
            return new List<float>() { GetNoteHeight(0, 4) };
        }
        return lines;
    }

    public void UpdateNote(Note note, int idx, bool hideLabel = false, bool hideNote = false)
    {
        foreach (var partial in notePartialLines[idx])
        {
            partial.QueueFree();
        }
        notePartialLines[idx].Clear();

        if (note is null)
        {
            noteLabels[idx].Visible = false;
            noteTextures[idx].Visible = false;
            return;
        }

        List<float> extraLines = GetPartialLinesRequired(note.ToMidiNote());
        if (extraLines is not null)
        {
            foreach (float y in extraLines)
            {
                ColorRect line = CreateLine(GetNoteCenterX(idx), y, columnWidth / 2);
                notePartialLines[idx].Add(line);
            }
        }

        noteLabels[idx].Visible = !hideLabel;
        noteLabels[idx].Text = note.GetNameShort();
        noteTextures[idx].Visible = !hideNote;

        GD.Print("Note index: ", note.ToMidiNote(), " Note letter index: ", note.GetToneIndex(), " Octave: ", note.GetOctave(), "Height: ", GetNoteHeight(note.GetToneIndex(), note.GetOctave()));

        float xx = noteTextures[idx].Position.X;
        float yy = GetNoteHeight(note.GetToneIndex(), note.GetOctave()) - noteTextures[idx].Size.Y / 2;

        noteTextures[idx].Position = new Vector2(xx, yy);
    }

    public float GetNoteCenterX(int index)
    {
        return leftMargin + columnWidth / 2  + columnWidth * index;
    }

    float GetSheetWidth()
    {
        return leftMargin + columnCount * columnWidth;
    }

    public override void _Ready()
    {
        base._Ready();

        noteLabels = new List<Label>();
        noteTextures = new List<TextureRect>();
        notePartialLines = new List<List<ColorRect>>();

        noteLabels.Add(noteLabel);
        noteTextures.Add(noteTexture);
        notePartialLines.Add(new List<ColorRect>());

        for (int i = 1; i < columnCount; i++)
        {
            noteLabels.Add((Label)noteLabel.Duplicate());
            noteTextures.Add((TextureRect)noteTexture.Duplicate());
            notePartialLines.Add(new List<ColorRect>());

            labelsContainer.AddChild(noteLabels[i]);
            notesContainer.AddChild(noteTextures[i]);
        }

        for (int i = 0; i < noteLabels.Count; i++)
        {
            noteLabels[i].Position = new Vector2(GetNoteCenterX(i) - noteLabel.Size.X / 2, noteLabel.Position.Y);
            noteTextures[i].Position = new Vector2(GetNoteCenterX(i) - noteTexture.Size.X / 2, noteTexture.Position.Y);
        }
        BuildStaff();
    }

}
