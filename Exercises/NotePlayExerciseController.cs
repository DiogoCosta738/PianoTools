using Godot;
using System;
using NAudio.Midi;
using System.ComponentModel.DataAnnotations;

public partial class NotePlayExerciseController : Node
{
    [Export] MidiKeyboardController midiKeyboardController;
    [Export] StaffController staffController;
    [Export] PianoUIController pianoController;
    [Export] Label scoreLabel;
    [Export] RichTextLabel feedbackLabel;
    [Export] Button startButton, resetButton;
    [Export] CheckBox useLabelCheck, useSheetCheck, useSharpsCheck, useFlatsCheck;

    int waitingNote;
    int correct = 0;
    int total = 0;

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
        feedbackLabel.Text = "Stand-by";
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
    }    

    void UpdateScoreLabel()
    {
        scoreLabel.Text = String.Format("Score: {0} / {1} ({2}%)", correct.ToString(), total.ToString(), total == 0 ? 100 : (correct + 0.0f) / total);
    }

    public void PlayedNote(int noteIndex)
    {
        if (waitingNote < 0) return;
        if (noteIndex == waitingNote)
        {
            feedbackLabel.Text = "[color=green]Great![/color]";
            correct++;
            total++;
            UpdateScoreLabel();
            PickNewNote();
        }
        else
        {
            feedbackLabel.Text = "[color=red]Wrong![/color]";
            total++;
            UpdateScoreLabel();
        }
    }
}
