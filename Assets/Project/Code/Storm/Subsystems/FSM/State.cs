using System;
using UnityEngine;

namespace Storm.Subsystems.FSM {
  public class State : MonoBehaviour {

    protected string AnimParam = "";

    protected IStateMachine FSM;

    #region State Overrides
    /// <summary>
    /// First time initialization that is common to all states that will belong to a specific implentation of a state machine.
    /// Ex. PlayerStates will always need to get a reference to the player.
    /// </summary>
    public virtual void OnStateAddedGeneral() {

    }

    /// <summary>
    /// First time initialization for a specific state. Dependencies common to all states should have been added by this point.
    /// </summary>
    public virtual void OnStateAdded() {

    }

    /// <summary>
    ///  Fires whenever the state is entered into, after the previous state exits.
    /// </summary>
    public virtual void OnStateEnter() {

    }

    /// <summary>
    /// Fires when the state exits, before the next state is entered into.
    /// </summary>
    public virtual void OnStateExit() {

    }


    /// <summary>
    /// Fires once per frame. Use this instead of Unity's built in Update() function.
    /// </summary>
    public virtual void OnUpdate() {

    }

    /// <summary>
    /// Fires with every physics tick. Use this instead of Unity's built in FixedUpdate() function.
    /// </summary>
    public virtual void OnFixedUpdate() {

    } 

    #endregion


    #region State Machine Interfacing

    public void Inject(IStateMachine stateMachine) {
      this.FSM = stateMachine;
    }

    /// <summary>
    /// Pre-hook called by the Player Character when a player state is first added to the player.
    /// </summary>
    public void HiddenOnStateAdded(IStateMachine stateMachine) {
      FSM = stateMachine;
      OnStateAddedGeneral();
      OnStateAdded();
    }

    /// <summary>
    /// Pre-hook called by the Player Character when a player enters a given state.
    /// </summary>
    public void EnterState() {
      enabled = true;

      if (string.IsNullOrEmpty(AnimParam)) {
        throw new UnityException(string.Format("Please set {0}.AnimParam to the name of the animation parameter in the  behavior's Awake() method.", this.GetType()));
      }

      Debug.Log("anim param: " + AnimParam);
      FSM.SetAnimParam(AnimParam);
      OnStateEnter();
    }

    /// <summary>
    /// Pre-hook called by the Player Character when a player exits a given state.
    /// </summary>
    public void ExitState() {
      OnStateExit();
      enabled = false;
    }

    /// <summary>
    /// Change state. The old state behavior will be detached from the player after this call.
    /// </summary>
    public void ChangeToState<S>() where S : State {

      S state;

      // Add the state if it's not already there.
      if (FSM.ContainsState<S>()) {
        state = FSM.GetState<S>();
        
      } else {
        state = gameObject.AddComponent<S>();
        FSM.RegisterState(state);
        state.HiddenOnStateAdded(FSM);
      }

      FSM.OnStateChange(this, state);
    }

    public string GetAnimParam() {
      return AnimParam;
    }
    #endregion

  }
}