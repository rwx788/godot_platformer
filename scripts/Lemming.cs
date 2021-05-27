
using System;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;


public class Lemming : KinematicBody2D
{
     
    public Vector2 velocity = new Vector2(0,0);
    [Export] public int gravity  = 800;
    
    public override void _PhysicsProcess(float delta)
    {  
        velocity.y += gravity;
        var collision = MoveAndCollide(velocity * delta);
        if(collision != null)
        {
            if(collision.Collider.HasMethod("collide"))
            {
                velocity.x *= -100;    
            }
        }
    }  
    
    
}