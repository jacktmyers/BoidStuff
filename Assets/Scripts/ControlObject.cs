using System;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class ControlObject : MonoBehaviour
{
    public ControlObjectSettings Settings;
    public ComputeShaderHandler Water;
    public Collider2D Top;
    public Collider2D Bottom;
    public Collider2D Left;
    public Collider2D Right;
    private Collider2D controlCollider;
    private IdTracker idTracker;
    public BoidManager WaterBoidManager;
    public BoidSettings WakeSettings;
    public BoidSettings IdleSettings;
    private Rigidbody2D rigidBody;
    private DateTime nextBoidTime;
    private BoatAnimator boatAnimator;
    private string currId;
    private DateTime chargingStart;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = this.gameObject.GetComponent<Rigidbody2D>();
        controlCollider = this.gameObject.GetComponent<Collider2D>();
        boatAnimator = this.gameObject.GetComponent<BoatAnimator>();
        idTracker = this.GetComponent<IdTracker>();
        chargingStart = DateTime.MaxValue;
        if (idTracker == null)
        {
            throw new System.Exception("Control Object could not find an associated Id Tracker!");
        }
        if (WaterBoidManager == null)
        {
            throw new System.Exception("Control Object was not assigned a Water Boid Manager");
        }
        if (WakeSettings == null)
        {
            throw new System.Exception("Control Object was not assigned a boid setting for Wake");
        }
        nextBoidTime = DateTime.Now.AddSeconds(IdleSettings.SpawnCoolDown);
    }

    void FixedUpdate(){
        // Forward Key Events
        if (Input.GetKey("w"))
        {
            Vector2 appliedForce = CheckCollisions();
            this.rigidBody.AddForce(appliedForce, ForceMode2D.Impulse);
        }
        else {
            Water.MovingOffScreen(Vector2.zero);
            WaterBoidManager.MovingOffScreen(Vector2.zero);
        }

        // Turning Key Events
        if (Input.GetKey("a")){
            this.rigidBody.AddTorque(Settings.TurnSpeed);
        }
        if (Input.GetKey("d")){
            this.rigidBody.AddTorque(Settings.TurnSpeed * -1.0f);
        }
    }

    // Update is called once per frame
    void Update() {
        // Forward Key Events
        if (Input.GetKeyDown("w"))
        {
            currId = idTracker.GetUniqueId();
            nextBoidTime = DateTime.Now.AddSeconds(WakeSettings.SpawnCoolDown);
        }
        if (Input.GetKeyUp("w")){
            nextBoidTime = DateTime.Now.AddSeconds(IdleSettings.SpawnCoolDown);
        }
        if (Input.GetKey("w"))
        {
            if (nextBoidTime < DateTime.Now){
                WaterBoidManager.CreateBoids(WakeSettings, currId, this.gameObject);
                nextBoidTime = DateTime.Now.AddSeconds(WakeSettings.SpawnCoolDown);
            }
        }
        else {
            if (nextBoidTime < DateTime.Now){
                WaterBoidManager.CreateBoids(IdleSettings, currId, this.gameObject);
                nextBoidTime = DateTime.Now.AddSeconds(IdleSettings.SpawnCoolDown);
            }
        }

        // Shooting Key Events
        if (Input.GetKeyDown("space")){
            boatAnimator.StartCharging();
            chargingStart = DateTime.Now;
        }
        if (Input.GetKeyUp("space")){
            if (chargingStart.AddSeconds(Settings.MinimumChargeTime) < DateTime.Now){
                boatAnimator.StartShooting();
                this.rigidBody.AddForce(Settings.KickBack*this.transform.up*-1, ForceMode2D.Impulse);
            }
            else{
                boatAnimator.StopCharging();

            }
        }
    }
    private Vector2 CheckCollisions(){
        float xComp = Vector2.Dot(this.transform.up * Settings.Speed, Vector2.right);
        float yComp = Vector2.Dot(this.transform.up * Settings.Speed, Vector2.up);
        Vector2 controllerDir = new Vector2(xComp, yComp);
        Vector2 waterDir = new Vector2(0,0);
        if (controlCollider.IsTouching(Left)){
            controllerDir.x = Math.Max(xComp, 0);
            waterDir.x = Math.Min(xComp, 0);
        }
        if (controlCollider.IsTouching(Right)){
            controllerDir.x = Math.Min(xComp, 0);
            waterDir.x = Math.Max(xComp, 0);
        }
        if (controlCollider.IsTouching(Top)){
            controllerDir.y = Math.Min(yComp, 0);
            waterDir.y = Math.Max(yComp, 0);
        }
        if (controlCollider.IsTouching(Bottom)){
            controllerDir.y = Math.Max(yComp, 0);
            waterDir.y = Math.Min(yComp, 0);
        }
        Water.MovingOffScreen(waterDir * Settings.OffScreenSpeed);
        WaterBoidManager.MovingOffScreen(waterDir * -1 * Settings.OffScreenWakeSpeed);
        return controllerDir;
    }
}
