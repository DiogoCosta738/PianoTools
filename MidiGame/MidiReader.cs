using Godot;
using System;
using System.Xml.XPath;
using MidiParser;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Channels;

public class MidiNote
{
	public int startTime;
	public int endTime;
	public int midiNote;
	public int channel;

	public override string ToString()
	{
		return string.Format("Channel: {0}, Note: {1}, Start: {2}, End: {3}.", channel, midiNote, startTime, endTime);
	}
}

public partial class MidiReader : Node
{
	[Export] MidiTrackRenderer renderer;
	[Export(PropertyHint.File)] public string midiFileAccess;

	public Dictionary<int, List<MidiNote>> _notes;

	public void Init()
	{
		if (midiFileAccess is not null)
		{
			InitFromMidiTrack();
			return;
		}
	}

	public void InitFromMidiTrack()
	{
		GD.Print("MidiFileAccessPath:", midiFileAccess);
		MidiFile midiFile = new MidiFile(ProjectSettings.GlobalizePath(midiFileAccess));

		// 0 = single-track, 1 = multi-track, 2 = multi-pattern
		int midiFileformat = midiFile.Format;
		// also known as pulses per quarter note
		int ticksPerQuarterNote = midiFile.TicksPerQuarterNote;

		Dictionary<int, Dictionary<int, MidiNote>> openNotes = new Dictionary<int, Dictionary<int, MidiNote>>();
		Dictionary<int, List<MidiNote>> midiNotes = new Dictionary<int, List<MidiNote>>();
		GD.Print("Tracks:", midiFile.Tracks.Length);
		int trackIdx = 0;
		foreach (MidiTrack track in midiFile.Tracks)
		{
			// GD.Print("MidiEvents in Track:", track.MidiEvents.Count);
			foreach (MidiEvent midiEvent in track.MidiEvents)
			{
				// GD.Print("MIDI EVENT TYPE:", midiEvent.MidiEventType);
				if (midiEvent.MidiEventType == MidiEventType.NoteOn)
				{
					var channel = midiEvent.Channel;
					var note = midiEvent.Note;
					// GD.Print(string.Format("Midi NoteOn. Channel: {0}, Note: {1}", channel, note));
					if (!openNotes.ContainsKey(trackIdx))
						openNotes.Add(trackIdx, new Dictionary<int, MidiNote>());
					if (!midiNotes.ContainsKey(trackIdx))
						midiNotes.Add(trackIdx, new List<MidiNote>());
					// Debug.Assert(!openNotes[trackIdx].ContainsKey(note));
					MidiNote midiNote = new MidiNote();
					midiNote.channel = channel;
					midiNote.midiNote = note;
					midiNote.startTime = midiEvent.Time;
					if (openNotes[trackIdx].ContainsKey(note))
					{
						GD.Print("Skipping double note on...");
						// openNotes[trackIdx][note].endTime = midiEvent.Time;
						// openNotes[trackIdx].Remove(note);
					}
					else
					{
						openNotes[trackIdx].Add(note, midiNote);
						midiNotes[trackIdx].Add(midiNote);
					}
				}
				else if (midiEvent.MidiEventType == MidiEventType.NoteOff)
				{
					int channel = midiEvent.Channel;
					int note = midiEvent.Note;
					int time = midiEvent.Time;
					// GD.Print(string.Format("Midi NoteOff. Channel: {0}, Note: {1}", channel, note));
					// Debug.Assert(openNotes.ContainsKey(trackIdx) && openNotes[trackIdx].ContainsKey(note));
					if (!(openNotes.ContainsKey(trackIdx) && openNotes[trackIdx].ContainsKey(note)))
					{
						GD.Print("Skipping note off...");
					}
					else
					{
						openNotes[trackIdx][note].endTime = midiEvent.Time;
						openNotes[trackIdx].Remove(note);
					}
				}
				else if (midiEvent.MidiEventType == MidiEventType.PitchBendChange)
				{

				}
			}

			foreach (var textEvent in track.TextEvents)
			{
				if (textEvent.TextEventType == TextEventType.Lyric)
				{
					var time = textEvent.Time;
					var text = textEvent.Value;
				}
			}
			trackIdx++;
		}
		foreach (int track in midiNotes.Keys)
		{
			// GD.Print("Track: ", track);
			foreach (MidiNote note in midiNotes[track])
			{
				// GD.Print("\t", note.ToString());
			}
		}
		_notes = midiNotes;
		renderer.SetMidiNotes(_notes[2], 0);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Init();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
