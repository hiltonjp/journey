﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Storm.Subsystems.Transitions {

  /// <summary>
  /// A behavior representing a transition from one Unity scene to another.
  /// </summary>
  public class Transition : MonoBehaviour {

    /// <summary>
    /// The scene that will be loaded.
    /// </summary>
    [Tooltip("The scene that will be loaded.")]
    public string destinationScene;

    /// <summary>
    /// The name of the spawn point the player will be set at.
    /// </summary>
    [Tooltip("The name of the spawn point the player will be set at.")]
    public string spawnPointName;

    /// <summary>
    /// The virtual camera that will be activated once the scene loads.
    /// 
    /// </summary>
    [Tooltip("")]
    public string vcamName;


    #region Unity API
    //-------------------------------------------------------------------------
    // Unity API
    //-------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other) {
      if (other.CompareTag("Player")) {
        Debug.Log(spawnPointName);
        TransitionManager.MakeTransition(destinationScene, spawnPointName, vcamName);
      }
    }
    #endregion
  }

}