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
    /// The ammount of fear which this zone applies per feartick. Set to one for no change. Do not set to zero.
    /// </summary>
    [Tooltip("The ammount of fear which this zone applies per feartick. Set to one for no change. These stack multiplicitively")]
    public float fearModifier;

}
