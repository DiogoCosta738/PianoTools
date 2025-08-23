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
        midiKeyboardController.OnNoteDown += (note, vel) => CallDeferred("SubmitNote", note);
    }
}
