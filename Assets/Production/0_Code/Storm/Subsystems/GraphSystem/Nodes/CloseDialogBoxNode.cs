using System.Collections.Generic;
using Sirenix.OdinInspector;
using Storm.Subsystems.Dialog;
using UnityEngine;
using UnityEngine.EventSystems;
using XNode;

namespace Storm.Subsystems.Graph {

  /// <summary>
  /// A dialog node representing list of decisions.
  /// </summary>
  [NodeTint(NodeColors.DBOX_COLOR)]
  [NodeWidth(360)]
  [CreateNodeMenu("Dialog/Dialog Box/Close Dialog Box")]
  public class CloseDialogBoxNode : AutoNode {
    /// <summary>
    /// Input connection from the previous node(s).
    /// </summary>
    [Input(connectionType=ConnectionType.Multiple)]
    public EmptyConnection Input;

    [Space(8, order=0)]


    /// <summary>
    /// Output connection for the next node.
    /// </summary>
    [Output(connectionType=ConnectionType.Override)]
    public EmptyConnection Output;

    #region XNode API
    //---------------------------------------------------
    // XNode API
    //---------------------------------------------------
    
    /// <summary>
    /// Get the value of a port.
    /// </summary>
    /// <param name="port">The input/output port.</param>
    /// <returns>The value for the port.</returns>
    public override object GetValue(NodePort port) {
      return null;
    }
    #endregion


    #region Dialog Node API
    //---------------------------------------------------
    // Dialog Node API
    //---------------------------------------------------
    
    /// <summary>
    /// Invoke the events in the list.
    /// </summary>
    public override void Handle(GraphEngine graphEngine) {
      DialogManager.CloseDialogBox();
    }

    #endregion
  }

}