using Storm.Attributes;
using Storm.Characters.Player;
using Storm.Components;
using Storm.LevelMechanics.Platforms;
using Storm.Math;
using UnityEngine;


namespace Storm.Flexible.Interaction {
  /// <summary>
  /// An object that the player can pick up and carry around.
  /// </summary>
  public class Carriable : Interactible {

    #region Fields

    /// <summary>
    /// The name of the indicator that will display over the player's head when
    /// they can interact with this object.
    /// </summary>
    private const string QUESTION_MARK = "QuestionMark";

    /// <summary>
    /// Threshold for whether or not the player is immobile on the ground.
    /// </summary>
    private const float SITTING_THRESHOLD = 0.1f;

    /// <summary>
    /// Whether or not the object is considered organic. This determines whether or not the object can travel through live wires.
    /// </summary>
    [Tooltip("Whether or not the object is considered organic. This determines whether or not the object can travel through live wires.")]
    public bool IsOrganic;

    private bool thrown;

    private bool releasedAction;

    /// <summary>
    /// Physics information (position, velocity) for this object.
    /// </summary>
    public IPhysics Physics;

    /// <summary>
    /// The original scale of the carriable object (so it can be reset after a player state animation gets interrupted).
    /// </summary>
    private Vector3 originalScale;

    private ICollision collisionSensor;


    [SerializeField]
    [ReadOnly]
    private bool freeze;

    [SerializeField]
    [ReadOnly]
    private bool stacked;

    #endregion
      
    #region Unity API
    protected new void Awake() {
      base.Awake();

      BoxCollider2D[] cols = GetComponents<BoxCollider2D>();
      col = cols[0];

      PlayerCharacter player = FindObjectOfType<PlayerCharacter>();

      Physics = gameObject.AddComponent<PhysicsComponent>();
      originalScale = transform.localScale;
    }


    private void Start() {
      collisionSensor = new CollisionComponent(col);

      // Allow carriable items to be thrown up through one-way platforms.
      OneWayPlatform.RegisterCollider(col);
    }

    private void FixedUpdate() {
      if (!freeze && 
          !collisionSensor.IsTouchingLeftWall(col.bounds.center, col.bounds.size) &&
          !collisionSensor.IsTouchingRightWall(col.bounds.center, col.bounds.size)) {
            
        if (Mathf.Abs(Physics.Vx) < 0.01f) {
          FreezePosition();
        }
      } else if (freeze && Mathf.Abs(Physics.Vx) > 0.01f) {
        UnfreezePosition();
      }

      if (!stacked && !interacting) {
        TryStack();
      }
    }


    private void TryStack() {
      RaycastHit2D[] hits = Physics2D.RaycastAll(
        new Vector2(col.bounds.center.x, col.bounds.center.y-col.bounds.extents.y), 
        Vector2.down,
        0.1f
      );

      foreach (RaycastHit2D hit in hits) {
        if (hit.collider == col) continue; // skip your collider.

        if (!hit.collider.isTrigger) {
          Carriable carriable = hit.collider.GetComponent<Carriable>();
          if (carriable != null && carriable.freeze) {
            Physics.Px = carriable.Physics.Px;
            stacked = true;
            Physics.Velocity = Vector2.zero;
          }
        }
      }

    }


    private void FreezePosition() {
      freeze = true;
      Physics.Freeze(true, false, true);
    }


    private void UnfreezePosition() {
      freeze = false;
      Physics.Freeze(false, false, true);
    }
    #endregion

    #region Public Interface
    /// <summary>
    /// Pick up this object.
    /// </summary>
    public void OnPickup() {
      interacting = true;
      thrown = false;
      stacked = false;
      UnfreezePosition();

      if (player == null) {
        player = FindObjectOfType<PlayerCharacter>();
      }

      player.CarriedItem = this;
      player.Physics.AddChild(transform);

      Physics.Disable();
      Physics.SetParent(player.transform.GetChild(0));
      Physics.ResetLocalPosition();
      
      col.enabled = false;
      releasedAction = !player.HoldingAction() || player.ReleasedAction();
    }
    
    /// <summary>
    /// Put down this object.
    /// </summary>
    public void OnPutDown() {
      thrown = false;
      interacting = false;
      stacked = false;
      UnfreezePosition();

      if (player == null) {
        player = FindObjectOfType<PlayerCharacter>();
      }

      player.CarriedItem = null;

      Physics.Enable();
      Physics.ClearParent();
      col.enabled = true;
      transform.localScale = originalScale;
    }

    /// <summary>
    /// Throw this object.
    /// </summary>
    public void OnThrow() {
      thrown = true;
      interacting = false;
      stacked = false;
      UnfreezePosition();

      if (player == null) {
        player = FindObjectOfType<PlayerCharacter>();
      }

      player.CarriedItem = null;

      Physics.Enable();
      Physics.ClearParent();
      col.enabled = true;
      transform.localScale = originalScale;
    }

    #endregion

    #region Interactible API
    
    /// <summary>
    /// What the object should do when interacted with.
    /// </summary>
    public override void OnInteract() {
      OnPickup();
    }
    
    /// <summary>
    /// Whether or not the indicator for this interactible should be shown.
    /// </summary>
    /// <remarks>
    /// This is used when this particular interactive object is the closest to the player. If the indicator can be shown
    /// that usually means it can be interacted with.
    /// </remarks>
    public override bool ShouldShowIndicator() {
      return (!thrown &&
              !player.IsCrawling() && 
              !player.IsDiving() &&
              !player.IsInWallAction() &&
              player.CarriedItem == null);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
      thrown = false;
    }
    #endregion
  }
}
