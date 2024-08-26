using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor.UIElements;
using UnityEngine;

public class ComputeShaderHandler : MonoBehaviour
{
    int screenWidth = Screen.width;
    int screenHeight = Screen.height;
    public ComputeShader DistanceFromBoidXY;
    public ComputeShader DistanceFromBoid;
    public ComputeShader Fill;
    public bool initialized {get; private set;} = false;
    public AnimationCurve Falloff;
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
        Fill.SetVector("FillValue",new Vector4(1,0,0,1));
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

        // Scale for longest straight screen distance
        //float scale = (screenWidth > screenHeight) ? (1.0f/(float)screenWidth) : (1.0f/(float)screenHeight);

        // Scale for diagonal screen distance
        float scale = 1.0f/(float)Math.Sqrt(Math.Pow(screenWidth,2) + Math.Pow(screenHeight,2));

        DistanceFromBoid.SetTexture(0,"Result", dest);
        DistanceFromBoid.SetFloat("Scale",scale);
        DistanceFromBoid.SetInt("BoidPositionX",boidPixelLocation.x);
        DistanceFromBoid.SetInt("BoidPositionY",boidPixelLocation.y);

        // For Testing
        //DistanceFromBoid.SetInt("BoidPositionX",screenWidth);
        //DistanceFromBoid.SetInt("BoidPositionY",screenHeight);

        DistanceFromBoid.Dispatch(0,screenWidth/8,screenHeight/8,1);
    }
}
