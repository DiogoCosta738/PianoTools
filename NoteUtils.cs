using Godot;
using System;

public static class NoteUtils
{
    static string[] noteShortName = {
		"C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
	};

	static bool[] noteAccidental = {
		false, true, false, true, false, false, true, false, true, false, true, false
	};
    
    public static (int noteNameIndex, int octave, string accidental) SplitMidiNote(int midiIndex, bool preferFlat = false)
    {
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
                return (noteNameIndex, octave, "b");
            }
            return (noteNameIndex, octave, "#");
        }
        else
        {
            return (noteNameIndex, octave, "");
        }
    }

	public static string GetNoteNameShort(int midiNote)
    {
        int octave = (midiNote) / 12;
        int semitone = midiNote % 12;
        return $"{noteShortName[semitone]}{octave}";
    }
	
	public static bool HasAccidental(int midiNote)
	{
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
