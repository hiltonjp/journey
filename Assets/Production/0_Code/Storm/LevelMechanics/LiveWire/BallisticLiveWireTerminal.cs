using Storm.Characters.PlayerOld;
using UnityEngine;

namespace Storm.LevelMechanics.Livewire {

  /// <summary>
  /// A Level Object that transforms Jerrod into a spark of energy
  /// and allows the player to fling him in any direction in a ballistic arc.
  /// </summary>
  /// <seealso cref="LivewireNode" />
  public class BallisticLivewireTerminal : LivewireNode {

    #region Unity API
    //-------------------------------------------------------------------------
    // Unity API
    //-------------------------------------------------------------------------

    /// <summary>
    /// Turns Jerrod into a spark of energy.
    ///
    /// This method fires when:
    /// 1. Another game object's collider component intersects with this game object's collider
    /// 2. This game object's collider is marked as "IsTrigger" in the inspector
    /// </summary>
    protected override void OnTriggerEnter2D(Collider2D collider) {
      if (collider.CompareTag("Player")) {
        PlayerCharacterOld player = collider.gameObject.GetComponent<PlayerCharacterOld>();

        player.SwitchBehavior(PlayerBehaviorEnum.AimLiveWire);
        player.AimLiveWireMovement.SetLaunchPosition(transform.position);
      }
    }
    #endregion
  }

}