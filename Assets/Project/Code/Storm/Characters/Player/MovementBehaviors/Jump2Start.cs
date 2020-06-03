﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Storm.Characters.Player {

  /// <summary>
  /// When the player prepares to do a double jump.
  /// </summary>
  public class Jump2Start : HorizontalMotion {

    private void Awake() {
      AnimParam = "jump_2_start";
    }


    public override void OnUpdate() {
      if (player.PressedJump()) {
        if (player.InCoyoteTime()) {
          player.UseCoyoteTime();
          ChangeToState<Jump1Start>();
        } else {
          base.TryBufferedJump();
        }
      }
    }

    public override void OnFixedUpdate() {
      Facing facing = MoveHorizontally();
      if (player.IsTouchingGround()) {
        Debug.Log("Touching Ground");

        if (Mathf.Abs(physics.Vx) > idleThreshold) {
          ChangeToState<RollStart>();
        } else {
          ChangeToState<Land>();
        }
      } else if (player.IsTouchingLeftWall() || player.IsTouchingRightWall()) {
        if (player.IsRising()) {
          ChangeToState<WallRun>();
        } else  {
          ChangeToState<WallSlide>();
        }
      }
    }

    public void OnDoubleJumpFinished() {
      bool touchingWall = player.IsTouchingLeftWall() || player.IsTouchingRightWall();
      if (player.IsRising()) {
        ChangeToState<Jump2Rise>();
      } else {  
        ChangeToState<Jump2Fall>();
      }
    }

    public override void OnStateEnter() {
      MovementSettings settings = GetComponent<MovementSettings>();
      physics.Vy = settings.DoubleJumpForce;
      Debug.Log("DOUBLE JUMP: "+ physics.Velocity);

      player.DisablePlatformMomentum();
    }

  }

}