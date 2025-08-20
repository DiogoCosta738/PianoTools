using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class PianoUIController : Control
{
	[Export] bool mouseInput = true;
	[Export(PropertyHint.File)] string pianoKeyScene;
	private Vector4I _margins = new Vector4I(1,1,1,1) * 20;
	private int _octaves = 3;
	private int _lowestNote = -1;

	public Action<int> OnNoteDown, OnNoteUp;

	private void Init()
	{
		pianoKeys = new PianoUIKey[_octaves * 12];
		for(int i = 0; i < _octaves * 12; i++)
		{
			int local_i = i;
			//pianoKeys[i] = new PianoUIKey();
			PackedScene keyScene = GD.Load<PackedScene>(pianoKeyScene);
			pianoKeys[i] = keyScene.Instantiate<PianoUIKey>();
			pianoKeys[i].OnMouseEnter += () => OnMouseEnterKey(local_i);
			pianoKeys[i].OnMouseExit += () => OnMouseExitKey(local_i);
			AddChild(pianoKeys[i]);
		}

		int cur_tone = 0;
		int cur_note = 0;
		for(int oct = 0; oct < _octaves; oct++)
		{
			for(int k = 0; k < 7; k++)
			{
				if(has_sharp[cur_tone % 7])
				{
					cur_note++;
					RemoveChild(pianoKeys[cur_note]);
					AddChild(pianoKeys[cur_note]);
				}
				cur_note++;
				cur_tone++;
			}
		}
	}

	public void ShowKeyLabels(bool show)
	{
		foreach(var key in pianoKeys) key.ShowKeyLabels(show);
	}

	public void ShowNoteLabels(bool show)
	{
		foreach(var key in pianoKeys) key.ShowNoteLabels(show);
	}

	public List<int> GetNotesList()
	{
		List<int> notes = new List<int>();
		for(int i = 0; i < _octaves * 12; i++)
			notes.Add(_lowestNote + i);
		return notes;
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

	bool[] has_sharp =
	{
		true,	// C
		true, 	// D
		false, 	// E
		true,	// F
		true,	// G
		true,	// A
		false,	// B
	};

	private PianoUIKey[] pianoKeys;
	private int _mousePlayedKey;
	private int _hoveredKey;
	private bool GLISSANDO = false;

	public void SetLowestNote(int lowestNote)
	{
		if(lowestNote == _lowestNote) return;

		List<int> prevOnKeys = new List<int>();
		for (int i = 0; i < pianoKeys.Length; i++)
		{
			if (pianoKeys[i].IsOn())
			{
				prevOnKeys.Add(i);
				pianoKeys[i].KeyOff();
			}
		}

		int delta = lowestNote - _lowestNote;
		_lowestNote = lowestNote;
		
		foreach(var note in prevOnKeys)
		{
			int movedIndex = note - delta;
			if(movedIndex >= 0 && movedIndex < pianoKeys.Length)
			{
				pianoKeys[movedIndex].KeyOn();
			}
		}

		for(int i = 0; i < pianoKeys.Length; i++)
		{
			pianoKeys[i].NoteLabel = GetNoteNameShort(_lowestNote + i);
		}
	}

	static string[] noteShortName = {
		"C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
	};

	static bool[] noteAccidental = {
		false, true, false, true, false, false, true, false, true, false, true, false
	};

	public static string GetNoteNameShort(int midiNote)
	{
		int octave = (midiNote) / 12;
		int semitone = midiNote % 12;
		return $"{noteShortName[semitone]}{octave}";
	}
	
	public static bool HasAccidental(int midiNote)
	{
		int octave = (midiNote) / 12;
		int semitone = midiNote % 12;
		return noteAccidental[semitone];
	}

	private int NoteToKeyIndex(int midiNote)
	{
		int idx = midiNote - _lowestNote;
		if (idx < 0 || idx > pianoKeys.Length) return -1;
		return idx;
	}

	public void NoteOn(int midiNote)
	{
		int idx = NoteToKeyIndex(midiNote);
		if(idx == -1) return;
		if (idx >= pianoKeys.Length) return;

		pianoKeys[idx].KeyOn();
	}

	public void NoteOff(int midiNote)
	{
		int idx = NoteToKeyIndex(midiNote);
		if(idx == -1) return;
		if (idx >= pianoKeys.Length) return;

		pianoKeys[idx].KeyOff();
	}

	public void NoteOffAll()
	{
		foreach(var key in pianoKeys) key.KeyOff();
	}

	public void SetKeyLabel(int midiNote, string label)
	{
		int idx = NoteToKeyIndex(midiNote);
		if(idx == -1) return;

		pianoKeys[idx].KeyLabel = label;
	}


	
	private void MouseKeyOn(int idx)
	{
		OnNoteDown?.Invoke(_lowestNote + idx);
		pianoKeys[idx].KeyOn();
		_mousePlayedKey = idx;
	}

	private void MouseKeyOff(int idx)
	{
		OnNoteUp?.Invoke(_lowestNote + idx);
		pianoKeys[idx].KeyOff();
		_mousePlayedKey = -1;
	}

	private void OnMouseEnterKey(int idx)
	{
		_hoveredKey = idx;
		
		if(GLISSANDO && Input.IsMouseButtonPressed(MouseButton.Left)) 
		{
			if(_mousePlayedKey != -1)
			{
				MouseKeyOff(_mousePlayedKey);
			}
			MouseKeyOn(idx);
		}
	}

	private void OnMouseExitKey(int idx)
	{
		if(_hoveredKey == idx) _hoveredKey = -1;
		if(GLISSANDO)
		{
			if(_mousePlayedKey == idx)
			{
				MouseKeyOff(_mousePlayedKey);
			}
		}
	}

	private void OnMouseDown()
	{
		if(_mousePlayedKey != -1)
		{
			MouseKeyOff(_mousePlayedKey);
		}
		if(_hoveredKey != -1)
		{
			MouseKeyOn(_hoveredKey);
		}
	}

	private void OnMouseUp()
	{
		if(_mousePlayedKey != -1)
		{
			MouseKeyOff(_mousePlayedKey);
		}
	}

	public override void _Input(InputEvent @event)
    {
		if (!mouseInput) return;
        // Check for InputEventMouseButton
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			InputEventMouseButton ev = (InputEventMouseButton)@event;
			// Check if left mouse button is pressed
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseButtonEvent.IsPressed())
				{
					OnMouseDown();
				}
				else
				{
					OnMouseUp();
				}
			}
		}
    }

	public override void _Draw()
	{
		Vector2I size = GetWindow().Size;
		float xx = _margins.X;
		float key_size_x = (size.X - _margins.X * 2) / (7 * _octaves);
		float semitone_key_size_x = key_size_x * 0.8f;
		float key_size_y = 200;
		float semitone_key_size_y = key_size_y * 0.6f;
		int cur_tone = 0;
		int cur_note = 0;
		for(int oct = 0; oct < _octaves; oct++)
		{
			for(int k = 0; k < 7; k++)
			{
				Rect2 rect = new Rect2(xx, size.Y - _margins.W - key_size_y, key_size_x - 5, key_size_y);
				pianoKeys[cur_note].Update(rect, cur_note, false);

				if(has_sharp[cur_tone % 7])
				{
					cur_note++;

					float xx1 = xx;
					xx1 += key_size_x;
					xx1 -= semitone_key_size_x / 2;
					rect = new Rect2(xx1, size.Y - _margins.W - key_size_y, semitone_key_size_x - 5, semitone_key_size_y);
					pianoKeys[cur_note].Update(rect, cur_note, true);
				}

				cur_note++;
				cur_tone++;
				xx += key_size_x;
			}
		}
	}
}
