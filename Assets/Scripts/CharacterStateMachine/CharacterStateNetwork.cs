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
    }

    public void Update()
    {
        activeState.Update();
    }

    public void AddStateToNetwork(CharacterState state) {
        network.Add(state);
        state.network = this;
    }
}
