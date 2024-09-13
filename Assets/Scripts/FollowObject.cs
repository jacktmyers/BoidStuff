using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> Points;
    public float DistanceThreshold;
    public bool initialized { get; private set; }
    private Rigidbody2D rigidBody;
    public float minSpeed;
    public float maxSpeed;
    private int currentTarget;
    public float FollowFactor;
    void Start()
    {
        rigidBody = this.gameObject.GetComponent<Rigidbody2D>();
        initialized = true;
    }
    public IEnumerator ApplyForce(Vector2 thrust){
        while (!this.initialized){
            yield return null;
        }
        this.rigidBody.AddForce(thrust,ForceMode2D.Impulse);
    }
    public float Get2DSpeed(){
        if (!initialized){
            return 0;
        }
        return rigidBody.velocity.sqrMagnitude;
    }
    public void OverrideVelocity(Vector2 vel){
        if (!initialized){
            return;
        }
        this.rigidBody.velocity = vel;
    }
    public Vector2 Get2DDirection(){
        if (!initialized){
            return Vector2.zero;
        }
        return rigidBody.velocity.normalized;
    }
    public Vector2 Get2DPos(){
        if (!initialized){
            return Vector2.zero;
        }
        return new Vector2(this.gameObject.transform.position.x,this.gameObject.transform.position.y);
    }
    // Update is called once per frame
    /*
    void FixedUpdate()
    {
        if (Vector2.Distance(this.Get2DPos(),Get2DPos(Points[currentTarget])) < DistanceThreshold){
            currentTarget = ++currentTarget % Points.Count;
        }
        StartCoroutine(ApplyForce((Get2DPos(Points[currentTarget]) - this.Get2DPos())*FollowFactor));
        if (this.Get2DSpeed() < minSpeed){
            this.OverrideVelocity(this.Get2DDirection() * minSpeed);
        }
        else if  (this.Get2DSpeed() > maxSpeed){
            this.OverrideVelocity(this.Get2DDirection() * maxSpeed);
        }
        
    }
    */
}
