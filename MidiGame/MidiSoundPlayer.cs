using Godot;
using System;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using System.Linq;
using NAudio.Wave;
using System.IO;
using MeltySynth;
using NAudio.Midi;

public partial class MidiSoundPlayer : Node
{
	[Export] PianoUIController pianoController;
	[Export] OptionButton instrumentOptions;
	[Export] KeyboardController keyboardController;
	[Export] SoundfontConfigUI soundfontConfigUI;
	[Export] MidiKeyboardController midiInputController;
	[Export] StaffController staffController;

	[Export(PropertyHint.File)] string _soundfontPath;
	[Export(PropertyHint.File)] string _midiPath;
	[Export] CheckBox preferFlat;

	List<MMDevice> _devices;
	MMDevice _selectedDevice;

	private WasapiOut _waveOut;
	private MidiSampleProvider _player;

	private int _channel;
	private int _instrument;
	private List<string> _instrumentNames = new List<string>();

	bool waveRendered = false;

	List<int> notesDown;

	private List<MMDevice> GetAudioDevices()
	{
		MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
		return enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
	}

	public int GetLowestNote()
	{
		return soundfontConfigUI.Octave * 12 + soundfontConfigUI.Transpose;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		notesDown = new List<int>();

		string soundFontPath = ProjectSettings.GlobalizePath(_soundfontPath);
		SoundFont soundfont = new SoundFont(soundFontPath);
		instrumentOptions.Clear();
		_instrumentNames.Clear();
        foreach (var instrument in soundfont.Presets)
        {
			_instrumentNames.Add(instrument.Name);
			instrumentOptions.AddItem(instrument.Name);
        }
		instrumentOptions.ItemSelected += SetInstrument;
		_player = new MidiSampleProvider(soundFontPath);
		
		_devices = GetAudioDevices();
		_selectedDevice = _devices[1];
		_waveOut = new WasapiOut(AudioClientShareMode.Shared, true, 20);
		_waveOut.Init(_player);
		_waveOut.Play();

		keyboardController.SetLowestNote(GetLowestNote());
		pianoController.SetLowestNote(GetLowestNote());

		keyboardController.OnNoteDown += (note) => PlayNote(note);
		keyboardController.OnNoteUp += StopNote;
		pianoController.OnNoteDown += (note) => PlayNote(note);
		pianoController.OnNoteUp += StopNote;

		midiInputController.OnNoteDown += PlayNote;
		midiInputController.OnNoteUp += StopNote;

		soundfontConfigUI.OnNoteChange += UpdateNotes; 

		GetTree().CreateTimer(0.1f).Timeout += () => {
			List<int> pianoNotes = pianoController.GetNotesList();
			foreach(int note in pianoNotes)
			{
				string key = keyboardController.GetNoteKey(note);
				pianoController.SetKeyLabel(note, key);
			}
		};
	}

	private void UpdateNotes()
	{
		_player.StopAll();
		pianoController.NoteOffAll();
		keyboardController.SetLowestNote(GetLowestNote());
		pianoController.SetLowestNote(GetLowestNote());
	}

    private void PlayMidiFile(string file)
	{
		FileStream fs = new FileStream(ProjectSettings.GlobalizePath(file), FileMode.Open);
		MeltySynth.MidiFile midiFile = new MeltySynth.MidiFile(fs);
		_player.Play(midiFile, true);
	}

	private void SetInstrument(long index)
	{
		int idx = (int)index;
		_instrument = idx;
		_player.SetInstrument(_channel, idx);
	}

	public void SetChannel(int channel)
	{
		_channel = channel;
	}

	public void PlayNote(int midiNote, int velocity = 100)
	{
		GD.Print("Note on: ", midiNote, " ", velocity);
		_player.PlayNote(_channel, midiNote, velocity);
		pianoController.NoteOn(midiNote);

		if (notesDown.Contains(midiNote)) notesDown.Remove(midiNote);
		notesDown.Add(midiNote);

		Note note = NoteUtils.FromMidiNote(midiNote, preferFlat.ButtonPressed);
		staffController.CallDeferred("AddNote", note.GetToneIndex(), note.GetOctave(), note.GetAccidental());
	}

	public void StopNote(int midiNote)
	{
		GD.Print("Note off: ", midiNote);
		_player.StopNote(_channel, midiNote);
		pianoController.NoteOff(midiNote);

		if (notesDown.Contains(midiNote)) notesDown.Remove(midiNote);
		int nextMidiNote = -1;
		if (notesDown.Count != 0) nextMidiNote = notesDown[notesDown.Count - 1];

		Note note = NoteUtils.FromMidiNote(midiNote, preferFlat.ButtonPressed);
		staffController.CallDeferred("RemoveNote", note.GetToneIndex(), note.GetOctave(), note.GetAccidental());
	}

	public void SetVolume(int vol)
	{
		_player.SetVolume(vol);
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!waveRendered) 
		{
			waveRendered = true;
			// _player.RenderWave();
		}
	}
}
