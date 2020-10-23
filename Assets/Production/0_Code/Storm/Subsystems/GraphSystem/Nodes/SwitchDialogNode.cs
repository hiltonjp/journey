using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

using Storm.Flexible;
using Storm.Subsystems.Save;
using Storm.Subsystems.Transitions;
using UnityEngine.SceneManagement;
using System;

namespace Storm.Subsystems.Graph {

  /// <summary>
  /// A dialog node for switching a conversation graph in the given scene. When
  /// this node is hit, the target dialog holder will have its active dialog
  /// switched with the given dialog. The target does not need to be the same
  /// object the node is sitting on.
  /// </summary>
  [NodeTint(NodeColors.DYNAMIC_COLOR)]
  [NodeWidth(400)]
  [CreateNodeMenu("Dialog/Dynamic/Switch Dialog Node")]
  public class SwitchDialogNode : AutoNode, IStorable {
    /// <summary>
    /// Input connection from the previous node(s).
    /// </summary>
    [Input(connectionType=ConnectionType.Multiple)]
    public EmptyConnection Input;

    [Space(8, order=0)]

    /// <summary>
    /// The person/object to switch up the dialog with.
    /// </summary>
    [Tooltip("The GUID reference to the person/object to switch up the dialog with.")]
    public GuidReference Target;

    [Space(16, order=1)]

    /// <summary>
    /// The dialog graph in the scene that will be used in the conversation. Use
    /// this instead of the asset dialog if you need the graph to reference
    /// objects in the scene. This will be used instead of the asset graph if
    /// both are populated.
    /// </summary>
    [Tooltip("The dialog graph in the scene that will be used in the conversation.\n\nUse this instead of the asset dialog if you need the graph to reference objects in the scene. This will be used instead of the asset graph if both are populated.")]
    public GuidReference Dialog;


    [Space(8, order=2)]

    /// <summary>
    /// Output connection for the next node.
    /// </summary>
    [Output(connectionType=ConnectionType.Override)]
    public EmptyConnection Output;


    public override void Handle() {

      if (Target != null) {
        if (Dialog == null) {
          Debug.LogWarning("DialogSwitch object has no graphs attached to switch.");
          return;
        }

        // If the game object exists, it means the Target is in the current scene.
        if (Target.gameObject != null) {
          Talkative talkative = Target.gameObject.GetComponent<Talkative>();
          AutoGraph dialog = Dialog.gameObject.GetComponent<AutoGraph>();
          talkative.Dialog = dialog;
        }

        // Tell the save system that we're switching out dialogs.
        Store();

      } else {
        Debug.LogWarning("DialogSwitch object has no target!");
      }
    }

    #region Storable API
    //-------------------------------------------------------------------------
    // Storable API
    //-------------------------------------------------------------------------
    public void Store() {
      VSave.Set(StaticFolders.DIALOGS, Target.ToString()+Keys.CURRENT_DIALOG, Dialog.ToByteArray());
    }


    public void Retrieve() {

    }
    #endregion
  }
}