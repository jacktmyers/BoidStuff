using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class BoidBehavior : MonoBehaviour
{
    [Min(10)]
    public int CirclePointCount;
    private Rigidbody2D rigidBody;
    private LineRenderer visibleRenderer;
    private LineRenderer protectedRenderer;
    public bool ShowRanges = false;
    public bool initialized {get; private set;} = false;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = this.gameObject.GetComponent<Rigidbody2D>();
        visibleRenderer = this.gameObject.transform.Find("VisibleRange").GetComponent<LineRenderer>();
        protectedRenderer = this.gameObject.transform.Find("ProtectedRange").GetComponent<LineRenderer>();

        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Vector2 Get2DPos(){
        if (!initialized){
            return Vector2.zero;
        }
        return new Vector2(this.gameObject.transform.position.x,this.gameObject.transform.position.y);
    }
    public Vector2 Get2DVel(){
        if (!initialized){
            return Vector2.zero;
        }
        return rigidBody.velocity;
    }
    public float Get2DSpeed(){
        if (!initialized){
            return 0;
        }
        return rigidBody.velocity.sqrMagnitude;
    }
    public Vector2 Get2DDirection(){
        if (!initialized){
            return Vector2.zero;
        }
        return rigidBody.velocity.normalized;
    }
    public void ApplyForce(Vector2 thrust){
        if (!initialized){
            return;
        }
        this.rigidBody.AddForce(thrust,ForceMode2D.Impulse);
    }
    public void OverrideVelocity(Vector2 vel){
        if (!initialized){
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
    
    public void ReactiveForces(BoidBehavior[] boids, float protectedRange, float visibleRange, float sepFactor, float alignFactor, float cohesionFactor, float minSpeed, float maxSpeed, float turnFactor, Vector2 yBounds, Vector2 xBounds){
        if (!initialized) {
            return;
        }
        
        if (ShowRanges){
            RenderRange(protectedRenderer,protectedRange);
            RenderRange(visibleRenderer,visibleRange);
        }

        BoidBehavior[] protectedBoids = boids.Where(b => Vector2.Distance(b.Get2DPos(), this.Get2DPos()) < protectedRange).ToArray();
        BoidBehavior[] visibleBoids = boids.Where(b => Vector2.Distance(b.Get2DPos(), this.Get2DPos()) < visibleRange && !protectedBoids.Contains(b)).ToArray();

        // Separation
        Vector2 sep = Vector2.zero;
        foreach(BoidBehavior currBoid in protectedBoids){
            sep += this.Get2DPos() - currBoid.Get2DPos();
        }
        this.ApplyForce(sep*sepFactor);

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
            this.ApplyForce((velAvg - this.Get2DVel())*alignFactor);
            this.ApplyForce((posAvg - this.Get2DPos())*cohesionFactor);
        }

        // Turn Factor
        if (this.Get2DPos().x < xBounds.x*0.8f)
            this.ApplyForce(new Vector2(turnFactor,0));
        if (this.Get2DPos().x > xBounds.y*0.8f)
            this.ApplyForce(new Vector2(-1.0f*turnFactor,0));
        if (this.Get2DPos().y < yBounds.x*0.8f)
            this.ApplyForce(new Vector2(0,turnFactor));
        if (this.Get2DPos().y > yBounds.y*0.8f)
            this.ApplyForce(new Vector2(0,-1.0f*turnFactor));

        // Max and Min Speed
        if (this.Get2DSpeed() < minSpeed){
            this.OverrideVelocity(this.Get2DDirection() * minSpeed);
        }
        else if  (this.Get2DSpeed() > maxSpeed){
            this.OverrideVelocity(this.Get2DDirection() * maxSpeed);
        }
    }
}
