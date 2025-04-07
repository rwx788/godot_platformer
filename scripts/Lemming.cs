using System.Linq;
using Godot;

namespace Layka.scripts;

public partial class Lemming : CharacterBody2D
{
	[Export] public float WalkSpeed = 250f;
	[Export] public int Gravity = 1200;

	private Vector2 _velocity = Vector2.Zero;
	private int _direction = 1; // 1 = right, -1 = left

	private RayCast2D _wallRayCast1;
	private RayCast2D _wallRayCast2;
	private RayCast2D _floorRayCast1;
	private RayCast2D _floorRayCast2;


	public override void _Ready()
	{
		_wallRayCast1 = GetNode<RayCast2D>("WallRayCast_1");
		_wallRayCast2 = GetNode<RayCast2D>("WallRayCast_2");
		_floorRayCast1 = GetNode<RayCast2D>("FloorRayCast_1");
		_floorRayCast2 = GetNode<RayCast2D>("FloorRayCast_2");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Apply gravity if not on the floor.
		if (!IsOnFloor())
			_velocity.Y += Gravity * (float)delta;
		else
			_velocity.Y = 0;



		RayCast2D[] wallRays = [_wallRayCast1, _wallRayCast2];
		RayCast2D[] floorRays = [_floorRayCast1, _floorRayCast2];

		// Use LINQ's Any() method to detect collisions.
		bool wallDetected = wallRays.Any(ray => ray != null && ray.IsColliding());
		bool floorDetected = floorRays.Any(ray => ray != null && !ray.IsColliding());

		// If a wall is detected or no floor is detected ahead, reverse direction.
		if (wallDetected || floorDetected)
		{
			_direction *= -1;
		}
		// Move horizontally at a constant speed.
		_velocity.X = WalkSpeed * _direction;

		// Apply the calculated velocity and move the character.
		Velocity = _velocity;
		MoveAndSlide();
	}
}
