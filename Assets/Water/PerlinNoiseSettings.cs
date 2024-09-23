using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[CreateAssetMenu]
public class PerlinNoiseSettings : ScriptableObject
{
    public int Passes;
    public float Scale;
    public Vector2 Speed;
    public float Turbulence;
    public float AddedDropOff;
}
