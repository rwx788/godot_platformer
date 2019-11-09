extends KinematicBody2D

const MAX_SPEED = 600

export var jumping := false
export var move_speed := 250
export var jump_force := 700
export var gravity := 1800
export var slope_slide_threshold := 50.0

var speed := Vector2()
var velocity := Vector2()
var snap_vector := Vector2(0, 32)

func process_jump():
	if Input.is_action_just_pressed("jump"):
		speed.y = -jump_force
		jumping = true	

func _physics_process(delta):
	var direction_x := Input.get_action_strength("move_right") - Input.get_action_strength("move_left")
	
	speed.x = direction_x * move_speed	
	speed.y += gravity * delta
	# Limit maximum speed
	speed.x = clamp(speed.x, -MAX_SPEED, MAX_SPEED)
	speed.y = clamp(speed.y, -MAX_SPEED, MAX_SPEED)

	velocity = move_and_slide_with_snap( speed, speed if jumping else snap_vector, Vector2.UP, slope_slide_threshold )
	
	if is_on_floor():
		# Landed
		if jumping:
			velocity.y = 0
			jumping = false	
		# Is on floor, allow jumping	
		else:
			process_jump()
	# Allow wall jumps
	elif is_on_wall():
		process_jump()
		