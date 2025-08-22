using Godot;
using System;
using System.Threading;

public partial class NoteNameExercise : NoteExerciseBase
{
    [Export] Button openInputButton;
    [Export] CheckBox octavesCheck;

    NoteNameInput inputDlg;

    protected bool HasOctave()
    {
        return octavesCheck.ButtonPressed;
    }

    protected override bool MatchesWaitingNote(Note note)
    {
        bool match = true;
        if (HasSharp() || HasFlat())
        {
            match &= waitingNote.GetAccidental() == note.GetAccidental();
        }
        if (HasOctave())
        { 
            match &= waitingNote.GetOctave() == note.GetOctave();
        }
        return match && note.GetToneIndex() == waitingNote.GetToneIndex();
    }

    void AddFeedbackNote(Note note, int durationMs)
    {
        note = note.Clone();
        if (!HasOctave())
            note.SetOctave(waitingNote is not null ? waitingNote.GetOctave() : 5);
        staffController.UpdateNote(note);
        Thread thread = new Thread(() =>
        {
            Thread.Sleep(durationMs);
            staffController.CallDeferred("UpdateNote");
        });
        thread.Start();
    }

    public override void OnCorrectNote(Note note) { AddFeedbackNote(note, Mathf.RoundToInt(correctWaitSeconds * 1000)); }

    public override void OnWrongNote(Note note) { AddFeedbackNote(note, Mathf.RoundToInt(wrongWaitSeconds * 1000)); }

    void CloseInput()
    {
        if (inputDlg == null) return;
        inputDlg.QueueFree();
        inputDlg = null;
    }

    void OpenInput(bool force = false)
    {
        if (force) CloseInput();
        if (inputDlg is not null) return;

        int minOctave = HasOctave() ? 2 : -1;
        int maxOctave = HasOctave() ? 6 : -1;
        inputDlg = DialogueFactory.Instance.GetNoteNameInputDialogue(HasSharp(), HasFlat(), minOctave, maxOctave, SubmitNote);
    }

    public override void _Ready()
    {
        base._Ready();
        openInputButton.Pressed += () => { OpenInput(true); };
    }

    protected override void Start()
    {
        base.Start();
        OpenInput(); 
    }

    protected override void Stop()
    {
        base.Stop();
        CloseInput();
    }

}
