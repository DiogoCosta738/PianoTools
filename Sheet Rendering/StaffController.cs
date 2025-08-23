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

    List<bool> hideLabels;
    List<bool> hideRendering;
    List<List<NoteHeadUI>> noteHeads;
    List<List<Note>> notes;
    List<Label> noteLabels;
    List<List<ColorRect>> notePartialLines;

    List<NoteHeadUI> unusedHeads = new List<NoteHeadUI>();
    List<ColorRect> unusedPartialLines = new List<ColorRect>();

    List<int> notesDown;

    const float TreblePivotX = 235f / 364f; // 14.0f / 24;
    const float TreblePivotY = 570f / 899f;
    const float BassPivotX = 85f / 414f + 0.4f; // 24.5f / 49;
    const float BassPivotY = 140f / 457f - 0.009f;

    const float thickness = 4; // thickness of a sheet line
    const float spacing = 20; // distance between two sheet lines
    const float firstStaffTopMargin = spacing * 2 + spacing / 2; // two full lines plus one half life to account for note head size
    const float secondStaffTopMargin = firstStaffTopMargin + 5 * spacing + 2 * spacing; // 5 lines from first staff plus two full lines

    int columnCount = 2;
    int leftMargin = 75;
    int columnWidth = 100;

    StaffType staffType = StaffType.Grand;

    public void HideNote(int idx, bool hide)
    {
        hideRendering[idx] = hide;
    }

    public void HideLabel(int idx, bool hide)
    {
        hideLabels[idx] = hide;
    }

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
        ColorRect line;
        if (unusedPartialLines.Count != 0)
        {
            line = unusedPartialLines[unusedPartialLines.Count - 1];
            unusedPartialLines.RemoveAt(unusedPartialLines.Count - 1);
        }
        else
        {
            line = new ColorRect();
            line.Color = Colors.Black;
            notesContainer.AddChild(line);
        }

        line.Visible = true;
        line.Size = new Vector2(width, thickness);
        line.Position = new Vector2(x - width / 2, y - thickness / 2);
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

        float offsetX = Mathf.Max(trebleClef.Size.X * trebleClef.Scale.X, bassClef.Size.X * bassClef.Scale.X) - 20;
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
        // !TODO refactor to use heightIndex to be more readable and conform to the adjacency check
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

    const int A5 = 69; // first (lowest) note requiring ledger line above treble staff
    const int E2 = 28; // first (highest) note requiring ledger line below bass staff
    const int C4 = 48; // first (highest) note requiring ledger line below treble staff and first (lowest) above bass staff

    // as a list of y positions
    void AddPartialLinesRequired(Note note, HashSet<float> lines)
    {
        // the trick here is to ignore the accidental and use pure tone logic (it's the note that determines the height, accidentals are irrelevant and only confound)
        note = note.Clone();
        note.SetAccidental("");
        int midiIndex = note.ToMidiNote();

        void local_handle_semitone(int midiIndex, ref bool include, HashSet<float> extraLines)
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
            lines.Add(GetNoteHeight(0, 4));
            return;
        }
        return;
    }

    public NoteHeadUI GetNewNoteHead()
    {
        if (unusedHeads.Count != 0)
        {
            NoteHeadUI head = unusedHeads[unusedHeads.Count - 1];
            unusedHeads.RemoveAt(unusedHeads.Count - 1);
            return head;
        }

        NoteHeadUI newHead = (NoteHeadUI)noteHeadTemplate.Duplicate();
        newHead.CopyFrom(noteHeadTemplate);
        notesContainer.AddChild(newHead);
        return newHead;
    }

    public void AddNote(int noteTone, int octave, string accidental) { AddNote(new Note(noteTone, octave, accidental), 1); }
    public void AddNote(Note note) { AddNote(note, 1); }
    public void RemoveNote(int noteTone, int octave, string accidental) { RemoveNote(new Note(noteTone, octave, accidental), 1); }
    public void RemoveNote(Note note) { RemoveNote(note, 1); }

    public void AddNote(Note note, int idx)
    {
        notes[idx].Add(note);
        UpdateNotesAtPosition(idx);
    }

    public void RemoveNote(Note note, int idx)
    {
        notes[idx].Remove(note);
        UpdateNotesAtPosition(idx);
    }

    bool AreNotesAdjacent(Note note1, Note note2)
    {
        int heightIndex1 = note1.GetToneIndex() + note1.GetOctave() * 7;
        int heightIndex2 = note2.GetToneIndex() + note2.GetOctave() * 7;
        return Mathf.Abs(heightIndex1 - heightIndex2) <= 1;
    }

    void UpdateNotesAtPosition(int idx)
    {
        // clear partial lines
        foreach (var partial in notePartialLines[idx])
        {
            partial.Visible = false;
            unusedPartialLines.Add(partial);
        }
        notePartialLines[idx].Clear();

        // clear note heads
        for (int i = 0; i < noteHeads[idx].Count; i++)
        {
            NoteHeadUI head = noteHeads[idx][i];
            head.Visible = false;
            unusedHeads.Add(head);
        }
        noteHeads[idx].Clear();

        // no notes being rendered
        if (notes[idx].Count == 0)
        {
            noteLabels[idx].Visible = false;
            return;
        }

        noteLabels[idx].Visible = !hideLabels[idx];
        noteLabels[idx].Text = notes[idx][0].GetNameShort();

        notes[idx].Sort((note1, note2) =>
        {
            float height1 = GetNoteHeight(note1.GetToneIndex(), note1.GetOctave()), height2 = GetNoteHeight(note2.GetToneIndex(), note2.GetOctave());
            if (height1 != height2)
                return Mathf.Sign(height1 - height2);
            else
                return note1.ToMidiNote() - note2.ToMidiNote();
        });
        notes[idx].Reverse();
        foreach (Note note in notes[idx]) GD.Print(note.ToMidiNote());

        HashSet<float> partialLines = new HashSet<float>();
        bool extendedPartials = false;
        bool outward = false;
        for (int i = 0; i < notes[idx].Count; i++)
        {
            NoteHeadUI head = GetNewNoteHead();
            head.Visible = true;
            noteHeads[idx].Add(head);
            Note note = notes[idx][i];

            head.Visible = !hideRendering[idx];
            // GD.Print("Note index: ", note.ToMidiNote(), " Note letter index: ", note.GetToneIndex(), " Octave: ", note.GetOctave(), "Height: ", GetNoteHeight(note.GetToneIndex(), note.GetOctave()));
            float xx = GetNoteCenterX(idx) - head.Size.X / 2;
            float yy = GetNoteHeight(note.GetToneIndex(), note.GetOctave()) - head.Size.Y / 2;

            if (i > 0 && AreNotesAdjacent(notes[idx][i - 1], notes[idx][i]))
            {
                outward ^= true;
            }
            else
            {
                outward = false;
            }
            extendedPartials |= outward;

            head.Position = new Vector2(xx, yy);
            head.Setup(note, outward);

            AddPartialLinesRequired(note, partialLines);
        }

        if (partialLines.Count != 0)
        {
            foreach (float y in partialLines)
            {
                float width = (extendedPartials ? 2 : 1) * noteHeadTemplate.GetWidth() + 4;
                ColorRect line = CreateLine(GetNoteCenterX(idx), y, width);
                if (!extendedPartials)
                    line.Position -= new Vector2(noteHeadTemplate.GetWidth() / 2, 0);
                notePartialLines[idx].Add(line);
            }
        }
    }

    public float GetNoteCenterX(int index) { return leftMargin + columnWidth / 2 + columnWidth * index; }
    float GetSheetWidth() { return leftMargin + columnCount * columnWidth; }

    public override void _Ready()
    {
        base._Ready();

        noteHeadTemplate.InitTemplate();
        
        noteLabels = new List<Label>();
        noteHeads = new List<List<NoteHeadUI>>();
        notes = new List<List<Note>>();
        hideLabels = new List<bool>();
        hideRendering = new List<bool>();
        notePartialLines = new List<List<ColorRect>>();

        for (int i = 0; i < columnCount; i++)
        {
            notes.Add(new List<Note>());
            noteHeads.Add(new List<NoteHeadUI>());
            noteLabels.Add((Label)noteLabel.Duplicate());
            notePartialLines.Add(new List<ColorRect>());
            labelsContainer.AddChild(noteLabels[i]);
            hideLabels.Add(false);
            hideRendering.Add(false);
        }

        for (int i = 0; i < noteLabels.Count; i++)
        {
            noteLabels[i].Position = new Vector2(GetNoteCenterX(i) - noteLabel.Size.X / 2, noteLabel.Position.Y);
            UpdateNotesAtPosition(i);
        }
        BuildStaff();
    }
}
