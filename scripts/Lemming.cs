using System.Linq;
using System.Numerics;
using Godot;
using Vector2 = Godot.Vector2;

namespace Layka.scripts;

public partial class Lemming : CharacterBody2D
{
	[Export] public float WalkSpeed = 250f;
	[Export] public int Gravity = 1200;

	private Vector2 _velocity = Vector2.Zero;
	private float _direction = Vector2.Right.X; // 1 = right, -1 = left

	private RayCast2D _wallRayCastR;
	private RayCast2D _wallRayCastL;
	private RayCast2D _floorRayCastL;
	private RayCast2D _floorRayCastR;

	// Used to track the previous wall not to change direction multiple times
	private ulong _prevWallId = 0;

	public override void _Ready()
	{
		_wallRayCastR = GetNode<RayCast2D>("WallRayCast_R");
		_wallRayCastL = GetNode<RayCast2D>("WallRayCast_L");
		_floorRayCastR = GetNode<RayCast2D>("FloorRayCast_R");
		_floorRayCastL = GetNode<RayCast2D>("FloorRayCast_L");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Apply gravity if not on the floor.
		if (!IsOnFloor())
			_velocity.Y += Gravity * (float)delta;
		else
			_velocity.Y = 0;

		// Create arrays for wall and floor RayCast2D nodes.
		RayCast2D[] wallRays = [_wallRayCastL, _wallRayCastR];
		RayCast2D[] floorRays = [_floorRayCastL, _floorRayCastR];
		
		if (floorRays.Any(ray => ray != null && !ray.IsColliding()))
		{
			_direction *= -1;
			// Reset previous wall ID when turning due to lack of floor.
			_prevWallId = 0;
		}
		else
		{
			var collidedWallIds = wallRays
				.Where(ray => ray != null && ray.IsColliding())
				.Select(ray => ray.GetCollider().GetInstanceId());

			var newWall = collidedWallIds.ToList().FirstOrDefault(collidedWallId => _prevWallId != collidedWallId);
			if(newWall != 0)
			{
				_direction *= -1;
				_prevWallId = newWall;
			}
		}

		// Set horizontal velocity.
		_velocity.X = WalkSpeed * _direction;

		// Apply the calculated velocity and move the character.
		Velocity = _velocity;
		MoveAndSlide();
	}

}
