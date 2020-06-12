﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace Storm.Characters.Player {
  /// <summary>
  /// When the player prepares to do a single jump.
  /// </summary>
  public class SingleJumpStart : HorizontalMotion {

    #region Unity API
    private void Awake() {
      AnimParam = "jump_1_start";
    } 
    #endregion


    #region Player State API
    /// <summary>
    /// Fires once per frame. Use this instead of Unity's built in Update() function.
    /// </summary>
    public override void OnUpdate() {
      if (player.PressedJump()) {
        ChangeToState<DoubleJumpStart>();
      }
    }

    /// <summary>
    /// Fires with every physics tick. Use this instead of Unity's built in FixedUpdate() function.
    /// </summary>
    public override void OnFixedUpdate() {
      Facing facing = MoveHorizontally();
      player.SetFacing(facing);

    }

    /// <summary>
    ///  Fires whenever the state is entered into, after the previous state exits.
    /// </summary>
    public override void OnStateEnter() {
      player.DisablePlatformMomentum();
    }

    /// <summary>
    /// Fires when the state exits, before the next state is entered into.
    /// </summary>
    public override void OnStateExit() {
      MovementSettings settings = GetComponent<MovementSettings>();
      physics.Vy = settings.SingleJumpForce;
    }

    /// <summary>
    /// Animation event hook.
    /// </summary>
    public void OnSingleJumpFinished() {
      ChangeToState<SingleJumpRise>();
    }
    #endregion
  }

}