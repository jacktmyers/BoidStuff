using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[CreateAssetMenu]
public class BoidSettings : ScriptableObject
{
    public BoidBehavior BoidPrefab;
    public Vector2 SpawnLocation;
    public Vector2 StartVelocity;
    public bool Splash;
    public bool Wake;
    public bool Persistent;
    public bool Follow;
    public int BoidCount = 1;
    public float SpawnCoolDown;
    public float DespawnCoolDown;
    public float Lifetime;
    public float ProtectedRange;
    public float VisibleRange;
    public float SeparationFactor;
    public float CohesionFactor;
    public float AlignFactor;
    public float FollowFactor;
    public float TurnFactor;
    public float MaxSpeed;
    public float MinSpeed;
    public float StartSpeed;
    public float WakeAngle;
}
