using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class NoteUtils
{
    static string noteLetters = "CDEFGAB";

    static string[] noteShortName = {
        "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
    };

    static Dictionary<string, int> noteShortNameToSemitone = new Dictionary<string, int>()
    {
       { "C", 0 },
       { "C#", 1 },
       { "Cb", 11 },
       { "D", 2 },
       { "D#", 3 },
       { "Db", 1 },
       { "E", 4 },
       { "E#", 5 },
       { "Eb", 3 },
       { "F", 5 },
       { "F#", 6 },
       { "Fb", 4 },
       { "G", 7 },
       { "G#", 8 },
       { "Gb", 6 },
       { "A", 9 },
       { "A#", 10 },
       { "Ab", 8 },
       { "B", 11 },
       { "B#", 0 },
       { "Bb", 10 },
    };

    static Dictionary<string, (string shortName, int octaveShift)> flatEquivalent = new Dictionary<string, (string shortName, int octaveShift)>()
    {
            // { "C", "" },
        { "C#", ("Db", 0) },
            // { "Cb", "B" },
            // { "D", "" },
        { "D#", ("Eb", 0) },
            // { "Db", "C#" },
        // { "E", ("Fb", 0) },  // although they have a flat equivalent, we don't want to use them
            // { "E#", "F" },
            // { "Eb", "D#" },
            // { "F", "E#" },
        { "F#", ("Gb", 0) },
            // { "Fb", "E" },
            // { "G", "" },
        { "G#", ("Ab", 0) },
            // { "Gb", "F#" },
            // { "A", "" },
        { "A#", ("Bb", 0) },
            // { "Ab", "G#" },
        // { "B", ("Cb", 1) }, // although they have a flat equivalent, we don't want to use them
            // { "B#", "C" },
            // { "Bb", "A#" },
    };

    static bool[] noteAccidental = {
        false, true, false, true, false, false, true, false, true, false, true, false
    };

    public static char GetToneLetter(int tone)
    {
        return noteLetters[tone];
    }

    public static int ToMidiNote(Note note)
    {
        int oct = note.GetOctave();
        int baseIndex = noteShortNameToSemitone[note.GetToneLetter() + note.GetAccidental()];
        if (note.GetToneLetter() == 'C' && note.GetAccidental() == "b") oct--;
        return baseIndex + 12 * oct;
    }

    public static Note FromMidiNote(int midiIndex, bool preferFlat = false)
    {
        if (midiIndex < 0) return null;

        int octave = midiIndex < 0 ? 0 : midiIndex / 12;
        int semitone = midiIndex % 12;
        int noteTone = NoteUtils.SemitoneToTone(semitone);
        string shortName = noteShortName[semitone];
        if (preferFlat && flatEquivalent.ContainsKey(shortName))
        {
            (string flatShortName, int octaveShift) = flatEquivalent[shortName];
            octave += octaveShift;
            return new Note((noteTone + 1) % 7, octave, "b");
        }
        else if (NoteUtils.HasAccidental(midiIndex))
        {
            return new Note(noteTone, octave, "#");
        }
        else
        {
            return new Note(noteTone, octave, "");
        }
    }

    public static string GetNoteNameShort(int midiNote)
    {
        if (midiNote < 0) return "N/A";

        int octave = (midiNote) / 12;
        int semitone = midiNote % 12;
        return $"{noteShortName[semitone]}{octave}";
    }

    public static bool HasAccidental(int midiNote)
    {
        if (midiNote < 0) return false;

        int octave = (midiNote) / 12;
        int semitone = midiNote % 12;
        return noteAccidental[semitone];
    }

    // from 0-11 to 0-6
    public static int SemitoneToTone(int semitone, bool preferFlat)
    {
        int tone = SemitoneToTone(semitone);
        if (!preferFlat || !HasAccidental(semitone))
            return tone;
        return tone == 0 ? 6 : tone - 1;
    }
    public static int SemitoneToTone(int semitone)
    {
        semitone = semitone % 12;
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

    public static void TestNotes()
    {
        string[] accidentals = new string[] { "", "#", "b"};
        for (int oct = 1; oct <= 8; oct++)
        {
            for (int noteTone = 0; noteTone < noteLetters.Length; noteTone++)
            {
                foreach (var acc in accidentals)
                {
                    Note note = new Note(noteTone, oct, acc);
                    int midiNote = note.ToMidiNote();

                    Note noteSharp = NoteUtils.FromMidiNote(midiNote, preferFlat: false);
                    Note noteFlat = NoteUtils.FromMidiNote(midiNote, preferFlat: true);

                    int midiNoteSharp = noteSharp.ToMidiNote();
                    int midiNoteFlat = noteFlat.ToMidiNote();

                    // avoid the cumbersome accidental tests because of things like E# which is actually F
                    // maybe revisit the whole conversion later so make it exhaustive, meaning every note knows its forced sharp, flat, and whether it is "acceptable"
                    bool test1 = midiNote == midiNoteSharp;
                    bool test2 = midiNote == midiNoteFlat;
                    bool test3 = midiNoteSharp == midiNoteFlat;
                    /*
                    bool eq1 = note == noteSharp;
                    bool eq2 = note == noteFlat;
                    bool test4 = acc != "#" || eq1;
                    bool test5 = acc != "b" || eq2;
                    Debug.Assert(test1 && test2 && test3 && test4 && test5);
                    */
                    if (!(test1 && test2 && test3 /*&& test4 && test5*/))
                    {
                        GD.Print("ERROR: ", note.GetNameShort());
                    }
                }
            }
        }
    }
}
