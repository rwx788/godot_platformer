using System;
using System.Linq;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;


public class Player : KinematicBody2D
{
	 
	public const int MAX_SPEED = 600;
	
	[Export] public bool jumping  = false;
	[Export] public int move_speed  = 250;
	[Export] public int jump_force  = 700;
	[Export] public int gravity  = 1800;
	
	public Vector2 speed  = new Vector2();
	public Vector2 velocity  = new Vector2();
	public Vector2 snap_vector  = new Vector2(0, 32);
	public ulong prev_vert_wall  = 0;
	
	public void ProcessJump()
	{  
		speed.y = -jump_force;
		jumping = true;	
	}
	
	public override void _PhysicsProcess(float delta)
	{  
		var direction_x  = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
		
		speed.x = direction_x * move_speed	;
		speed.y += gravity * delta;
		// Limit maximum speed
		speed.x = Mathf.Clamp(speed.x, -MAX_SPEED, MAX_SPEED);
		speed.y = Mathf.Clamp(speed.y, -MAX_SPEED, MAX_SPEED);
	
		velocity = MoveAndSlideWithSnap( speed, jumping ? speed : snap_vector, Vector2.Up, false, 4, Mathf.Deg2Rad(46), true );
		
		if(IsOnFloor())
		{
				velocity.y = 0;
				jumping = false;
				prev_vert_wall = 0	;
				
		}
		if(Input.IsActionJustPressed("jump"))		
		{
			// Is on floor, allow jumping	
			if(IsOnFloor())
			{
				ProcessJump();
			// Allow wall jumps only when in air already
			}
			else if(IsOnWall() && jumping)
			{
				// Do !allow wall jumps from the same wall
				foreach(var i in Enumerable.Range(0, GetSlideCount()))
				{
					var collision = GetSlideCollision(i);
					if(collision.ColliderId != prev_vert_wall && collision.Collider.IsClass("StaticBody2D"))	
					{
						ProcessJump();
						prev_vert_wall = collision.ColliderId;
						break;
	
					}
				}
			}
		}
	}
	
	public void collide()
	{  
	
	
	}
	
}