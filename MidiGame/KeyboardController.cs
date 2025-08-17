using Godot;
using System;
using System.Collections.Generic;

public partial class KeyboardController : Node
{
	[Export] bool active;

	string index_to_key = "qwertyuiopasdfghjklzxcvbnm";
	Dictionary<char, int> key_to_index = new Dictionary<char, int>();
	int _lowestNote = 21;

	public void SetLowestNote(int note)
	{
		_lowestNote = note;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for(int i = 0; i < index_to_key.Length; i++)
			key_to_index.Add(index_to_key[i], i);
	}

	public string GetNoteKey(int midiNote)
	{
		int index = midiNote - _lowestNote;
		if(index < 0 || index >= index_to_key.Length) return "N/A";
		return Char.ToUpper(index_to_key[index]).ToString();
	}

	private int CharToIndex(char c)
	{
		if(!key_to_index.ContainsKey(c)) return -1;
		return key_to_index[c];
	}

	private char IndexToChar(int idx)
	{
		if(idx < 0 && idx >= index_to_key.Length) return ' ';
		return index_to_key[idx];
	}

	public Action<int> OnNoteDown;
	public Action<int> OnNoteUp;

	public override void _Input(InputEvent @event)
	{
		if (!active) return;
		
		if (@event is InputEventKey inputEventKey)
		{
			InputEventKey keyEvent = (InputEventKey)@event;

			var keycode = DisplayServer.KeyboardGetKeycodeFromPhysical(inputEventKey.PhysicalKeycode);
			string keystr = OS.GetKeycodeString(keycode);

			if (keystr.Length == 1 && Char.IsLetter(keystr[0]))
			{
				int idx = CharToIndex(Char.ToLower(keystr[0]));
				if (idx == -1) return;
				int note = idx + _lowestNote;

				if (keyEvent.Pressed && !keyEvent.Echo) OnNoteDown(note);
				else if (!keyEvent.Pressed) OnNoteUp(note);
			}

		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
