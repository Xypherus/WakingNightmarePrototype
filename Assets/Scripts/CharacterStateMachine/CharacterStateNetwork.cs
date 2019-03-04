using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterStateNetwork : MonoBehaviour{
    List<CharacterState> network;
    public CharacterState activeState;

    public CharacterStateNetwork() {
        network = new List<CharacterState>();
    }

    public CharacterStateNetwork(CharacterState[] states) {
        network = new List<CharacterState>(states);
        foreach (CharacterState state in network) {
            state.network = this;
        }
        activeState = network[0];
    }

    public void Update()
    {
        activeState.Subject();
        activeState.Update();
    }

    public void FixedUpdate()
    {
        activeState.Subject();
        activeState.FixedUpdate();
    }

    public void AddStateToNetwork(CharacterState state) {
        network.Add(state);
        state.network = this;
    }
}
