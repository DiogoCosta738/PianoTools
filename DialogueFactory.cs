using Godot;
using System;
using System.Diagnostics;

public partial class DialogueFactory : Node
{
    [Export] Control dialogueContainer;
    [Export] PackedScene noteNameInputScene;

    public static DialogueFactory Instance;

    public override void _Ready()
    {
        base._Ready();
        Debug.Assert(Instance == null);
        Instance = this;
    }

    public NoteNameInput GetNoteNameInputDialogue(bool hasSharp, bool hasFlat, int minOctave, int maxOctave, Action<Note> onNotePicked, Control parent = null)
    {
        NoteNameInput dlg = noteNameInputScene.Instantiate<NoteNameInput>();
        parent = parent is null ? dialogueContainer : parent;
        parent.AddChild(dlg);
        dlg.Setup(hasSharp, hasFlat, minOctave, maxOctave, onNotePicked);
        return dlg;
    }
}
