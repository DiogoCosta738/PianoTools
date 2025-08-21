using Godot;
using System;
using System.Collections.Generic;

public static class NoteUtils
{
    static string noteLetters = "CDEFGAB";

    static string[] noteShortName = {
        "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
    };

    static Dictionary<string, int> noteShortNameToIndex = new Dictionary<string, int>()
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
       { "Fb", 5 },
       { "G", 7 },
       { "G#", 8 },
       { "Gb", 7 },
       { "A", 9 },
       { "A#", 10 },
       { "Ab", 8 },
       { "B", 11 },
       { "B#", 0 },
       { "Bb", 10 },
    };

	static bool[] noteAccidental = {
        false, true, false, true, false, false, true, false, true, false, true, false
    };

    public static char GetNoteLetter(int idx)
    {
        return noteLetters[idx];
    }

    public static int ToMidiNote(Note note)
    {
        int oct = note.GetOctave();
        int baseIndex = noteShortNameToIndex[note.GetNoteLetter() + note.GetAccidental()];
        return baseIndex + 12 * oct;
    }

    public static Note FromMidiNote(int midiIndex, bool preferFlat = false)
    {
        if (midiIndex < 0) return null;

        int octave = midiIndex < 0 ? 0 : midiIndex / 12;
        int noteNameIndex = NoteUtils.SemitoneToTone(midiIndex % 12);
        if (NoteUtils.HasAccidental(midiIndex))
        {
            if (preferFlat)
            {
                noteNameIndex--;
                if (noteNameIndex < 0)
                {
                    octave--;
                    noteNameIndex = 7;
                }
                return new Note(noteNameIndex, octave, "b");
            }
            return new Note(noteNameIndex, octave, "#");
        }
        else
        {
            return new Note(noteNameIndex, octave, "");
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
}
