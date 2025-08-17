using Godot;
using System;

public partial class MidiCamera : Camera2D
{
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		int vx = (Input.IsActionPressed("camera_move_right") ? 1 : 0) + (Input.IsActionPressed("camera_move_left") ? -1 : 0);
		int vy = (Input.IsActionPressed("camera_move_up") ? -1 : 0) + (Input.IsActionPressed("camera_move_down") ? 1 : 0);
		float speed = 3;

		if(Input.IsActionPressed("camera_fast")) speed *= 3f;
		if(Input.IsActionPressed("camera_slow")) speed /= 3f;
		Position += (new Vector2(vx,vy)).Normalized() * speed;

		if(Input.IsActionPressed("camera_reset"))
		{
			Position = Vector2.Zero;
		}

		if(Input.IsActionPressed("camera_zoom_out") || Input.IsActionJustPressed("camera_zoom_out"))
		{
			Zoom /= 1.3f;
		}

		if(Input.IsActionPressed("camera_zoom_in") || Input.IsActionJustReleased("camera_zoom_in"))
		{
			Zoom *= 1.3f;
		}
	}
}
