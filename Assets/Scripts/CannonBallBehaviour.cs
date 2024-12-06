using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallBehavior : MonoBehaviour
{
    [HideInInspector]
    public float DisappearSpeed = 0;
    [HideInInspector]
    public BoidManager WaterBoidManager;
    [HideInInspector]
    public IdTracker IdTrackerInst;
    private Rigidbody2D CBRigidbody;
    public BoidSettings BoidSettingsInst;
    private bool Death = false;
    
    // Start is called before the first frame update
    void Start()
    {
        CBRigidbody = GetComponent<Rigidbody2D>();
        if (CBRigidbody == null){
            throw new System.Exception("Missing RigidBody2D");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Death && CBRigidbody.velocity.magnitude < DisappearSpeed){
            Destroy(GetComponent<SpriteRenderer>());
            WaterBoidManager.CreateBoids(BoidSettingsInst, IdTrackerInst.GetUniqueId(), this.gameObject);
            Death = true;
        }
    }
}
