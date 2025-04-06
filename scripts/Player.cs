using Godot;
using System;

public partial class Player : CharacterBody2D
{
    private const int MaxSpeed = 600;
    private const int MaxJumps = 2;  // Max number of jumps allowed (1 for regular jump, 1 for double jump)
    
    [Export] public int MoveSpeed = 250;
    [Export] public int JumpForce = 700;
    [Export] public int Gravity = 1800;

    private ulong _prevVertWall = 0;
    private int _jumpCount = 0;  // Tracks the number of jumps

    private bool IsJumping => (_jumpCount != 0);

    private Vector2 ProcessJump(Vector2 velocity, bool wallJump = false)
    {
        velocity.Y = -JumpForce;
        if (!wallJump)
        {
            _jumpCount++;
        }
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
            if (IsOnWall() && IsJumping)
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

        // Clamp velocity to MAX_SPEED
        velocity.X = Mathf.Clamp(velocity.X, -MaxSpeed, MaxSpeed);
        velocity.Y = Mathf.Clamp(velocity.Y, -MaxSpeed, MaxSpeed);

        // Set the velocity and handle movement
        Velocity = velocity;

        // Use MoveAndSlide for collision detection and floor check
        MoveAndSlide();
    }

    public void Collide()
    {
        // Placeholder for additional collision logic
    }
}
