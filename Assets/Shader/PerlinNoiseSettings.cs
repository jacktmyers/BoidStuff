using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PerlinNoiseSettings : ScriptableObject
{
    public int Passes;
    public float Scale;
    public float SpeedX;
    public float SpeedY;
    public float Turbulence;
}
