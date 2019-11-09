extends KinematicBody2D

export var snap := false
export var move_speed := 250
export var jump_force := 500
export var gravity := 900
export var slope_slide_threshold := 50.0

var velocity := Vector2()

func _physics_process(delta):
	var direction_x := Input.get_action_strength("move_right") - Input.get_action_strength("move_left")
	velocity.x = direction_x * move_speed

	if Input.is_action_just_pressed("jump") and snap:
		velocity.y = -jump_force
		snap = false

	velocity.y += gravity * delta

	var snap_vector = Vector2(0, 32) if snap else Vector2()
	velocity = move_and_slide_with_snap(velocity, snap_vector, Vector2.UP, slope_slide_threshold, 4, deg2rad(46))

	if is_on_floor():
		velocity.y = 0

	# Just landed
	if is_on_floor() and not snap:
		snap = true