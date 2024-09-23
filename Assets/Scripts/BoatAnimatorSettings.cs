using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BoatAnimatorSettings : ScriptableObject
{
    public Sprite DefaultKeyFrame;
    public List<Sprite> ShootingKeyFrames;
    public List<Sprite> ChargingKeyFrames;
    public List<float> ShootingDelays;
    public List<float> ChargingDelays;
    public float Shake;
}
