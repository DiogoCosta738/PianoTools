using Godot;
using System;

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
