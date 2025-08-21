using Godot;
using System;

public partial class NoteNameExercise : NoteExerciseBase
{
    [Export] Button openInputButton;
    [Export] CheckBox octavesCheck;

    NoteNameInput inputDlg;


    public override void _Ready()
    {
        base._Ready();

    }

    protected override void Start()
    {
        base.Start();
        if (inputDlg is null)
        {
            int minOctave = 2;
            int maxOctave = 3;
            inputDlg = DialogueFactory.Instance.GetNoteNameInputDialogue(HasSharp(), HasFlat(), minOctave, maxOctave, null);
        }
    }

    protected override void Stop()
    {
        base.Stop();
        if (inputDlg is not null)
        {
            inputDlg.QueueFree();
        }
    }

}
