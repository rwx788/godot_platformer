using System.Linq;
using Godot;

namespace Layka.scripts;

public partial class Player : CharacterBody2D
{
	private const int MaxSpeed = 600;
	private const int MaxJumps = 2;
	
	[Export] public int MoveSpeed = 250;
	[Export] public int JumpForce = 700;
	[Export] public int Gravity = 1800;
	[Export] public float PickupRange = 50f;

	private RayCast2D _wallRayCastR;
	private RayCast2D _wallRayCastL;
	private RayCast2D _floorRayCastL;
	private RayCast2D _floorRayCastR;
	
	private Vector2 _carryOffset = new Vector2(45, 0);

	private ulong _prevVertWall = 0;
	private int _jumpCount = 0;  // Tracks the number of jumps

	
	// Holds the current object being carried (if any)
	private Lemming _carried = null;
	private bool IsCarrying => _carried != null;

	private bool IsJumping => (_jumpCount != 0);

	private Vector2 ProcessJump(Vector2 velocity, bool wallJump = false)
	{
		velocity.Y = -JumpForce;
		if (!wallJump || IsCarrying)
		{
			_jumpCount++;
		}
		return velocity;
	}

	public override void _Ready()
	{
		_wallRayCastR = GetNode<RayCast2D>("WallRayCast_R");
		_wallRayCastL = GetNode<RayCast2D>("WallRayCast_L");
		_floorRayCastR = GetNode<RayCast2D>("FloorRayCast_R");
		_floorRayCastL = GetNode<RayCast2D>("FloorRayCast_L");
	}

	public override void _PhysicsProcess(double delta)
	{
		UpDirection = Vector2.Up;
		MotionMode = MotionModeEnum.Grounded;
		Vector2 velocity = Velocity;

		// Handle horizontal movement
		float directionX = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
		velocity.X = directionX * MoveSpeed;

		// Apply gravity when not on the floor
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}

		// Reset jump count when on the floor
		if (IsOnFloor())
		{
			velocity.Y = 0;
			_jumpCount = 0;  // Reset jump count when on the floor
			_prevVertWall = 0;
		}

		// Handle jumping logic
		if (Input.IsActionJustPressed("jump"))
		{
			if (IsOnWall() && !IsCarrying && IsJumping)
			{
				// Allow only one wall jump from the same wall
				var collision = GetLastSlideCollision();
				if (collision != null && collision.GetCollider() is StaticBody2D collider)
				{
					ulong colliderId = collider.GetInstanceId();
					if (colliderId != _prevVertWall)
					{
						velocity = ProcessJump(velocity, true);
						_prevVertWall = colliderId;
					}
				}
			}
			else if (_jumpCount < MaxJumps)
			{
				velocity = ProcessJump(velocity);
			}
		}
		else if (Input.IsActionJustPressed("pick_up"))
		{
			// If already carrying an object, drop it.
			if (_carried != null)
			{
				DropCarried();
			}
			else
			{
				// Otherwise, try to pick up a nearby object.
				Lemming candidate = FindNearbyPickup();
				if (candidate != null)
				{
					Pickup(candidate);
				}
			}
		}

		// If carrying an object, update its position relative to the player.
		if (_carried != null)
		{
			_carried.GlobalPosition = GlobalPosition + _carryOffset;
		}

		// Clamp velocity to MAX_SPEED
		velocity.X = Mathf.Clamp(velocity.X, -MaxSpeed, MaxSpeed);
		velocity.Y = Mathf.Clamp(velocity.Y, -MaxSpeed, MaxSpeed);

		// Set the velocity and handle movement
		Velocity = velocity;

		// Use MoveAndSlide for collision detection and floor check
		MoveAndSlide();
	}

	private Lemming FindNearbyPickup()
	{
		if (_wallRayCastL.IsColliding() && _wallRayCastL.GetCollider() is Lemming)
		{
			_carryOffset *= Vector2.Left;
			return _wallRayCastL.GetCollider() as Lemming;
		}
		if (_wallRayCastR.IsColliding() && _wallRayCastR.GetCollider() is Lemming)
		{
			_carryOffset *= Vector2.Right;
			return _wallRayCastR.GetCollider() as Lemming;
		}
		return null;
	}

	/// <summary>
	/// Picks up the specified target.
	/// </summary>
	private void Pickup(Lemming target)
	{
		_carried = target;
		_carried.SetPhysicsProcess(false);

	}

	private void DropCarried()
	{
		if (_carried == null) return;
		_carried.SetPhysicsProcess(true);
		_carried = null;
	}
}
