﻿using System.Collections;
using System.Collections.Generic;
using Storm.Flexible;
using UnityEngine;


namespace Storm.Characters.Player {

  /// <summary>
  /// When the player is standing still.
  /// </summary>
  public class Idle : PlayerState {

    #region Unity API
    private void Awake() {
      AnimParam = "idle";
    }
    #endregion

    #region Player State API

    /// <summary>
    /// Fires once per frame. Use this instead of Unity's built in Update() function.
    /// </summary>
    public override void OnUpdate() {
      if (player.PressedJump()) {
        if (player.IsTouchingRightWall() || player.IsTouchingLeftWall()) {
          ChangeToState<WallRun>();
        } else {
          ChangeToState<SingleJumpStart>();
        }
      } else if (player.HoldingDown()) {
        ChangeToState<CrouchStart>();
      } else if (player.TryingToMove()) {
        ChangeToState<Running>();
      }
    }

    /// <summary>
    /// Fires with every physics tick. Use this instead of Unity's built in FixedUpdate() function.
    /// </summary>
    public override void OnFixedUpdate() {
      if (!player.IsTouchingGround()) {
        ChangeToState<SingleJumpFall>();
      }
    }
    
    /// <summary>
    ///  Fires whenever the state is entered into, after the previous state exits.
    /// </summary>
    public override void OnStateEnter() {
      physics.Velocity = Vector2.zero;
    }

    public override void OnSignal(GameObject obj) {
      Carriable carriable = obj.GetComponent<Carriable>();
      if (carriable != null) {
        if (carriable.Physics.Velocity != Vector2.zero) {
          carriable.OnPickup();
          ChangeToState<CarryIdle>();
        } else {
          ChangeToState<PickUpItem>();
        }
      }
    }
    #endregion
  }

}