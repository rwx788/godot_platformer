extends KinematicBody2D

const MAX_SPEED = 600

export var jumping := false
export var move_speed := 250
export var jump_force := 700
export var gravity := 1800

var speed := Vector2()
var velocity := Vector2()
var snap_vector := Vector2(0, 32)
var prev_vert_wall := 0

func process_jump():
	speed.y = -jump_force
	jumping = true	

func _physics_process(delta):
	var direction_x := Input.get_action_strength("move_right") - Input.get_action_strength("move_left")
	
	speed.x = direction_x * move_speed	
	speed.y += gravity * delta
	# Limit maximum speed
	speed.x = clamp(speed.x, -MAX_SPEED, MAX_SPEED)
	speed.y = clamp(speed.y, -MAX_SPEED, MAX_SPEED)

	velocity = move_and_slide_with_snap( speed, speed if jumping else snap_vector, Vector2.UP, 50, 4, deg2rad(46) )
	
	if is_on_floor():
			velocity.y = 0
			jumping = false
			prev_vert_wall = 0	
			
	if Input.is_action_just_pressed("jump"):		
		# Is on floor, allow jumping	
		if is_on_floor():
			process_jump()
		# Allow wall jumps only when in air already
		elif is_on_wall() and jumping:
			# Do not allow wall jumps from the same wall
			for i in get_slide_count():
				var collision = get_slide_collision(i)
				if collision.collider_id != prev_vert_wall and collision.collider.is_class('StaticBody2D'):	
					process_jump()
					prev_vert_wall = collision.collider_id
					break

func collide() -> void:
	pass