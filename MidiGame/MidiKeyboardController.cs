using System;
using Godot;
using NAudio.Midi;

public partial class MidiKeyboardController : Node
{
    private MidiIn midiIn;

    public override void _Ready()
    {
        if (MidiIn.NumberOfDevices > 0)
        {
            midiIn = new MidiIn(0); // first MIDI device
            midiIn.MessageReceived += OnMidiMessageReceived;
            midiIn.Start();
        }
        else
        {
            GD.Print("No MIDI devices detected.");
        }
    }

    public Action<int, int> OnNoteDown;
    public Action<int> OnNoteUp;
    private void OnMidiMessageReceived(object sender, MidiInMessageEventArgs e)
    {
        // Process MIDI messages
        GD.Print($"Message: {e.MidiEvent}");
        if (e.MidiEvent is NoteOnEvent noteOn)
        {
            OnNoteDown?.Invoke(noteOn.NoteNumber, noteOn.Velocity);
        }
        else if (e.MidiEvent is NoteEvent noteOff && noteOff.Velocity == 0)
        {
            OnNoteUp?.Invoke(noteOff.NoteNumber);
        }
    }

    public override void _ExitTree()
    {
        midiIn?.Stop();
        midiIn?.Dispose();
    }
}