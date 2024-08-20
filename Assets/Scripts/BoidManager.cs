using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    private List<BoidBehavior> AllBoids;
    [Min(1)]
    public int BoidCount = 1;
    public BoidBehavior BoidPrefab;
    public Vector2 YSpawnRange;
    public Vector2 XSpawnRange;
    public float ProtectedRange;
    public float VisibleRange;
    public float SeparationFactor;
    public float AlignFactor;
    // Start is called before the first frame update
    void Start()
    {
        AllBoids = new List<BoidBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        if (AllBoids.Count < BoidCount){
            for (int i=0; i<BoidCount-AllBoids.Count; i++){
                BoidBehavior newBoid = Instantiate(BoidPrefab);
                newBoid.gameObject.transform.position = new Vector2(Random.Range(XSpawnRange.x,XSpawnRange.y),Random.Range(YSpawnRange.x,YSpawnRange.y));
                AllBoids.Add(newBoid);
            }
        }
        if (AllBoids.Count > BoidCount){
            for (int i=0; i<AllBoids.Count-BoidCount; i++){
                BoidBehavior removedBoid = AllBoids.Last();
                AllBoids.Remove(removedBoid);
                Destroy(removedBoid.gameObject);
            }
        }
        foreach (BoidBehavior currBoid in AllBoids){
            currBoid.ReactiveForces(AllBoids.ToArray(), ProtectedRange, VisibleRange, SeparationFactor, AlignFactor);
        }
        
    }
}
