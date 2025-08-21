using Godot;
using System;
using NAudio.Midi;
using System.ComponentModel.DataAnnotations;
using System.Threading;

public class Note
{
    int noteLetterIndex = 0; // C
    int octave = 4;
    string accidental = "";

    public Note(int letterIndex, int oct, string acc)
    {
        noteLetterIndex = letterIndex;
        octave = oct;
        accidental = acc;
    }

    public string GetNameShort()
    {
        return GetNoteLetter() + accidental + octave.ToString();
    }

    public bool HasAccidental()
    {
        return accidental != "";
    }

    public int GetNoteLetterIndex()
    {
        return noteLetterIndex;
    }

    public char GetNoteLetter()
    {
        return NoteUtils.GetNoteLetter(noteLetterIndex);
    }

    public int GetOctave()
    {
        return octave;
    }

    public string GetAccidental()
    {
        return accidental;
    }

    public int ToMidiNote()
    {
        return NoteUtils.ToMidiNote(this);
    }
}

public partial class NoteExerciseBase : Node
{
    [Export] StaffController staffController;
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
            if (NoteUtils.HasAccidental(newNote) && !HasSharp() && !HasFlat())
            {
                newNote = -1;
            }
        }
        return NoteUtils.FromMidiNote(newNote, false);
    }

    void PickNewNote()
    {
        waitingNote = GenerateNote();
        staffController.UpdateNote(waitingNote, 0, !useLabelCheck.ButtonPressed, !useSheetCheck.ButtonPressed);
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

    public void SubmitNote(int noteLetterIndex, int octave, string accidental)
    {
        SubmitNote(noteLetterIndex, octave, accidental);
    }

    public void SubmitNote(Note note)
    {
        if (waitingNote is null || IsWaiting()) return;
        if (MatchesWaitingNote(note))
        {
            SetFeedback("[color=green]Great![/color]");
            correct++;
            total++;
            UpdateScoreLabel();
            StartWaiting(correctWaitSeconds, Colors.Green, () => PickNewNote());
        }
        else
        {
            SetFeedback("[color=red]Wrong![/color]");
            total++;
            UpdateScoreLabel();
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

    bool IsWaiting()
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
