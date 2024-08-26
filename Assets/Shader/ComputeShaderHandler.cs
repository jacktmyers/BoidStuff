using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class ComputeShaderHandler : MonoBehaviour
{
    int screenWidth = Screen.width;
    int screenHeight = Screen.height;
    public ComputeShader DistanceFromBoid;
    public ComputeShader Fill;
    public bool initialized {get; private set;} = false;
    
    private RenderTexture outTexture;
    private BoidManager boidManagerRef;
    // Start is called before the first frame update
    void Start()
    {
        boidManagerRef = FindObjectsOfType<BoidManager>().First();
        if (boidManagerRef == null){
            throw new Exception("No Object in Scene With Boid Manager Component");
        }

        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

/*
    public RenderTexture GetDistanceFromBoid(RenderTexture ogTex, BoidBehavior boid)
    {

    }
    */
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (!this.initialized){
            return;
        }

        if (outTexture == null)
        {
            outTexture = new RenderTexture(screenWidth, screenHeight, 24);
            outTexture.enableRandomWrite = true;
            outTexture.Create();
        }

        outTexture.Release();
        Fill.SetTexture(0,"Result", outTexture);
        Fill.Dispatch(0,screenWidth/8,screenHeight/8,1);
        foreach(BoidBehavior boid in boidManagerRef.GetBoidEnumerable()){
            GetDistanceFromBoid(boid,outTexture);
        }
        Graphics.Blit(outTexture,dest);
    }
    
    public void GetDistanceFromBoid(BoidBehavior boid, RenderTexture dest)
    {
        Func<Vector3,Vector2Int> stripZ = (Vector3 point) => new Vector2Int((int)point.x, (int)point.y);
        Vector2Int boidPixelLocation = stripZ(Camera.current.WorldToScreenPoint(new Vector3(boid.transform.position.x, boid.transform.position.y, 0)));
        float scale = (screenWidth > screenHeight) ? (1.0f/(float)screenWidth) : (1.0f/(float)screenHeight);

        DistanceFromBoid.SetTexture(0,"Result", dest);
        DistanceFromBoid.SetFloat("Scale",scale);
        DistanceFromBoid.SetInt("BoidPositionX",boidPixelLocation.x);
        DistanceFromBoid.SetInt("BoidPositionY",boidPixelLocation.y);
        DistanceFromBoid.Dispatch(0,screenWidth/8,screenHeight/8,1);
    }
}
