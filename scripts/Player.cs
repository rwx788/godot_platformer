using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const int MAX_SPEED = 600;

	[Export] public bool jumping = false;
	[Export] public int move_speed = 250;
	[Export] public int jump_force = 700;
	[Export] public int gravity = 1800;
	
	public ulong prev_vert_wall = 0;

	public Vector2 ProcessJump(Vector2 velocity)
	{
		velocity.Y = -jump_force;
		jumping = true;
		return velocity;
	}

	public override void _Ready()
	{
		
	}
	public override void _PhysicsProcess(double delta)
	{
		UpDirection = Vector2.Up;
		//WallMinSlideAngle = Mathf.DegToRad(80);
		MotionMode = MotionModeEnum.Grounded;
		//FloorMaxAngle = Mathf.DegToRad(80);
		Vector2 velocity = Velocity;
		
		// Handle horizontal movement
		float direction_x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
		velocity.X = direction_x * move_speed;

		// Apply gravity only when not on the floor
		if (!IsOnFloor())
		{
			velocity.Y += gravity * (float)delta;
		}

		// Clamp velocity to MAX_SPEED
		velocity.X = Mathf.Clamp(velocity.X, -MAX_SPEED, MAX_SPEED);
		velocity.Y = Mathf.Clamp(velocity.Y, -MAX_SPEED, MAX_SPEED);

		// Ensure we correctly detect the floor
		if (IsOnFloor())
		{
			velocity.Y = 0;
			jumping = false;
			prev_vert_wall = 0;
		}

		// Handle jumping logic
		if (Input.IsActionJustPressed("jump"))
		{
			if (IsOnFloor())
			{
				velocity = ProcessJump(velocity);
			}
			else if (IsOnWall() && jumping)
			{
				// Wall jump logic using GetLastSlideCollision()
				var collision = GetLastSlideCollision();
				if (collision != null && collision.GetCollider() is StaticBody2D collider)
				{
					ulong colliderId = collider.GetInstanceId();
					if (colliderId != prev_vert_wall)
					{
						velocity = ProcessJump(velocity);
						prev_vert_wall = colliderId;
					}
				}
			}
		}
		// Set the velocity and handle movement
		Velocity = velocity;
		
		// Use MoveAndSlide correctly for collision detection and floor check
		MoveAndSlide();
	}

	public void Collide()
	{
		// Placeholder for additional collision logic
	}
}
