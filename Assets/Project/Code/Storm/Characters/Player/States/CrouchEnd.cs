﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Storm.Characters.Player {

  /// <summary>
  /// When the player stands up from crouch.
  /// </summary>
  public class CrouchEnd : PlayerState {

    #region Unity API
    private void Awake() {
      AnimParam = "crouch_end";
    }

    #endregion

    #region Player State API

    /// <summary>
    /// Animation event hook.
    /// </summary>
    public void OnCrouchEndFinished() {
      ChangeToState<Idle>();
    }

    /// <summary>
    /// Fires once per frame. Use this instead of Unity's built in Update() function.
    /// </summary>
    public override void OnUpdate() {
      if (player.TryingToMove()) {
        ChangeToState<Running>();
      } else if (player.HoldingDown()) {
        ChangeToState<CrouchStart>();
      } else if (player.HoldingJump()) {
        ChangeToState<SingleJumpStart>();
      }
    }


    public override void OnStateEnter() {
      physics.Velocity = Vector2.zero;
    }
    #endregion

  }
}