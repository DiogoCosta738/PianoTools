using Godot;
using System;
using NAudio.Midi;
using System.ComponentModel.DataAnnotations;
using System.Threading;

public partial class NotePlayExerciseController : Node
{
    [Export] MidiKeyboardController midiKeyboardController;
    [Export] StaffController staffController;
    [Export] PianoUIController pianoController;
    [Export] Label scoreLabel;
    [Export] RichTextLabel feedbackLabel;
    [Export] Button startButton, resetButton;
    [Export] CheckBox useLabelCheck, useSheetCheck, useSharpsCheck, useFlatsCheck;
    [Export] ColorRect waitBackground, waitForeground;

    int waitingNote;
    int correct = 0;
    int total = 0;

    double waitTarget;
    double curWait;

    double wrongWaitSeconds = 1, correctWaitSeconds = 1;
    Random rng;

    public static (string note, string accidental, int octave) GetNoteName(int midiNote)
    {
        string[] noteNames = { "C", "C", "D", "D", "E", "F", "F", "G", "G", "A", "A", "B" };
        string[] accidentals = { "", "♯", "", "♯", "", "", "♯", "", "♯", "", "♯", "" };

        int noteInOctave = midiNote % 12;
        int octave = (midiNote / 12) - 1;

        string name = noteNames[noteInOctave];
        string accidental = accidentals[noteInOctave];

        return (name, accidental, octave);
    }

    public override void _Ready()
    {
        base._Ready();
        midiKeyboardController.OnNoteDown += (note, vel) => CallDeferred("PlayedNote", note);
        startButton.Pressed += StartStop;
        resetButton.Pressed += Reset;
        rng = new Random();
        Reset();
    }

    void Reset()
    {
        correct = 0;
        total = 0;
        waitingNote = -1;
        UpdateScoreLabel();

        curWait = 0;
        waitTarget = 0;
        SetFeedback("Stand-by");
        UpdateWaitingBar();
    }

    void StartStop()
    {
        if (waitingNote >= 0)
        {
            waitingNote = -1;
            startButton.Text = "Start";
        }
        else
        {
            startButton.Text = "Stop";
            PickNewNote();
        }
    }

    void PickNewNote()
    {
        int noteMin = 24;
        int noteMax = 72;
        int noteRange = noteMax - noteMin;
        int newNote = -1;

        while (newNote == -1)
        {
            newNote = rng.Next() % noteRange + noteMin;

            var tuple = GetNoteName(newNote);
            string noteName = tuple.Item1;
            string accidental = tuple.Item2;
            int octave = tuple.Item3;

            if (accidental != "" && !useSharpsCheck.ButtonPressed && !useFlatsCheck.ButtonPressed)
            {
                newNote = -1;
            }
        }
        waitingNote = newNote;
        staffController.UpdateNote(newNote, 0, !useLabelCheck.ButtonPressed, !useSheetCheck.ButtonPressed);
        SetFeedback("Play note!");
    }

    void UpdateScoreLabel()
    {
        scoreLabel.Text = String.Format("Score: {0} / {1} ({2}%)", correct.ToString(), total.ToString(), total == 0 ? 100 : (correct + 0.0f) / total);
    }

    public void PlayedNote(int noteIndex)
    {
        if (waitingNote < 0 || IsWaiting()) return;
        if (noteIndex == waitingNote)
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

    Action OnDoneWaiting;
    void StartWaiting(double newTarget, Color color, Action onDoneWaiting)
    {
        if (IsWaiting())
            return;
        waitTarget = newTarget;
        curWait = 0;
        SetWaitingBarColor(Colors.Green);
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
