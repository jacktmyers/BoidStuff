using System;
using UnityEngine;

public class ControllObject : MonoBehaviour
{
    private IdTracker idTracker;
    public BoidManager WaterBoidManager;
    public BoidSettings WakeSettings;
    private Rigidbody2D rigidBody;
    private DateTime nextBoidTime;
    private string currId;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = this.gameObject.GetComponent<Rigidbody2D>();
        idTracker = this.GetComponent<IdTracker>();
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
        nextBoidTime = DateTime.MaxValue;
        
    }

    void FixedUpdate(){
        float speed = .25f;
        float turnSpeed = 10f;
        if (Input.GetKey("space"))
        {
            this.rigidBody.AddRelativeForce(new Vector2(0,speed), ForceMode2D.Impulse);
            if (nextBoidTime < DateTime.Now){
                WaterBoidManager.CreateBoids(WakeSettings, currId, this.gameObject);
                nextBoidTime = DateTime.Now.AddSeconds(WakeSettings.SpawnCoolDown);
            }
        }
        if (Input.GetKey("left")){
            this.rigidBody.AddTorque(turnSpeed);
        }
        if (Input.GetKey("right")){
            this.rigidBody.AddTorque(turnSpeed * -1.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float speed = .25f;
        float turnSpeed = 10f;
        if (Input.GetKeyDown("space"))
        {
            currId = idTracker.GetUniqueId();
            WaterBoidManager.CreateBoids(WakeSettings, currId, this.gameObject);
            nextBoidTime = DateTime.Now.AddSeconds(WakeSettings.SpawnCoolDown);
            this.rigidBody.AddRelativeForce(new Vector2(0,speed), ForceMode2D.Impulse);
        }
        if (Input.GetKeyUp("space")){
            nextBoidTime = DateTime.MaxValue;
            //WaterBoidManager.KillBoids(WakeSettings, currId);
            //currId = "";
        }
        if (Input.GetKeyDown("left")){
            this.rigidBody.AddTorque(turnSpeed);
        }
        if (Input.GetKeyDown("right")){
            this.rigidBody.AddTorque(turnSpeed * -1.0f);
        }
    }
}
