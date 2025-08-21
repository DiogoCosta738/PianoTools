using Godot;
using System;
using NAudio.Midi;
using System.ComponentModel.DataAnnotations;
using System.Threading;

public partial class NotePlayExerciseController : NoteExerciseBase
{
    [Export] MidiKeyboardController midiKeyboardController;

    public override void _Ready()
    {
        base._Ready();
        midiKeyboardController.OnNoteDown += (note, vel) => CallDeferred("PlayedNote", note);
    }

    protected override int GenerateNote()
    {
        int noteMin = 24;
        int noteMax = 72;
        int noteRange = noteMax - noteMin;
        int newNote = -1;
        while (newNote == -1)
        {
            newNote = rng.Next() % noteRange + noteMin;
            if (NoteUtils.HasAccidental(newNote) && !useSharpsCheck.ButtonPressed && !useFlatsCheck.ButtonPressed)
                newNote = -1;
        }
        return newNote;
    }
}
