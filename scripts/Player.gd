extends RigidBody2D

var jumping = false
var stopping_jump = false

var WALK_ACCEL = 800.0
var WALK_DEACCEL = 800.0
var WALK_MAX_VELOCITY = 200.0
var AIR_ACCEL = 200.0
var AIR_DEACCEL = 200.0
var JUMP_VELOCITY = 460
var STOP_JUMP_FORCE = 900.0
var MAX_FLOOR_AIRBORNE_TIME = 0.15

var airborne_time = 1e20
var floor_h_velocity = 0.0
var screen_size  # Size of the game window.
signal hit

# Called when the node enters the scene tree for the first time.
func _ready():
    screen_size = get_viewport_rect().size
    #hide()

func start(pos):
    position = pos
    show()
    $CollisionShape2D.disabled = false

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass

#var thrust = Vector2(0, 250)
#var torque = 20000

func _integrate_forces(state):
    var lv = state.get_linear_velocity()
    var step = state.get_step()

    var move_left = Input.is_action_pressed("ui_left")
    var move_right = Input.is_action_pressed("ui_right")
    var jump = Input.is_action_pressed("ui_up")

    # Deapply prev floor velocity
    lv.x -= floor_h_velocity
    floor_h_velocity = 0.0

    var found_floor = false
    var floor_index = -1

    for x in range(state.get_contact_count()):
        var ci = state.get_contact_local_normal(x)
        if ci.dot(Vector2(0, -1)) > 0.6:
            found_floor = true
            floor_index = x

    if found_floor:
        airborne_time = 0.0
    else:
        airborne_time += step # Time it spent in the air
    
    var on_floor = airborne_time < MAX_FLOOR_AIRBORNE_TIME
    # Process jump
    if jumping:
        if lv.y > 0:
            # Set off the jumping flag if going down
            jumping = false
        elif not jump:
            stopping_jump = true
        
        if stopping_jump:
            lv.y += STOP_JUMP_FORCE * step
    
    if on_floor:
        # Process logic when character is on floor
        if move_left and not move_right:
            if lv.x > -WALK_MAX_VELOCITY:
                lv.x -= WALK_ACCEL * step
        elif move_right and not move_left:
            if lv.x < WALK_MAX_VELOCITY:
                lv.x += WALK_ACCEL * step
        else:
            var xv = abs(lv.x)
            xv -= WALK_DEACCEL * step
            if xv < 0:
                xv = 0
            lv.x = sign(lv.x) * xv
        
        # Check jump
        if not jumping and jump:
            lv.y = -JUMP_VELOCITY
            jumping = true
            stopping_jump = false
    else:
        # Process logic when the character is in the air
        if move_left and not move_right:
            if lv.x > -WALK_MAX_VELOCITY:
                lv.x -= AIR_ACCEL * step
        elif move_right and not move_left:
            if lv.x < WALK_MAX_VELOCITY:
                lv.x += AIR_ACCEL * step
        else:
            var xv = abs(lv.x)
            xv -= AIR_DEACCEL * step
            if xv < 0:
                xv = 0
            lv.x = sign(lv.x) * xv
    # Apply floor velocity
    if found_floor:
        floor_h_velocity = state.get_contact_collider_velocity_at_position(floor_index).x
        lv.x += floor_h_velocity
    
    # Finally, apply gravity and set back the linear velocity
    lv += state.get_total_gravity() * step
    state.set_linear_velocity(lv)

#    if jump:
#        applied_force = thrust.rotated(rotation)
#    else:
#        applied_force = Vector2()
#    var rotation_dir = 0
#    if move_right:
#        rotation_dir += 1
#    if move_left:
#        rotation_dir -= 1
#    applied_torque = rotation_dir * torque


func _on_Player_body_entered(body):
    hide()  # Player disappears after being hit.
    emit_signal("hit")
    $CollisionShape2D.set_deferred("disabled", true)