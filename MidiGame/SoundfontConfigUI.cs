using Godot;
using System;

public partial class SoundfontConfigUI : Node
{
	[Export] Container container;

	[Export] Label octavesLabel;
	[Export] Button octaveUpButton;
	[Export] Button octaveDownButton;

	[Export] Label transposeLabel;
	[Export] Button transposeUpButton;
	[Export] Button transposeDownButton;

	[Export] Button hideButton;

	private bool _hidden = false;
	private Tween menuTween = null;

	private int lowest_octave = 2;
	public int Octave
	{
		get { return lowest_octave; }
		set { lowest_octave = value; }
	}

	private int transpose = 0;
	public int Transpose 
	{
		get { return transpose; }
		set { transpose = value; }
	}

	private int instrument;
	public int Instrument 
	{
		get { return instrument; }
		set { instrument = value; }
	}

	public Action OnNoteChange;
	
	public void SetOctave(int octave)
	{
		this.lowest_octave = octave;
		octavesLabel.Text = octave.ToString();
		OnNoteChange?.Invoke();
	}

	public void SetTranspose(int transpose)
	{
		this.transpose = transpose;
		transposeLabel.Text = transpose.ToString();
		OnNoteChange?.Invoke();
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hideButton.ButtonDown += ToggleHide;
		SetOctave(lowest_octave);
		SetTranspose(0);

		octaveDownButton.ButtonDown += () => SetOctave(lowest_octave - 1);
		octaveUpButton.ButtonUp += () => SetOctave(lowest_octave + 1);

		transposeDownButton.ButtonDown += () => SetTranspose(transpose - 1);
		transposeUpButton.ButtonUp += () => SetTranspose(transpose + 1);
	}

	private void ToggleHide()
	{
		if(menuTween != null)
		{
			menuTween.Kill();
			menuTween = null;
		}
		_hidden ^= true;
		Tween tween = GetTree().CreateTween();
		int yy = (int)hideButton.Position.Y;
		int orig_yy = yy;
		int target_yy = _hidden ? -yy : 0;
		float frac = Math.Abs(target_yy - container.Position.Y) / yy;
		float duration = 0.5f;
		tween.TweenProperty(container, "position", new Vector2(0, target_yy), frac * duration).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.InOut);
		tween.TweenCallback(Callable.From(() => menuTween = null));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
