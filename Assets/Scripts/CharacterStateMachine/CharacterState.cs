using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterState {
    public List<StateTransition> transitions;
    public string name;
    public CharacterStateNetwork network;

    public CharacterState(string name) {
        this.name = name;
    }

    public CharacterState(string name, CharacterStateNetwork network) {
        this.name = name;
        this.network = network;
        network.AddStateToNetwork(this);
    }

    public virtual void OnStateEnter() {
        if (network) {
            network.activeState = this;
        }
    }
    public abstract void OnStateExit();
    public abstract void Subject();
    public abstract void Update();

    public void AddTransition(CharacterState to, UnityEngine.Events.UnityAction callback = null) {
        StateTransition transition = new StateTransition(this, to, callback);
    }

    public void Transition(string stateName) {
        foreach (StateTransition transition in transitions) {
            if (transition.to.name == stateName) {
                transition.Transition();
                break;
            }
        }
    }
    public void Transition(CharacterState to) {
        foreach (StateTransition transition in transitions) {
            if (transition.to == to) {
                transition.Transition();
                break;
            }
        }
    }
    public static implicit operator bool(CharacterState state) {
        if (state.network != null)
        {
            return !object.ReferenceEquals(state, null);
        }
        else { return false; }
    }
}

public class StateTransition {
    public CharacterState from;
    public CharacterState to;
    public UnityEngine.Events.UnityAction continueWith;

    public StateTransition(CharacterState from, CharacterState to, UnityEngine.Events.UnityAction callback) {
        this.from = from;
        this.to = to;
        this.continueWith = callback;
    }

    public void Transition() {
        to.OnStateEnter();
        from.OnStateExit();

        continueWith();
    }
}
