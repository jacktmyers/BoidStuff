using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Random = UnityEngine.Random;
using Quaternion = UnityEngine.Quaternion;
using System.Security.Cryptography;
using UnityEngine.SocialPlatforms;
using UnityEditor.Experimental.GraphView;


public class BoidManager : MonoBehaviour
{
    private List<BoidBehavior> AllBoids;
    private Vector2 offScreenTranslation;
    void Start()
    {
        AllBoids = new List<BoidBehavior>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DateTime currentTime = DateTime.Now;
        List<BoidBehavior> ToDie = new List<BoidBehavior>();
        foreach (BoidBehavior boid in AllBoids)
        {
            if (currentTime > boid.Death){
                ToDie.Add(boid);
                continue;
            }
            if (!boid.Active && (currentTime > boid.Birth)){
                boid.Activate();
            }
            if (boid.Active) {
                boid.ReactiveForces(AllBoids.ToArray());    
                boid.transform.position = boid.transform.position + Time.deltaTime * new Vector3(offScreenTranslation.x,offScreenTranslation.y,0);
            }
        }
        foreach (BoidBehavior boid in ToDie){
            AllBoids.Remove(boid);
            Destroy(boid.gameObject);
        }
    }
    public Vector2 Get2DPos(GameObject go){
        return new Vector2(go.transform.position.x, go.transform.position.y);
    }
    public void CreateBoids(BoidSettings settings, string id, GameObject followObject = null)
    {
        if (settings.Follow)
        {
            if (followObject == null)
            {
                throw new Exception("Boid was indicated to follow but was not supplied a follow object!");
            }
        }
        DateTime currentTime = DateTime.Now;
        for (int i=0; i<settings.BoidCount; i++)
        {
            BoidBehavior currBoid = Instantiate(settings.BoidPrefab);
            currBoid.Id = id;
            currBoid.SetBoidSettings(settings);
            currBoid.Birth = currentTime.AddSeconds(settings.SpawnCoolDown * i);
            if (settings.Follow){
                currBoid.gameObject.transform.position = followObject.transform.position;
                currBoid.FollowObject = followObject;
            }
            else {
                currBoid.gameObject.transform.position = settings.SpawnLocation;
            }
            if (!settings.Persistent){
                currBoid.Death = currentTime.AddSeconds((settings.SpawnCoolDown * i) + (settings.SpawnCoolDown * i) + settings.Lifetime);
            }
            else {
                currBoid.Death = DateTime.MaxValue;
            }
            if (settings.Splash){
                currBoid.StartingVelocity = new Vector2(Random.Range(settings.MaxSpeed * -1, settings.MaxSpeed),Random.Range(settings.MaxSpeed * -1, settings.MaxSpeed));
            }
            else if (settings.Wake && settings.Follow){
                float calcAngle = (180.0f - settings.WakeAngle)*(float)Math.PI/360.0f;
                float randAngle = Random.Range(calcAngle,(float)Math.PI - calcAngle);
                currBoid.StartingVelocity = followObject.transform.TransformDirection(new Vector2((float)Math.Cos(randAngle), (float)Math.Sin(randAngle)) * -1 * settings.StartSpeed);
            }
            AllBoids.Add(currBoid);
        }
    }
    public void KillBoids(BoidSettings settings, string id){
        DateTime currentTime = DateTime.Now;
        int i = 0;
        AllBoids.Where(b => b.Id == id).ToList().ForEach(b =>
        {
            b.Death = currentTime.AddSeconds(settings.DespawnCoolDown * i);
            i++;
        });
    }
    public IEnumerable<BoidBehavior> GetBoidEnumerable()
    {
        return AllBoids;
    }
    public void MovingOffScreen(Vector2 dir){
        offScreenTranslation = dir;
    }
}
