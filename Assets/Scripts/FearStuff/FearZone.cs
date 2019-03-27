using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FearZone : MonoBehaviour {

    /// <summary>
    /// The type of fear which this zone applies
    /// </summary>
    [Tooltip("The type of fear which this zone applies")]
    public FearTypes fearType;

    /// <summary>
    /// The ammount of fear which this zone applies per feartick. Set to 0 if zone is darkness.
    /// </summary>
    [Tooltip("The ammount of fear which this zone applies per feartick. Set to 0 if zone is darkness.")]
    public int fearApplied;

    private void Start()
    {
        if(fearType == FearTypes.Darkness) { fearApplied = 0; }
    }

}
