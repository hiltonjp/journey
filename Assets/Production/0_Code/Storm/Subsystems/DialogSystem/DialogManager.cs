﻿using System.Collections;
using System.Collections.Generic;
using Storm.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


using Storm.Extensions;
using Storm.Characters.Player;
using Storm.Subsystems.Transitions;

using XNode;
using UnityEngine.SceneManagement;
using Storm.Flexible;
using Storm.Subsystems.Graph;

namespace Storm.Subsystems.Dialog {

  /// <summary>
  /// A manager for conversations with NPCs and the like. This is essentially a Decorator/Wrapper around the
  /// Graph traversal engine.
  /// </summary>
  /// <remarks>
  /// Decorator/Wrapper Pattern: 
  /// https://www.tutorialspoint.com/design_pattern/decorator_pattern.htm
  /// https://refactoring.guru/design-patterns/decorator
  /// </remarks>
  /// <seealso cref="AutoNode" />
  /// <seealso cref="AutoGraphAsset" />
  /// <seealso cref="GraphEngine" />
  public class DialogManager : Singleton<DialogManager> {

    #region Properties
    //-------------------------------------------------------------------------
    // Properties
    //-------------------------------------------------------------------------
    /// <summary>
    /// The Dialog Manager's graph engine. This is responsible for traversing
    //the graph.
    /// </summary>
    public static GraphEngine GraphEngine { get { return Instance.graphEngine; } }

    #endregion

    #region Fields
    //---------------------------------------------------
    // Fields
    //---------------------------------------------------
    /// <summary>
    /// A reference to the player character.
    /// </summary>
    private IPlayer player;

    /// <summary>
    /// A map of Dialog Boxes that can be opened/closed, by name.
    /// </summary>
    private Dictionary<string, IDialogBox> dialogBoxes;

    /// <summary>
    /// The dialog box that's currently open.
    /// </summary>
    private IDialogBox openDialogBox;

    [Space(10, order=0)]

    /// <summary>
    /// The dialog box that will be used by default for any
    /// </summary>
    [Tooltip("The dialog box that will be opened by default at the start of every conversation and inspection.")]
    public IDialogBox DefaultDialogBox;

    /// <summary>
    /// The graph traversal engine. The dialog manager delegates off running a branching
    /// conversation off to this.
    /// </summary>
    private GraphEngine graphEngine;
    
    /// <summary>
    /// Whether or not the text is still being written to the screen.
    /// </summary>
    [Tooltip("Whether or not the text is still being written to the screen.")]
    [ReadOnly]
    public bool StillWriting;
    #endregion

    #region Unity API
    //---------------------------------------------------------------------
    // Unity API
    //---------------------------------------------------------------------
      
    protected void Start() {
      graphEngine = gameObject.AddComponent<GraphEngine>();

      player = FindObjectOfType<PlayerCharacter>();
      SceneManager.sceneLoaded += OnNewScene;

      IDialogBox[] boxes = GetComponentsInChildren<DialogBox>();
      if (DefaultDialogBox == null && boxes.Length == 1) {
        DefaultDialogBox = boxes[0];
      }

      dialogBoxes = new Dictionary<string, IDialogBox>();
      foreach (IDialogBox box in boxes) {
        if (!dialogBoxes.ContainsKey(box.name)) {
          dialogBoxes.Add(box.name, box);
        } else {
          Debug.LogWarning("A Dialog Box named \"" + box.name + "\" has already been added to the DialogManager");
        }
      }
    }

    /// <summary>
    /// Static delegate for MonoBehaviour's StartCoroutine() method. :)
    /// </summary>
    /// <param name="routine">The subroutine</param>
    public static void StartThread(IEnumerator routine) => Instance.StartCoroutine(routine);
    #endregion

    #region Dependency Injection
    //---------------------------------------------------------------------
    // Dependency Injection
    //---------------------------------------------------------------------
      
    /// <summary>
    /// Unit test dependency injection point for the graphing engine.
    /// </summary>
    /// <param name="graphEngine">The graphing engine.</param>
    public static void Inject(GraphEngine graphEngine) {
      Instance.graphEngine = graphEngine;
    }

    /// <summary>
    /// Dependency injection point for a reference to the player.
    /// </summary>
    /// <param name="player">A reference to the player.</param>
    public static void Inject(IPlayer player) {
      Instance.player = player;
    }

    /// <summary>
    /// Dependency injection point for a Dialog graph.
    /// </summary>
    /// <param name="dialog">The dialog to inject</param>
    public static void Inject(IAutoGraph dialog) {
      Instance.graphEngine.Inject(dialog);
    }

    /// <summary>
    /// Dependency injection point for a dialog node.
    /// </summary>
    /// <param name="node">The node to inject.</param>
    public static void Inject(IAutoNode node) {
      Instance.graphEngine.Inject(node);
    }


    public static void Inject(IDialogBox dialogBox, bool open) {
      Instance.DefaultDialogBox = dialogBox;
      if (open) {
        Instance.openDialogBox = dialogBox;
        Instance.openDialogBox.Open();
      }
    }
    #endregion
     

    #region Top-Level Interface
    //---------------------------------------------------------------------
    // Top Level Interaction
    //---------------------------------------------------------------------
      
    /// <summary>
    /// Begins a new dialog with the player.
    /// </summary>
    public static void StartDialog(IAutoGraph graph) => Instance.StartDialog_Inner(graph);
    private void StartDialog_Inner(IAutoGraph graph) {
      if (graph == null) {
        throw new UnityException("No dialog has been set!");
      }
      
      if (player == null) {
        player = GameManager.Player;
      }

      player.DisableJump();
      player.DisableMove();
      player.DisableCrouch();

      openDialogBox = DefaultDialogBox;
      openDialogBox.Open();

      graphEngine.StartGraph(graph);
    }

    /// <summary>
    /// Continues the dialog.
    /// </summary>
    public static void ContinueDialog() => Instance.graphEngine.Continue();

    
    /// <summary>
    /// End the current dialog.
    /// </summary>
    public static void EndDialog() => Instance.EndDialog_Inner();
    private void EndDialog_Inner() {
      if (player == null) {
        player = GameManager.Player;
      }
      
      player.EnableJump();
      player.EnableCrouch();
      player.EnableMove();

      if (openDialogBox != null) {
        openDialogBox.Close();
        openDialogBox = null;
      }

      if (player.CurrentInteractible != null) {
        player.CurrentInteractible.EndInteraction();
      }

      graphEngine.EndGraph();
    }
    #endregion

    #region Dialog UI Manipulation
    //---------------------------------------------------------------------
    // Dialog UI Manipulation
    //---------------------------------------------------------------------
      
    /// <summary>
    /// Type out a sentence.
    /// </summary>
    /// <param name="sentence">The sentence to type.</param>
    /// <param name="speaker">The speaker saying it, if any.</param>
    public static void Type(string sentence, string speaker = "") => Instance.Type_Inner(sentence, speaker);
    private void Type_Inner(string sentence, string speaker = "") {
      if (openDialogBox != null) {
        openDialogBox.Type(sentence, speaker);
      } else {
        Debug.LogWarning("There's no dialog box currently open!");
      }
    }

    /// <summary>
    /// Remove the decision buttons from the screen.
    /// </summary>
    public static void ClearDecisions() => Instance.openDialogBox.ClearDecisions();


    /// <summary>
    /// Open the dialog box with a given name. If no name is provided, the
    /// default dialog box will be opened.
    /// </summary>
    /// <param name="name">The name of the dialog box to open.</param>
    public static void OpenDialogBox(string name = "") => Instance.OpenDialogBox_Inner(name);
    private void OpenDialogBox_Inner(string name = "") {
      if (string.IsNullOrEmpty(name)) {
        OpenDefaultDialogBox();
      } else {

        if (dialogBoxes.ContainsKey(name)) {
          if (openDialogBox == null) {
            openDialogBox = dialogBoxes[name];
            openDialogBox.Open();
          } else {
            SwitchToDialogBox(name);
          }
        } else {
          Debug.LogWarning("The Dialog Box \"" + name + "\" doesn't exist!");
        }

      }
    }

    /// <summary>
    /// Opens or switches to the default dialog box.
    /// </summary>
    private void OpenDefaultDialogBox() {
      if (openDialogBox != null) {
        openDialogBox.Close();
      }

      openDialogBox = DefaultDialogBox;
      openDialogBox.Open();
    }

    /// <summary>
    /// Switch to a dialog box of a given name. If no name is provided, the
    /// default dialog box will be opened.
    /// </summary>
    /// <param name="name">The name of the dialog box to switch to.</param>
    public static void SwitchToDialogBox(string name = "") => Instance.SwitchToDialogBox_Inner(name);
    private void SwitchToDialogBox_Inner(string name = "") {
    if (string.IsNullOrEmpty(name)) {
        OpenDefaultDialogBox();
      } else {

        if (dialogBoxes.ContainsKey(name)) {
          if (openDialogBox != null) {
            openDialogBox.Close();
            openDialogBox = dialogBoxes[name];
            openDialogBox.Open();
          } else {
            OpenDialogBox(name);
          }
        } else {
          Debug.LogWarning("The Dialog Box \"" + name + "\" doesn't exist!");
        }

      }
    }

    /// <summary>
    /// Close the current dialog box.
    /// </summary>
    public static void CloseDialogBox() => Instance.CloseDialogBox_Inner();
    private void CloseDialogBox_Inner() {
      if (openDialogBox != null) {
        openDialogBox.Close();
        openDialogBox = null;
      } else {
        Debug.LogWarning("There is no dialog box open currently!");
      }
    }

    #endregion 

    #region Getters/Setters
    //---------------------------------------------------------------------
    // Getters/Setters
    //---------------------------------------------------------------------

    /// <summary>
    /// Set the current node in the dialog graph.
    /// </summary>
    public static void SetCurrentNode(IAutoNode node) => Instance.graphEngine.SetCurrentNode(node);


    /// <summary>
    /// Get the current node in the dialog graph. Don't use this while in the
    /// middle of another dialog.
    /// </summary>
    public static IAutoNode GetCurrentNode() => Instance.graphEngine.GetCurrentNode();

    /// <summary>
    /// Set the current dialog that should be handled. Don't use this while in
    /// the middle of another dialog.
    /// </summary>
    public static void SetCurrentDialog(IAutoGraph dialog) => Instance.graphEngine.SetCurrentGraph(dialog);

    /// <summary>
    /// Whether or not the dialog has completed.
    /// </summary>
    public static bool IsDialogFinished() => Instance.graphEngine.IsGraphFinished();

    /// <summary>
    /// Get the on screen decision buttons.
    /// </summary>
    /// <returns>The list of decision buttons on screen.</returns>
    public static List<DecisionBox> GetDecisionButtons() => Instance.GetDecisionButtons_Inner();
    private List<DecisionBox> GetDecisionButtons_Inner() {
      return openDialogBox.GetDecisionButtons();
    }

    #endregion
      
    /// <summary>
    /// How the dialog manager should handle the loading of a new scene.
    /// </summary>
    private void OnNewScene(Scene aScene, LoadSceneMode aMode) {
      player = GameManager.Player;
    }

    /// <summary>
    /// Locks handling a dialog. This will prevent more nodes from being fired
    /// in a conversation until the lock has been released.
    /// </summary>
    /// <returns>True if the lock was obtained, false otherwise.</returns>
    public static bool LockNode() => Instance.graphEngine.LockNode();

    /// <summary>
    /// Unlocks handling a dialog. If there was previously a lock on firing more
    /// nodes in the conversation, this will release it.
    /// </summary>
    /// <returns>
    /// Whether or not the current node was locked.
    /// </returns>
    /// <remarks>
    /// Don't use this without first trying to obtain the lock for yourself.
    /// </remarks>
    public static bool UnlockNode() => Instance.graphEngine.UnlockNode();

    /// <summary>
    /// Try to start handling a node in the conversation.
    /// </summary>
    /// <returns>
    /// True if previous node in the conversation graph is finished being handled. False otherwise.
    /// </returns>
    public static bool StartHandlingNode() => Instance.graphEngine.StartHandlingNode();

    /// <summary>
    /// Try to finish handling a node in the conversation.
    /// </summary>
    /// <returns>
    /// True if the current node finished handling successfully. False if the current node still needs time to finish.
    /// </returns>
    public static bool FinishHandlingNode() => Instance.graphEngine.FinishHandlingNode();
  }
}
