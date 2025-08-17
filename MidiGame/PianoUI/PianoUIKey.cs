using Godot;
using NAudio.Midi;
using System;
using System.Collections;

public partial class PianoUIKey : Control
{
	[Export] Label labelText1, labelText2;

	static bool GLISSANDO = true;

	Rect2 _rect;
	int _note;
	bool _isSemitone;
	bool _isPlaying = false;
	bool _isHovering = false;
	ColorRect _collisionRect;

	string label1, label2;

	public string KeyLabel {
		get { return label1; }
		set { label1 = value; labelText1.Text = label1; }
	}

	public string NoteLabel {
		get { return label2; }
		set { label2 = value; labelText2.Text = label2; }
	}

	public void SetHover(bool hover)
	{
		_isHovering = hover;
		QueueRedraw();
	}

	private void HoverOn()
	{
		SetHover(true);
		OnMouseEnter();
	}

	private void HoverOff()
	{
		SetHover(false);
		OnMouseExit();
	}

	public void ShowKeyLabels(bool show)
	{
		labelText1.Visible = show;
	}

	public void ShowNoteLabels(bool show)
	{
		labelText2.Visible = show;
	}

	public void Update(Rect2 rect, int note, bool isSemitone)
	{
		_rect = rect;
		_note = note;
		_isSemitone = isSemitone;

		Position = _rect.Position;
		Size = _rect.Size;

		_rect = new Rect2(0,0, _rect.Size.X, _rect.Size.Y);
		_collisionRect.Position = _rect.Position;
		_collisionRect.Size = _rect.Size;

		labelText1.SelfModulate = _isSemitone ? Colors.White : Colors.Black;
		labelText2.SelfModulate = _isSemitone ? Colors.White : Colors.Black;
		
		QueueRedraw();
	}

	bool _dirty = true;
	public void KeyOn()
	{
		_isPlaying = true;
		_dirty = true;
	}

	public void KeyOff()
	{
		_isPlaying = false;
		_dirty = true;
	}
	
	public bool IsOn()
	{
		return _isPlaying;
	}

	public Action OnMouseEnter;
	public Action OnMouseExit;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// MouseEntered += HoverOn;
		// MouseExited += HoverOff;

		_collisionRect = new ColorRect();
		AddChild(_collisionRect);
		_collisionRect.Color = new Color(0,0,0,0);
		// _collisionRect.MouseFilter = MouseFilterEnum.Stop;
		_collisionRect.MouseEntered += HoverOn;
		_collisionRect.MouseExited += HoverOff;
		_collisionRect.MouseFilter = MouseFilterEnum.Pass;
		MouseFilter = MouseFilterEnum.Pass;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_dirty)
		{
			_dirty = false;
			QueueRedraw();
		}
	}

    public override void _Draw()
    {
		DrawRect(_rect, _isPlaying ? Colors.Cyan : _isSemitone ? Colors.Black : Colors.White);
		DrawRect(_rect, _isHovering ? Colors.DarkGray : Colors.Gray, false, 5);
    }
}
