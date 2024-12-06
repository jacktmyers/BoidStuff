using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CannonBallSettings : ScriptableObject
{
    public float MaxCannonBallTorque;
    public float DisappearSpeed;
    public float[] CannonBallForces;
}
