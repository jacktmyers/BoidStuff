using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class BoidBehavior : MonoBehaviour
{
    //-FOR DEBUGGING RANGES-//
    [Min(10)]
    public int CirclePointCount;
    private LineRenderer visibleRenderer;
    private LineRenderer protectedRenderer;
    //=-=-=-=-=-=-=-=-=-=-=-//

    private Rigidbody2D rigidBody;
    private BoidSettings boidSettings;
    public bool ShowRanges = false;
    public bool Initialized {get; private set;} = false;
    public bool Active {get; private set;} = false;
    public string Id;
    public DateTime Death;
    public DateTime Birth;
    public Vector2 StartingVelocity = new Vector2(0,0);
    public GameObject FollowObject;
    public Vector2 CustomForce;
    // Start is called before the first frame update
    void Start()
    {
        CustomForce = Vector2.zero;
        rigidBody = this.gameObject.GetComponent<Rigidbody2D>();
        visibleRenderer = this.gameObject.transform.Find("VisibleRange").GetComponent<LineRenderer>();
        protectedRenderer = this.gameObject.transform.Find("ProtectedRange").GetComponent<LineRenderer>();
        Initialized = true;
    }
    public void SetBoidSettings(BoidSettings settings)
    {
        this.boidSettings = settings;
    }
    public void Activate(){
        OverrideVelocity(StartingVelocity);
        if (boidSettings.Follow)
        {
            this.gameObject.transform.position = Get2DPos(FollowObject);
        }
        Active = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Vector2 Get2DPos(){
        if (!Initialized){
            return Vector2.zero;
        }
        return new Vector2(this.gameObject.transform.position.x,this.gameObject.transform.position.y);
    }
    public Vector2 Get2DPos(GameObject go){
        return new Vector2(go.transform.position.x, go.transform.position.y);
    }
    public Vector2 Get2DVel(){
        if (!Initialized){
            return Vector2.zero;
        }
        return rigidBody.velocity;
    }
    public float Get2DSpeed(){
        if (!Initialized){
            return 0;
        }
        return rigidBody.velocity.sqrMagnitude;
    }
    public Vector2 Get2DDirection(){
        if (!Initialized){
            return Vector2.zero;
        }
        return rigidBody.velocity.normalized;
    }
    public IEnumerator ApplyForce(Vector2 thrust){
        while (!this.Initialized){
            yield return null;
        }
        this.rigidBody.AddForce(thrust,ForceMode2D.Impulse);
    }
    public void OverrideVelocity(Vector2 vel){
        if (!Initialized){
            return;
        }
        this.rigidBody.velocity = vel;
    }
    private void RenderRange(LineRenderer renderer, float radius){
        renderer.positionCount = CirclePointCount+1;
        Vector2 centerPos = Get2DPos();
        float radStep = 2.0f*Mathf.PI/((float)renderer.positionCount - 1.0f);
        for (int i=0; i<renderer.positionCount; i++){
            renderer.SetPosition(i,new Vector2(centerPos.x + Mathf.Cos(radStep*i)*radius, centerPos.y + Mathf.Sin(radStep*i)*radius));
        }
    }
    
    public void ReactiveForces(BoidBehavior[] boids){
        if (!Initialized || !Active) {
            return;
        }
        if (ShowRanges){
            RenderRange(protectedRenderer,boidSettings.ProtectedRange);
            RenderRange(visibleRenderer,boidSettings.VisibleRange);
        }
        BoidBehavior[] protectedBoids = boids.Where(b => Vector2.Distance(b.Get2DPos(), this.Get2DPos()) < boidSettings.ProtectedRange).ToArray();
        BoidBehavior[] visibleBoids = boids.Where(b => Vector2.Distance(b.Get2DPos(), this.Get2DPos()) < boidSettings.VisibleRange && !protectedBoids.Contains(b)).ToArray();

        // Separation
        Vector2 sep = Vector2.zero;
        foreach(BoidBehavior currBoid in protectedBoids){
            sep += this.Get2DPos() - currBoid.Get2DPos();
        }
        StartCoroutine(ApplyForce(sep*boidSettings.SeparationFactor));

        // Alignment and Cohesion
        Vector2 velAvg = Vector2.zero;
        Vector2 posAvg = Vector2.zero;
        int visibleCount = 0;
        foreach(BoidBehavior currBoid in visibleBoids){
            velAvg += currBoid.Get2DVel();
            posAvg += currBoid.Get2DPos();
            visibleCount++;
        }
        if (visibleCount > 0){
            velAvg = velAvg / visibleCount;
            posAvg = posAvg / visibleCount;
            StartCoroutine(this.ApplyForce((velAvg - this.Get2DVel())*boidSettings.AlignFactor));
            StartCoroutine(this.ApplyForce((posAvg - this.Get2DPos())*boidSettings.CohesionFactor));
        }

        // TODO: FIX THIS TO BE MORE FLEXIBLE
        // Turn Factor
        /*
        if (this.Get2DPos().x < xBounds.x*0.8f)
            StartCoroutine(this.ApplyForce(new Vector2(turnFactor,0)));
        if (this.Get2DPos().x > xBounds.y*0.8f)
            StartCoroutine(this.ApplyForce(new Vector2(-1.0f*turnFactor,0)));
        if (this.Get2DPos().y < yBounds.x*0.8f)
            StartCoroutine(this.ApplyForce(new Vector2(0,turnFactor)));
        if (this.Get2DPos().y > yBounds.y*0.8f)
            StartCoroutine(ApplyForce(new Vector2(0,-1.0f*turnFactor)));
        */

        // Following Point
        if (boidSettings.Follow){
            StartCoroutine(ApplyForce((Get2DPos(FollowObject) - this.Get2DPos())*boidSettings.FollowFactor));
        }

        StartCoroutine(ApplyForce(CustomForce));
        
        // Max and Min Speed
        if (this.Get2DSpeed() < boidSettings.MinSpeed){
            this.OverrideVelocity(this.Get2DDirection() * boidSettings.MinSpeed);
        }
        else if  (this.Get2DSpeed() > boidSettings.MaxSpeed){
            this.OverrideVelocity(this.Get2DDirection() * boidSettings.MaxSpeed);
        }
    }
}
