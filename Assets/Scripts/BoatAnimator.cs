using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using DateTime = System.DateTime;

public class BoatAnimator : MonoBehaviour
{
    public BoatAnimatorSettings Settings;
    private SpriteRenderer spriteRenderer;
    private DateTime chargeStarted;
    private DateTime shot;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Settings.DefaultKeyFrame;
        chargeStarted = DateTime.MaxValue;
        shot = DateTime.MaxValue;
    }

    // Update is called once per frame
    void Update()
    {
        DateTime now = DateTime.Now;
        if ((shot != DateTime.MaxValue) && (Settings.ShootingKeyFrames.Count == Settings.ShootingDelays.Count)){
            for (int i=Settings.ShootingKeyFrames.Count-1; i>=0; i--){
                if (now > shot.AddSeconds(Settings.ShootingDelays.GetRange(0,i).Sum())){
                    spriteRenderer.sprite = Settings.ShootingKeyFrames[i];
                    if (i == Settings.ShootingKeyFrames.Count-1){
                        shot = DateTime.MaxValue;
                    }
                    break;
                }
            }
        }
        if ((chargeStarted != DateTime.MaxValue) && (Settings.ChargingKeyFrames.Count == Settings.ChargingDelays.Count)){
            for (int i=Settings.ChargingKeyFrames.Count-1; i>=0; i--){
                if (now > chargeStarted.AddSeconds(Settings.ChargingDelays.GetRange(0,i).Sum())){
                    spriteRenderer.sprite = Settings.ChargingKeyFrames[i];
                    if (i == Settings.ChargingKeyFrames.Count-1){
                        chargeStarted = DateTime.MaxValue;
                    }
                    break;
                }
            }
        }
    }
    public void StopCharging(){
        chargeStarted = DateTime.MaxValue;
    }
    public void StartCharging(){
        chargeStarted = DateTime.Now;
    }
    public void StartShooting(){
        chargeStarted = DateTime.MaxValue;
        shot = DateTime.Now;
    }
}
