using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private const int MaxSpeed = 600;

	[Export] public bool Jumping = false;
	[Export] public int MoveSpeed = 250;
	[Export] public int JumpForce = 700;
	[Export] public int Gravity = 1800;
	
	private ulong _prevVertWall = 0;

	private Vector2 ProcessJump(Vector2 velocity)
	{
		velocity.Y = -JumpForce;
		Jumping = true;
		return velocity;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		UpDirection = Vector2.Up;
		MotionMode = MotionModeEnum.Grounded;
		Vector2 velocity = Velocity;
		
		// Handle horizontal movement
		float directionX = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
		velocity.X = directionX * MoveSpeed;

		// Apply gravity only when not on the floor
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}

		// Ensure we correctly detect the floor
		if (IsOnFloor())
		{
			velocity.Y = 0;
			Jumping = false;
			_prevVertWall = 0;
		}

		// Handle jumping logic
		if (Input.IsActionJustPressed("jump"))
		{
			if (IsOnFloor())
			{
				velocity = ProcessJump(velocity);
			}
			else if (IsOnWall() && Jumping)
			{
				// Wall jump logic using GetLastSlideCollision()
				var collision = GetLastSlideCollision();
				if (collision != null && collision.GetCollider() is StaticBody2D collider)
				{
					ulong colliderId = collider.GetInstanceId();
					if (colliderId != _prevVertWall)
					{
						velocity = ProcessJump(velocity);
						_prevVertWall = colliderId;
					}
				}
			}
		}
		
		// Clamp velocity to MAX_SPEED
		velocity.X = Mathf.Clamp(velocity.X, -MaxSpeed, MaxSpeed);
		velocity.Y = Mathf.Clamp(velocity.Y, -MaxSpeed, MaxSpeed);

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
