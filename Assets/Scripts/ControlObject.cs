using System;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class ControlObject : MonoBehaviour
{
    public GameObject CannonBallPrefab;
    public ComputeShaderHandler Water;
    public Rigidbody2D UnconstrainedClone;
    public ControlObjectSettings Settings;
    public CannonBallSettings CBSettingsInst;
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
            Vector2 unconstrainedForce = this.transform.up * Settings.Speed;
            Vector2 appliedForce = CheckCollisions(unconstrainedForce);
            this.rigidBody.AddForce(appliedForce, ForceMode2D.Impulse);
            UnconstrainedClone.AddForce(unconstrainedForce, ForceMode2D.Impulse);
        }

        if (Input.GetKey("a")){
            UnconstrainedClone.AddTorque(Settings.TurnSpeed);
            this.rigidBody.AddTorque(Settings.TurnSpeed);
        }
        if (Input.GetKey("d")){
            UnconstrainedClone.AddTorque(Settings.TurnSpeed * -1.0f);
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
                WaterBoidManager.CreateBoids(WakeSettings, currId, UnconstrainedClone.gameObject);
                nextBoidTime = DateTime.Now.AddSeconds(WakeSettings.SpawnCoolDown);
            }
        }
        else {
            if (nextBoidTime < DateTime.Now){
                WaterBoidManager.CreateBoids(IdleSettings, currId, UnconstrainedClone.gameObject);
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
                UnconstrainedClone.AddForce(Settings.KickBack*this.transform.up*-1, ForceMode2D.Impulse);
                ShootCannonBall();
            }
            else{
                boatAnimator.StopCharging();
            }
        }
    }
    private Vector2 CheckCollisions(Vector2 startingForce){
        float xComp = Vector2.Dot(startingForce, Vector2.right);
        float yComp = Vector2.Dot(startingForce, Vector2.up);
        Vector2 controllerDir = new Vector2(xComp, yComp);
        if (controlCollider.IsTouching(Left)){
            controllerDir.x = Math.Max(xComp, 0);
        }
        if (controlCollider.IsTouching(Right)){
            controllerDir.x = Math.Min(xComp, 0);
        }
        if (controlCollider.IsTouching(Top)){
            controllerDir.y = Math.Min(yComp, 0);
        }
        if (controlCollider.IsTouching(Bottom)){
            controllerDir.y = Math.Max(yComp, 0);
        }
        return controllerDir;
    }
    void OnCollisionStay2D(Collision2D col){
        if (controlCollider.IsTouching(Right) || controlCollider.IsTouching(Left)){
            Water.UpdateScreenPositionX(UnconstrainedClone.gameObject.transform.position.x,this.transform.position.x);
        }
        if (controlCollider.IsTouching(Top) || controlCollider.IsTouching(Bottom)){
            Water.UpdateScreenPositionY(UnconstrainedClone.gameObject.transform.position.y,this.transform.position.y);
        }
    }
    public void ShootCannonBall(){
        GameObject cannonBall = Instantiate(CannonBallPrefab);
        cannonBall.transform.position = this.transform.position; 

        cannonBall.GetComponent<Rigidbody2D>().AddForce(boatAnimator.GetChargingForce(DateTime.Now.Second - chargingStart.Second) * this.transform.up, ForceMode2D.Force);
        cannonBall.GetComponent<Rigidbody2D>().AddTorque(UnityEngine.Random.Range(CBSettingsInst.MaxCannonBallTorque * -1, CBSettingsInst.MaxCannonBallTorque));
        cannonBall.GetComponent<CannonBallBehavior>().DisappearSpeed = CBSettingsInst.DisappearSpeed;
        cannonBall.GetComponent<CannonBallBehavior>().WaterBoidManager = WaterBoidManager;
        cannonBall.GetComponent<CannonBallBehavior>().IdTrackerInst = idTracker;
    }
}