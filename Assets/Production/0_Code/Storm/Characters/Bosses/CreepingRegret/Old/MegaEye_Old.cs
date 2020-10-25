using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Storm.Flexible;
using Storm.Flexible.Interaction;
using Storm.Subsystems.Reset;
using Storm.Subsystems.Transitions;
using UnityEngine;

namespace Storm.Characters.Bosses {
  /// <summary>
  /// The largest eye for the boss "creeping regret."
  /// </summary>
  [RequireComponent(typeof(Shaking))]
  public class MegaEye_Old : PropEye, ITriggerableParent, IResettable {
    #region Fields
    //-------------------------------------------------------------------------
    // Fields
    //-------------------------------------------------------------------------
    /// <summary>
    /// The number of times the main eye needs to be hit.
    /// </summary>
    [Tooltip("The number of times the main eye needs to be hit.")]
    public int TotalHealth;

    /// <summary>
    /// The number of mini eyes to open at the beginning of the battle.
    /// </summary>
    [Tooltip("The number of mini eyes to open at the beginning of the battle.")]
    public int NumEyesStart;

    /// <summary>
    /// The number of eyes that get added onto the open mini eyes after each
    /// attack on the main eye.
    /// </summary>
    [Tooltip("The number of eyes that get added onto the open mini eyes after each attack on the main eye.")]
    public int NumEyesAdded;

    /// <summary>
    /// The number of eyes that will be opened this round.
    /// </summary>
    [Tooltip("The number of eyes that will be opened this round.")]
    [SerializeField]
    [ReadOnly]
    private int numEyes;

    /// <summary>
    /// The remaining number of times the main eye needs to be hit.
    /// </summary>
    [Tooltip("The remaining number of times the main eye needs to be hit.")]
    [SerializeField]
    [ReadOnly]
    private int remainingHealth;
    
    /// <summary>
    /// The list of miniature eyes connected to the main eye.
    /// </summary>
    [SerializeField]
    [ReadOnly]
    private MiniEye_Old[] miniEyes;


    /// <summary>
    /// The dangerous walls keeping the player in center stage.
    /// </summary>
    public GameObject DangerousWalls;


    /// <summary>
    /// The boss's list of attacks
    /// </summary>
    private CreepingRegretAttacks attackEngine;

    /// <summary>
    /// The speech bubbles this attack will use during the fight.
    /// </summary>
    public List<Animator> SpeechBubbles;

    /// <summary>
    /// The speech bubble that's next up to display.
    /// </summary>
    private int currentSpeechBubble;
    #endregion

    #region Unity API
    //-------------------------------------------------------------------------
    // Unity API
    //-------------------------------------------------------------------------

    protected new void Awake() {
      base.Awake();
      miniEyes = FindObjectsOfType<MiniEye_Old>();
      remainingHealth = TotalHealth;
      numEyes = NumEyesStart;
      attackEngine = transform.root.GetComponentInChildren<CreepingRegretAttacks>();
      currentSpeechBubble = 0;
    }

    private void Start() {
      foreach (MiniEye_Old eye in miniEyes) {
        eye.OnEyeClose += TryOpen;
      }
      
      Reset();
    }
    #endregion

    #region Triggerable Parent API
    /// <summary>
    /// If the main eye is open & is hit with a carriable object, the boss takes
    /// damage
    /// </summary>
    /// <param name="col">The collider that hit the boss's main eye.</param>
    public void PullTriggerEnter2D(Collider2D col) {
      Carriable carriable = col.transform.root.GetComponent<Carriable>();
      if (carriable != null && col == carriable.Collider && open) {
        TakeDamage();
        numEyes += NumEyesAdded;
        OpenRandomEyes();
        carriable.Physics.Velocity = Vector2.zero;
      }
    }

    public void PullTriggerStay2D(Collider2D col) {}

    public void PullTriggerExit2D(Collider2D col) {}
    #endregion

    #region Public Interface
    //-------------------------------------------------------------------------
    // Public Interface
    //-------------------------------------------------------------------------

    /// <summary>
    /// Close all mini eyes in the scene.
    /// </summary>
    public void CloseMiniEyes() {
      foreach (MiniEye_Old eye in miniEyes) {
        eye.Close();
      }
    }

    /// <summary>
    /// Reset the boss.
    /// </summary>
    public void Reset() {
      if (animator == null) {
        animator = GetComponent<Animator>();
      }

      foreach (Transform child in DangerousWalls.transform) {
        child.gameObject.SetActive(false);
      }
      DangerousWalls.SetActive(false);

      remainingHealth = TotalHealth;
      numEyes = NumEyesStart;

      Close();
      CloseMiniEyes();
      OpenRandomEyes();

      if (attackEngine != null) {
        attackEngine.StopAttacks();
        attackEngine.ResetPhase();
      }

      currentSpeechBubble = 0;
    }
    #endregion


    #region Helper Methods
    //-------------------------------------------------------------------------
    // Helper Functions
    //-------------------------------------------------------------------------

    /// <summary>
    /// Try to open the main eye. If there are any small eyes still open, this
    /// does nothing.
    /// </summary>
    private void TryOpen() {
      if (AnyEyesOpen()) {
        return;
      }

      Open();
    }

    /// <summary>
    /// Check to see if any small eyes are open.
    /// </summary>
    /// <returns>True if at least one small eye is open. False otherwise.</returns>
    private bool AnyEyesOpen() {
      foreach (MiniEye_Old eye in miniEyes) {
        if (eye.IsOpen) {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Check to see if all small eyes are open.
    /// </summary>
    /// <returns>True if all small eyes are open. False otherwise.</returns>
    private bool AllEyesOpen()  {
      foreach (MiniEye_Old eye in miniEyes) {
        if (!eye.IsOpen) {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Make the boss take damage. If the boss hits 0 health, the boss will be destroyed.
    /// </summary>
    private void TakeDamage() {
      remainingHealth--;
      if (remainingHealth == 3) {
        attackEngine.StartAttacks();
      }

      if (remainingHealth == 2) {
        DangerousWalls.SetActive(true);
        foreach (Transform child in DangerousWalls.transform) {
          child.gameObject.SetActive(true);
        }
      }

      if (remainingHealth == 1) {
        attackEngine.NextPhase();
      }

      if (remainingHealth == 0) {
        StartCoroutine(OpenPropEyes());
        enabled = false;
      }

      animator.SetTrigger("damage");
      shaking.Shake();
      Close();
      UseSpeechBubble(null);
    }

    /// <summary>
    /// Open a random set of small eyes. If the boss is at 1 health, opens all
    /// small eyes.
    /// </summary>
    private void OpenRandomEyes() {      
      if (numEyes > miniEyes.Length || AllEyesOpen()) {
        return;
      } else if (remainingHealth == 1) {
        foreach (MiniEye_Old eye in miniEyes) {
          eye.Open();
        }
        return;
      }

      for (int i = 0; i < numEyes; i++) {
        bool opened = false;
        
        // Open a random eye. If the selected eye is already open,
        // choose a different one.
        while (!opened) {
          int index = Random.Range(0, miniEyes.Length);
          if (!miniEyes[index].IsOpen) {
            miniEyes[index].Open();
            opened = true; 
          }
        }

      }
    }

    private IEnumerator OpenPropEyes() {

      attackEngine.StopAttacks();
      GameManager.Player.DisableCrouch();
      GameManager.Player.DisableJump();
      GameManager.Player.DisableMove();

      yield return new WaitForSeconds(3f);

      
      Open();

      foreach (MiniEye_Old eye in miniEyes) {
        eye.Open();
      }

      yield return new WaitForSeconds(1f);
      

      List<PropEye> closedEyes = new List<PropEye>(transform.root.GetComponentsInChildren<PropEye>(true));

      for (int size = closedEyes.Count; size > 0; size--) {
        int index = Random.Range(0, size);
        
        PropEye eye = closedEyes[index];
        closedEyes.Remove(eye);
        eye.Open();

        yield return new WaitForSeconds(0.02f);

      }

      yield return new WaitForSeconds(2f);

      TransitionManager.MakeTransition("main_menu");
      
      GameManager.Player.EnableCrouch();
      GameManager.Player.EnableJump();
      GameManager.Player.EnableMove();
    }

    /// <summary>
    /// 
    /// </summary>
    public void UseSpeechBubble(MiniEye_Old eye) {
      Animator bubble = SpeechBubbles[currentSpeechBubble];
      currentSpeechBubble++;

      Vector3 previous = bubble.gameObject.transform.position;

      if (eye != null) {
        bubble.transform.position = eye.transform.position;
      } else {
        bubble.transform.position = transform.position;
      }
      
      StartCoroutine(UseSpeechBubble(bubble, previous, 1.5f));
    }


    private IEnumerator UseSpeechBubble(Animator bubble, Vector3 position, float delay) {
      bubble.SetTrigger("open");

      yield return new WaitForSeconds(delay);

      bubble.SetTrigger("close");

      yield return new WaitForSeconds(delay);

      bubble.transform.position = position;
    }
    
    #endregion
  }
}

