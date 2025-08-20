using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class NoteNameInput : Control
{
    [Export] Control octavesPanel;
    [Export] Label octavesLabel;
    [Export] Slider octavesSlider;
    [Export] Control octavesContainer;

    [Export] Control notesPanel;
    [Export] Label noteLabel;
    [Export] GridContainer buttonContainer;


    bool includeOctave;
    bool includeSharps;
    bool includeFlats;

    List<Button> sharpButtons, flatButtons, normalButtons;
    List<Label> octaveLabels;
    int minOctave = 1, maxOctave = 8;
    int curMinOctave = 1, curMaxOctave = 8;

    Action<int, string, int> OnSubmit;

    public override void _Ready()
    {
        base._Ready();
        sharpButtons = new List<Button>();
        flatButtons = new List<Button>();
        normalButtons = new List<Button>();
        string notes = "CDEFGAB";
        for (int acc = 0; acc < 3; acc++)
        {
            List<Button> list = acc == 0 ? flatButtons : acc == 1 ? normalButtons : sharpButtons;
            string accidental = acc == 0 ? "b" : acc == 1 ? "" : "#";
            for (int i = 0; i < 7; i++)
            {
                Button btn = new Button();
                buttonContainer.AddChild(btn);
                btn.Text = notes[i] + accidental;
                btn.Pressed += () => Submit(i, accidental);
                btn.CustomMinimumSize = new Vector2(75, 50);
                list.Add(btn);
            }
        }

        octaveLabels = new List<Label>();
        for (int i = minOctave; i <= maxOctave; i++)
        {
            Label label = new Label();
            label.Text = i.ToString();
            octaveLabels.Add(label);
            octavesContainer.AddChild(label);
        }
        octavesSlider.MinValue = minOctave;
        octavesSlider.MaxValue = maxOctave;
        octavesSlider.Step = 1;
        octavesSlider.ValueChanged += (val) => UpdateOctave(Mathf.RoundToInt(val));

        curMinOctave = minOctave;
        curMaxOctave = maxOctave;
        UpdateOctave(1);
        CallDeferred("RepositionOctaveLabels");
    }

    void RepositionOctaveLabels()
    {
        float margin = 8;
        float width = octavesContainer.Size.X - octaveLabels[0].Size.X - margin / 2;
        float step = width / (maxOctave - minOctave);
        float minX = margin;
        for (int i = 0; i <= maxOctave - minOctave; i++)
        {
            float w = octaveLabels[i].Size.X;
            octaveLabels[i].Position = new Vector2(minX + step * i - w / 2, octaveLabels[i].Position.Y);
        }
        octavesContainer.CustomMinimumSize = new Vector2(0, octaveLabels[0].Size.Y);
    }

    void UpdateOctave(int octave)
    {
        octave = Mathf.Clamp(octave, curMinOctave, curMaxOctave);
        octavesSlider.Value = octave;
        octavesLabel.Text = "Octave: " + octave.ToString();
    }

    void Submit(int note, string accidental)
    {
        int octave = Mathf.RoundToInt(octavesSlider.Value);
        OnSubmit?.Invoke(note, accidental, octave);
    }

    public void Setup(bool hasSharp, bool hasFlat, int octaveMin, int octaveMax, Action<int, string, int> OnSubmit)
    {
        this.OnSubmit = OnSubmit;
        for (int i = 0; i < 7; i++)
        {
            sharpButtons[i].Visible = hasSharp;
            flatButtons[i].Visible = hasFlat;
        }

        if (octaveMin <= 0 || octaveMax <= 0)
        {
            octavesPanel.Visible = false;
        }
        else
        {
            octavesPanel.Visible = true;
            octaveMin = Math.Max(1, octaveMin);
            octaveMax = Math.Min(8, octaveMax);
            curMinOctave = octaveMin;
            curMaxOctave = octaveMax;
            for (int i = 0; i < 8; i++)
            {
                bool inRange = (i + 1) >= octaveMin && (i + 1) <= octaveMax;
                octaveLabels[i].Modulate = inRange ? Colors.White : Colors.Gray;
            }
            UpdateOctave(Mathf.RoundToInt(octavesSlider.Value));
        }
    }
}
