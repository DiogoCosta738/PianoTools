using Godot;
using System;
using NAudio.Midi;
using System.ComponentModel.DataAnnotations;
using System.Threading;

public class Note
{
    protected int noteTone = 0; // C
    protected int octave = 4;
    protected string accidental = "";

    public Note(int tone, int oct, string acc)
    {
        noteTone = tone;
        octave = oct;
        accidental = acc;
    }

    public Note Clone()
    {
        return new Note(noteTone, octave, accidental);
    }
    
    // !TODO: I may want a different Equals method that compares either the sound or the notation
    public override bool Equals(object obj)
    {
        if (obj is Note otherNote)
            return noteTone == otherNote.noteTone && octave == otherNote.octave && accidental == otherNote.accidental;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(noteTone, octave, accidental);
    }

    public static bool operator ==(Note left, Note right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    // Inequality operator
    public static bool operator !=(Note left, Note right) => !(left == right);

    public string GetNameShort() { return GetToneLetter() + accidental + octave.ToString(); }

    public int GetToneIndex() { return noteTone; }

    public char GetToneLetter() { return NoteUtils.GetToneLetter(noteTone); }

    public int GetOctave() { return octave; }

    public void SetOctave(int octave) { this.octave = octave; }

    public bool HasAccidental() { return accidental != ""; }

    public string GetAccidental() { return accidental; }
    public void SetAccidental(string accidental) { this.accidental = accidental; }

    public int ToMidiNote() { return NoteUtils.ToMidiNote(this); }
}

public partial class NoteExerciseBase : Node
{
    [Export] protected StaffController staffController;
    [Export] protected Label scoreLabel;
    [Export] protected RichTextLabel feedbackLabel;
    [Export] protected Button startButton, resetButton;
    [Export] protected CheckBox useLabelCheck, useSheetCheck, useSharpsCheck, useFlatsCheck;
    [Export] protected ColorRect waitBackground, waitForeground;

    protected Note waitingNote;
    protected int correct = 0;
    protected int total = 0;

    protected double waitTarget;
    protected double curWait;
    protected double wrongWaitSeconds = 1, correctWaitSeconds = 1;

    protected Random rng;

    Action OnDoneWaiting;

    protected bool HasSharp() { return useSharpsCheck.ButtonPressed; }
    protected bool HasFlat() { return useFlatsCheck.ButtonPressed; }
    protected bool RenderName() { return useLabelCheck.ButtonPressed; }
    protected bool RenderNote() { return useSheetCheck.ButtonPressed; }

    public override void _Ready()
    {
        base._Ready();
        startButton.Pressed += StartStop;
        resetButton.Pressed += Reset;
        rng = new Random();
        Reset();
    }

    protected virtual void Reset()
    {
        if (waitingNote is not null) staffController.RemoveNote(waitingNote, 0);
        waitingNote = null;
        ResetScore();
        ResetWaiting();
        SetFeedback("Stand-by");
    }

    void ResetScore()
    {
        correct = 0;
        total = 0;
        UpdateScoreLabel();
    }

    protected virtual void Start()
    {
        startButton.Text = "Stop";
        PickNewNote();
    }

    protected virtual void Stop()
    {
        if (waitingNote is not null) staffController.RemoveNote(waitingNote, 0);
        waitingNote = null;
        startButton.Text = "Start";
    }


    void StartStop()
    {
        if (waitingNote is not null)
        {
            Stop();
        }
        else
        {
            Start();
        }
    }

    protected virtual Note GenerateNote()
    {
        int noteMin = 24;
        int noteMax = 72;
        int noteRange = noteMax - noteMin;
        int newNote = -1;
        while (newNote == -1)
        {
            newNote = rng.Next() % noteRange + noteMin;
            if (NoteUtils.HasAccidental(newNote) && !HasFlat() && !HasSharp())
                newNote = -1;
        }
        return NoteUtils.FromMidiNote(newNote, preferFlat: HasFlat() && (!HasSharp() || rng.Next() % 2 == 1));
    }

    void PickNewNote()
    {
        if (waitingNote is not null) staffController.RemoveNote(waitingNote, 0);
        waitingNote = GenerateNote();
        staffController.HideLabel(0, !RenderName());
        staffController.HideNote(0, !RenderNote());
        staffController.AddNote(waitingNote, 0);
        SetFeedback("Play note!");
    }

    void UpdateScoreLabel()
    {
        scoreLabel.Text = String.Format("Score: {0} / {1} ({2}%)", correct.ToString(), total.ToString(), total == 0 ? 100 : (correct + 0.0f) / total);
    }

    protected virtual bool MatchesWaitingNote(Note note)
    {
        return note.ToMidiNote() == waitingNote.ToMidiNote();
    }

    public void SubmitNote(int midiNote)
    {
        SubmitNote(NoteUtils.FromMidiNote(midiNote));
    }

    public void SubmitNote(int noteTone, int octave, string accidental)
    {
        SubmitNote(noteTone, octave, accidental);
    }

    public virtual void OnSubmitNote(Note note) {}
    public virtual void OnCorrectNote(Note note) { }
    public virtual void OnWrongNote(Note note) {}

    public void SubmitNote(Note note)
    {
        OnSubmitNote(note);
        if (waitingNote is null || IsWaiting()) return;

        if (MatchesWaitingNote(note))
        {
            SetFeedback("[color=green]Great![/color]");
            correct++;
            total++;
            UpdateScoreLabel();
            OnCorrectNote(note);
            StartWaiting(correctWaitSeconds, Colors.Green, () => PickNewNote());
        }
        else
        {
            SetFeedback("[color=red]Wrong![/color]");
            total++;
            UpdateScoreLabel();
            OnWrongNote(note);
            StartWaiting(wrongWaitSeconds, Colors.Red, () => SetFeedback("Try again!"));
        }
    }

    void SetFeedback(string feedback)
    {
        feedbackLabel.Text = feedback;
    }

    void ResetWaiting()
    {
        curWait = 0;
        waitTarget = 0;
        UpdateWaitingBar();
    }

    void StartWaiting(double newTarget, Color color, Action onDoneWaiting)
    {
        if (IsWaiting())
            return;
        waitTarget = newTarget;
        curWait = 0;
        SetWaitingBarColor(color);
        OnDoneWaiting = onDoneWaiting;
    }

    protected bool IsWaiting()
    {
        return curWait < waitTarget;
    }

    void SetWaitingBarColor(Color color)
    {
        waitForeground.Color = color;
    }

    void UpdateWaitingBar()
    {
        if (IsWaiting())
        {
            waitForeground.Size = new Vector2(waitBackground.Size.X * (float)(1 - curWait / waitTarget), waitBackground.Size.Y);
            waitForeground.Position = new Vector2(0, 0);
        }
        else
        {
            waitForeground.Size = new Vector2(0, 0);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (IsWaiting())
        {
            curWait += delta;
            UpdateWaitingBar();
            if (!IsWaiting())
                OnDoneWaiting?.Invoke();
        }
    }
}
