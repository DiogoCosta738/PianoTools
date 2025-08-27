using Godot;
using System;
using NAudio.Midi;
using System.ComponentModel.DataAnnotations;
using System.Threading;

public partial class NotePlayExerciseController : NoteExerciseBase
{
    [Export] MidiKeyboardController midiKeyboardController;
    [Export] PianoUIController screenPianoController;

    [Export] CheckBox ignoreOctavesCheck;

    protected override bool MatchesWaitingNote(Note note)
    {
        if (ignoreOctavesCheck.ButtonPressed)
        {
            return note.GetToneIndex() == waitingNote.GetToneIndex() && note.GetAccidental() == waitingNote.GetAccidental();
        }
        return note.ToMidiNote() == waitingNote.ToMidiNote();
    }

    public override void _Ready()
    {
        base._Ready();
        midiKeyboardController.OnNoteDown += (note, vel) => CallDeferred("SubmitNote", note);
        screenPianoController.OnNoteDown += (note) => CallDeferred("SubmitNote", note);
    }
}
