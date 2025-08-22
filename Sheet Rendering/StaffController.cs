using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;

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
    [Export] NoteHeadUI noteHeadTemplate;
    [Export] Label noteLabel;
    [Export] TextureRect trebleClef, bassClef;

    List<ColorRect> staffLines;

    List<NoteHeadUI> noteHeads;
    List<Label> noteLabels;
    List<List<ColorRect>> notePartialLines;

    List<int> notesDown;

    const float TreblePivotX = 1; // 14.0f / 24;
    const float TreblePivotY = 35f / 54;
    const float BassPivotX = 1; // 24.5f / 49;
    const float BassPivotY = 9.5f / 29;

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

    const int NOTE_C = 0;
    const int NOTE_D = 1;
    const int NOTE_E = 2;
    const int NOTE_F = 3;
    const int NOTE_G = 4;
    const int NOTE_A = 5;
    const int NOTE_B = 6;

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
            height += spacing * 5; // staff height
            height += 2 * spacing; // more room at the bottom
        }
        notesContainer.CustomMinimumSize = new Vector2(GetSheetWidth(), height);

        trebleClef.Visible = staffType == StaffType.Treble || staffType == StaffType.Grand;
        bassClef.Visible = staffType == StaffType.Bass || staffType == StaffType.Grand;

        float offsetX = Mathf.Max(trebleClef.Size.X * trebleClef.Scale.X, bassClef.Size.X * bassClef.Scale.X);
        trebleClef.Position = new Vector2(
            offsetX - trebleClef.Size.X * TreblePivotX * trebleClef.Scale.X,
            GetNoteHeight(NOTE_G, 4) - trebleClef.Size.Y * trebleClef.Scale.Y * TreblePivotY);

        bassClef.Position = new Vector2(
            offsetX - bassClef.Size.X * BassPivotX * bassClef.Scale.X,
            GetNoteHeight(NOTE_F, 3) - bassClef.Size.Y * bassClef.Scale.Y * BassPivotY);
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
    List<float> GetPartialLinesRequired(Note note)
    {
        // the trick here is to ignore the accidental and use pure tone logic (it's the note that determines the height, accidentals are irrelevant and only confound)
        note = note.Clone();
        note.SetAccidental("");
        int midiIndex = note.ToMidiNote();

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
            noteHeads[idx].Visible = false;
            return;
        }

        List<float> extraLines = GetPartialLinesRequired(note);
        if (extraLines is not null)
        {
            foreach (float y in extraLines)
            {
                ColorRect line = CreateLine(GetNoteCenterX(idx), y, columnWidth / 3);
                notePartialLines[idx].Add(line);
            }
        }

        noteLabels[idx].Visible = !hideLabel;
        noteLabels[idx].Text = note.GetNameShort();
        noteHeads[idx].Visible = !hideNote;
        noteHeads[idx].Setup(note);

        GD.Print("Note index: ", note.ToMidiNote(), " Note letter index: ", note.GetToneIndex(), " Octave: ", note.GetOctave(), "Height: ", GetNoteHeight(note.GetToneIndex(), note.GetOctave()));

        float xx = GetNoteCenterX(idx) - noteHeads[idx].Size.X / 2;
        float yy = GetNoteHeight(note.GetToneIndex(), note.GetOctave()) - noteHeads[idx].Size.Y / 2;

        noteHeads[idx].Position = new Vector2(xx, yy);
    }

    public float GetNoteCenterX(int index)
    {
        return leftMargin + columnWidth / 2 + columnWidth * index;
    }

    float GetSheetWidth()
    {
        return leftMargin + columnCount * columnWidth;
    }

    public override void _Ready()
    {
        base._Ready();

        noteLabels = new List<Label>();
        noteHeads = new List<NoteHeadUI>();
        notePartialLines = new List<List<ColorRect>>();

        noteLabels.Add(noteLabel);
        noteHeads.Add(noteHeadTemplate);
        notePartialLines.Add(new List<ColorRect>());

        for (int i = 1; i < columnCount; i++)
        {
            noteLabels.Add((Label)noteLabel.Duplicate());
            noteHeads.Add((NoteHeadUI)noteHeadTemplate.Duplicate());
            notePartialLines.Add(new List<ColorRect>());

            labelsContainer.AddChild(noteLabels[i]);
            notesContainer.AddChild(noteHeads[i]);
        }

        for (int i = 0; i < noteLabels.Count; i++)
        {
            noteLabels[i].Position = new Vector2(GetNoteCenterX(i) - noteLabel.Size.X / 2, noteLabel.Position.Y);
            noteHeads[i].Position = new Vector2(GetNoteCenterX(i) - noteHeadTemplate.Size.X / 2, noteHeadTemplate.Position.Y);
        }
        BuildStaff();

        for (int i = 0; i < noteLabels.Count; i++)
            UpdateNote(null, i);
    }

}
