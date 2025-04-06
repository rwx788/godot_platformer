
using System;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;


public partial class Lemming : CharacterBody2D
{
	 
	public Vector2 velocity = new Vector2(0,0);
	[Export] public int gravity  = 800;
	
	public override void _PhysicsProcess(double delta)
	{  
		velocity.Y += gravity;
		var collision = MoveAndCollide(velocity * (float) delta);
		if(collision != null)
		{
			if(collision.GetCollider().HasMethod("collide"))
			{
				velocity.X *= -100;    
			}
		}
	}  
	
	
}
