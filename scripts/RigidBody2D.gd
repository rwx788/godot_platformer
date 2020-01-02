extends KinematicBody2D

var velocity = Vector2(0,0)
export var gravity := 800

func _physics_process(delta):
	velocity.y += gravity
	var collision = move_and_collide(velocity * delta)
	if collision:
		if collision.collider.has_method("collide"):
			print("aaa")
			velocity.x *= -100
