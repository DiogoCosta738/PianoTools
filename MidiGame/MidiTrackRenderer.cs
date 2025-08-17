using Godot;
using System;
using System.Collections.Generic;

public partial class MidiTrackRenderer : Node2D
{
	List<MidiNote> _notes;
	int track = 0;

	public void SetMidiNotes(List<MidiNote> notes, int track)
	{
		_notes = notes;
		QueueRedraw();
	}

	public void RenderNotes()
	{
		if (_notes is null) return;
		
		int offset_x = 0;
		int offset_y = 0 * 100;
		int note_thickness = 2;

		foreach(MidiNote note in _notes)
		{
			
			float x_scale = 0.1f;
			float y_scale = 5;
			float length = (note.endTime - note.startTime) * x_scale;
			float height = y_scale;
			float y_spacing = 1;
			float xx = offset_x + note.startTime * x_scale;
			float yy = offset_y + note.midiNote * height + y_spacing;
			Rect2 rect = new Rect2(xx, yy, length, height);
			DrawRect(rect, Colors.Red);
		}
		
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _Draw()
    {
        base._Draw();
		RenderNotes();
    }
}
