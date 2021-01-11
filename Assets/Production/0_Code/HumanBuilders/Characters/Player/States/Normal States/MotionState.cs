using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HumanBuilders {

  /// <summary>
  /// Shared behavior for player states that allow the player to move left/right.
  /// </summary>
  public abstract class MotionState : PlayerState {

    #region Fields
    /// <summary>
    /// The player's maximum horizontal movement speed.
    /// </summary>
    protected float maxSpeed;

    /// <summary>
    /// The squared velocity of the player's max speed.
    /// </summary>
    protected float maxSqrVelocity;

    /// <summary>
    /// How quickly the player accelerates to top speed, as a fraction of the player's top speed.
    /// </summary>
    protected float acceleration;

    /// <summary>
    /// The player acceleration in terms of units/sec^2
    /// </summary>
    protected float accelerationFactor;

    /// <summary>
    /// How quickly the player decelerates.
    /// </summary>
    protected float deceleration;

    /// <summary>
    /// The deceleration of the player in terms of a force vector.
    /// </summary>
    protected Vector2 decelerationForce;

    /// <summary>
    /// How quickly the player turns around while running.
    /// </summary>
    protected float agility;

    /// <summary>
    /// How slow the player needs to be moving to switch back to idle state.
    /// </summary>
    protected float idleThreshold;

    /// <summary>
    /// Instantaneous deceleration to facilitate wall jumping.
    /// </summary>
    protected float wallJumpMuting;

    /// <summary>
    /// Whether or not to keep the momentum for a wall jump. After the second
    /// jump in a wall jump, the player has the option to resume tight control
    /// over the character if they move left/right.
    /// </summary>
    protected bool keepWallJumpMomentum;
    #endregion



    #region Player State API
    /// <summary>
    /// First time initialization for the state. A reference to the player and the player's rigidbody will already have been added by this point.
    /// </summary>
    public override void OnStateAdded() {
      
      // Apply various motion settings.
      maxSpeed = settings.MaxSpeed;
      maxSqrVelocity = maxSpeed*maxSpeed;

      acceleration = settings.Acceleration;
      accelerationFactor = maxSpeed*acceleration;

      deceleration = settings.Deceleration;
      decelerationForce = new Vector2(1-deceleration, 1);

      agility = settings.Agility;

      idleThreshold = settings.IdleThreshold;
    }
    #endregion

    #region Public Interface
    /// <summary>
    /// Tries to perform some kind of jump, accounting for any input leniency.
    /// </summary>
    public abstract bool TryBufferedJump();

    /// <summary>
    /// Translate user input into horizontal motion.
    /// </summary>
    /// <returns>Which direction the player should be facing.</returns>
    public Facing MoveHorizontally() {
      float input = player.GetHorizontalInput();
      bool movingEnabled = player.CanMove();

      TryDecelerate(input, player.IsWallJumping(), movingEnabled);

      if (!movingEnabled) {
        return GetFacing();
      } 

      TryUnparentPlayerTransform(player.IsPlatformMomentumEnabled(), input);

      // factor in turn around time.
      float inputDirection = Mathf.Sign(input);
      float motionDirection = Mathf.Sign(physics.Vx);
      float adjustedInput = (inputDirection == motionDirection) ? (input) : (input*agility);

      if (Mathf.Abs(input) == 1 && player.CanInterruptWallJump()) {
        player.StopWallJumpMuting();
      }

      if (player.IsWallJumping()) {
        adjustedInput *= wallJumpMuting;
      }

      float horizSpeed = Mathf.Clamp(physics.Vx + (adjustedInput*accelerationFactor), -maxSpeed, maxSpeed);
      physics.Vx = horizSpeed;
      
      return GetFacing();
    }  


    /// <summary>
    /// Attempt to remove the Player's transform from a platform's list of child
    /// transforms.
    /// </summary>
    /// <param name="platformMomentumEnabled">Whether or not platform momentum
    /// is currently enabled for the player.</param>
    /// <param name="input">The player's horizontal axis input.</param>
    /// <returns>Whether or not the player's transform was freed from its parent
    /// transform. True if yes, false if no.</returns>
    public bool TryUnparentPlayerTransform(bool platformMomentumEnabled, float input) {
      // Prevents the player from being dragged around by a platform they 
      // may have been parented to.
      if (!platformMomentumEnabled && input != 0) {
        transform.SetParent(null);
        return true;
      }

      return false;
    }

    /// <summary>
    /// Attempt to decelerate the player.
    /// </summary>
    /// <param name="input">The player's horizontal axis input.</param>
    /// <param name="wallJumping">Whether or not the player is wall currently in
    /// the air from a wall jump.</param>
    /// <param name="movingEnabled">Whether or not moving is enabled.</param>
    /// <returns>True if the player was decelerated. False otherwise.</returns>
    public bool TryDecelerate(float input, bool wallJumping, bool movingEnabled) {      
      if ((Mathf.Abs(input) != 1 && !wallJumping) || (!movingEnabled && !wallJumping)) {
        physics.Velocity *= decelerationForce;
        return true;
      }

      return false;
    }

    /// <summary>
    /// Get which direction the player is supposed to face based on their
    /// horizontal velocity.
    /// </summary>
    /// <returns>Which direction the player should face.</returns>
    public Facing GetFacing() {
      if (Mathf.Abs(physics.Vx) < idleThreshold) {
        return Facing.None;
      } else {
        return (Facing)Mathf.Sign(physics.Vx);
      }
    }
    #endregion
  }
}