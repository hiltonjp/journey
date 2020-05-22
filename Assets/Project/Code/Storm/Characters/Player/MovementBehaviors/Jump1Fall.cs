﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Storm.Characters.Player {

  /// <summary>
  /// When the player is falling after their first jump.
  /// </summary>
  public class Jump1Fall : HorizontalMotion {

    #region Fields
    /// <summary>
    /// The amount of time the player needs to be falling to turn the landing into a roll.
    /// </summary>
    private float rollOnLand;

    /// <summary>
    /// Times how long the player has been falling in this state.
    /// </summary>
    private float fallTimer;

    #endregion

    #region Unity API
    private void Awake() {
      AnimParam = "jump_1_fall";
    }
    #endregion


    #region Player State API

    /// <summary>
    /// Fires once per frame. Use this instead of Unity's built in Update() function.
    /// </summary>
    public override void OnUpdate() {
      if (Input.GetButtonDown("Jump")) {
        ChangeToState<Jump2Start>();
      }
    }

    /// <summary>
    /// Fires with every physics tick. Use this instead of Unity's built in FixedUpdate() function.
    /// </summary>
    public override void OnFixedUpdate() {
      fallTimer += Time.deltaTime;

      Facing facing = MoveHorizontally();
      player.SetFacing(facing);

      if (player.IsTouchingLeftWall() || player.IsTouchingRightWall()) {
        ChangeToState<WallSlide>();
      } else if (player.IsTouchingGround()) {
        float xVel = rigidbody.velocity.x;

        if (Mathf.Abs(xVel) > idleThreshold) {
          PickLanding<RollStart, CrouchStart, Land>();
        } else {
          PickLanding<CrouchEnd, CrouchStart, Land>();
        }
      }
    }

    /// <summary>
    /// Pick between one of three landing types.
    /// </summary>
    /// <typeparam name="RollTimeState">The landing to use when the player has been falling long enough to roll. </typeparam>
    /// <typeparam name="CrouchState">The landing to use if the player is trying to crouch.</typeparam>
    /// <typeparam name="LandState">The default landing state.</typeparam>
    private void PickLanding<RollTimeState,CrouchState,LandState>() 
      where RollTimeState : PlayerState 
      where CrouchState : PlayerState 
      where LandState : PlayerState {

      if (fallTimer > rollOnLand) {
        ChangeToState<RollTimeState>();
      } else  {
        if (Input.GetButton("Down")) {
          ChangeToState<CrouchState>();
        } else {
          ChangeToState<LandState>();
        }
      }
    }

    /// <summary>
    /// First time initialization for the state. A reference to the player and the player's rigidbody will already have been added by this point.
    /// </summary>
    public override void OnStateAdded() {
      base.OnStateAdded();

      MovementSettings settings = GetComponent<MovementSettings>();
      rollOnLand = settings.RollOnLand;
    }

    /// <summary>
    ///  Fires whenever the state is entered into, after the previous state exits.
    /// </summary>
    public override void OnStateEnter() {
      fallTimer = 0;
    }

    #endregion
  }
}