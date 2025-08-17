using System;
using NAudio.Wave;
using MeltySynth;
using Godot;

public class MidiSampleProvider : ISampleProvider
{
    private static WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    private Synthesizer synthesizer;
    private MidiFileSequencer sequencer;

    private object mutex;

    SoundFont _soundfont;
    public MidiSampleProvider(string soundFontPath)
    {
        _soundfont = new SoundFont(soundFontPath);
        foreach (var sample in _soundfont.SampleHeaders)
        {
            // GD.Print(sample.Name);
        }
        synthesizer = new Synthesizer(soundFontPath, format.SampleRate);
        sequencer = new MidiFileSequencer(synthesizer);

        mutex = new object();
    }

    public void Play(MidiFile midiFile, bool loop)
    {
        lock (mutex)
        {
            sequencer.Play(midiFile, loop);
        }
    }

    public void Stop()
    {
        lock (mutex)
        {
            sequencer.Stop();
        }
    }

    public int Read(float[] buffer, int offset, int count)
    {
        lock (mutex)
        {
            sequencer.RenderInterleaved(buffer.AsSpan(offset, count));
        }

        return count;
    }

    public void SetInstrument(int channel, int instrument_index)
    {
        synthesizer.ProcessMidiMessage(channel, 176, 0, 0); // set bank
        synthesizer.ProcessMidiMessage(channel, 192, instrument_index, -1); // set patch
    }


	public void SetVolume(int volume)
	{
		synthesizer.MasterVolume = volume;
	}

	public void PlayNote(int channel, int note, int velocity)
	{
		synthesizer.NoteOn(channel, note, velocity);
	}

	public void StopNote(int channel, int note)
	{
		synthesizer.NoteOff(channel, note);
	}

	public void StopAll(bool immediate = false)
	{
		synthesizer.NoteOffAll(immediate);
	}
	
	public void RenderWave()
	{
		float[] left = new float[format.SampleRate];
		float[] right = new float[format.SampleRate];
		synthesizer.Render(left, right);
	}

    public WaveFormat WaveFormat => format;
}