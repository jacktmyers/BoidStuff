using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BoidBehavior : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    public bool initialized {get; private set;} = false;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = this.gameObject.GetComponent<Rigidbody2D>();
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
    public void DeltaVelocity(Vector2 delta){
        if (!initialized){
            return;
        }
        this.rigidBody.velocity += delta;
    }
    public void ReactiveForces(BoidBehavior[] boids, float protectedRange, float visibleRange, float sepFactor, float alignFactor){
        BoidBehavior[] protectedBoids = boids.Where(b => Vector2.Distance(b.Get2DPos(), this.Get2DPos()) < protectedRange).ToArray();
        BoidBehavior[] visibleBoids = boids.Where(b => Vector2.Distance(b.Get2DPos(), this.Get2DPos()) < visibleRange && !protectedBoids.Contains(b)).ToArray();

        // Separation
        Vector2 sep = Vector2.zero;
        foreach(BoidBehavior currBoid in protectedBoids){
            sep += this.Get2DPos() - currBoid.Get2DPos();
        }
        this.DeltaVelocity(sep*sepFactor);

        // Alignment
        Vector2 velAvg = Vector2.zero;
        int visibleCount = 0;
        foreach(BoidBehavior currBoid in visibleBoids){
            velAvg += currBoid.Get2DVel();
            visibleCount++;
        }
        if (visibleCount > 0){
            velAvg = velAvg / visibleCount;
            this.DeltaVelocity((velAvg - this.Get2DVel())*alignFactor);
        }
    }
}
