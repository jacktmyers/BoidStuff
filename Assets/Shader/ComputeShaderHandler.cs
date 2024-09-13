using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Microsoft.Unity.VisualStudio.Editor;
using TreeEditor;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TerrainTools;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

public class ComputeShaderHandler : MonoBehaviour
{
    public ComputeShader DistanceFromBoidXY;
    public ComputeShader DistanceFromBoid;
    public ComputeShader TableLookup;
    public ComputeShader PerlinNoise;
    public ComputeShader RotateVec2;
    public Color Fill4Value;
    public ComputeShader Fill4;
    public ComputeShader Fill1;
    public bool initialized {get; private set;} = false;
    //private RenderTexture outTexture;
    public RenderTexture outTexture;
    //private RenderTexture distanceTexture;
    public RenderTexture distanceTexture;
    public RenderTexture noiseTexture;
    //private Texture2D sampleTexture;
    public Texture2D sampleTexture;
    private BoidManager boidManagerRef;
    private int screenWidth = Screen.width;
    private int screenHeight = Screen.height;
    private int samples;
    public MeshRenderer WaterRenderer;
    public PerlinNoiseSettings PerlinNoiseSettings;
    private ComputeBuffer Gradients;
    private ComputeBuffer RandomRotations;
    // Start is called before the first frame update
    void Start()
    {
        boidManagerRef = FindObjectsOfType<BoidManager>().First();
        if (boidManagerRef == null){
            throw new Exception("No Object in Scene With Boid Manager Component");
        }
        Gradients = new ComputeBuffer(256, sizeof(float) * 2);
        Gradients.SetData(Enumerable.Range(0, 256).Select((i) => GetRandomDirection()).ToArray());
        RandomRotations = new ComputeBuffer(256, sizeof(float));
        RandomRotations.SetData(Enumerable.Range(0, 256).Select(i => Random.Range(-1.0f*PerlinNoiseSettings.Turbulence, PerlinNoiseSettings.Turbulence)).ToArray());
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
    public void RenderImage()
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

        if (distanceTexture == null)
        {
            distanceTexture = new RenderTexture(screenWidth, screenHeight, 0, RenderTextureFormat.RFloat);
            distanceTexture.enableRandomWrite = true;
            distanceTexture.Create();
        }
        if (noiseTexture == null){
            noiseTexture = new RenderTexture(screenWidth, screenHeight, 0, RenderTextureFormat.RFloat);
            noiseTexture.enableRandomWrite = true;
            noiseTexture.Create();
        }
        
        if (sampleTexture == null)
        {
            throw new Exception("Sample Texture not Included!");
        }
        else {
            samples = sampleTexture.width;
        }

        //AggregateDistanceFromBoids(distanceTexture);
        //TransformWithLookup(sampleTexture, distanceTexture, outTexture);
        RotateVectors(PerlinNoiseSettings, RandomRotations, Gradients);
        GeneratePerlinNoise(Gradients, PerlinNoiseSettings, noiseTexture);
        TransformWithLookup(sampleTexture, noiseTexture, outTexture);
        WaterRenderer.material.mainTexture = outTexture;
        //Graphics.Blit(outTexture,WaterRenderer.material);
    }
    public void CurveToTexture(int samples, Texture2D dest, AnimationCurve redCurve, AnimationCurve greenCurve, AnimationCurve blueCurve, AnimationCurve alphaCurve){
        float step = 1.0f/(float)samples;
        for (int i=1; i<=samples; i++){
            dest.SetPixel(i-1,0,new Color(
                redCurve.Evaluate(step*i),
                greenCurve.Evaluate(step*i),
                blueCurve.Evaluate(step*i),
                alphaCurve.Evaluate(step*i)
            ));
        }
        dest.Apply();
    }
    public void RotateVectors(PerlinNoiseSettings settings, ComputeBuffer rotations, ComputeBuffer gradients) {
        RotateVec2.SetBuffer(0, "Result", gradients);
        RotateVec2.SetBuffer(0, "rotations", rotations);
		RotateVec2.Dispatch(0, 256/8, 1, 1);
	}
    public void GeneratePerlinNoise(ComputeBuffer gradients, PerlinNoiseSettings settings, RenderTexture dest){
		PerlinNoise.SetTexture(0, "Result", dest);
		PerlinNoise.SetFloat("res", (float)screenHeight > screenWidth? screenHeight * settings.Scale : screenWidth * settings.Scale);
		PerlinNoise.SetFloat("tx", (float) EditorApplication.timeSinceStartup * settings.SpeedX);
		PerlinNoise.SetFloat("ty", (float) EditorApplication.timeSinceStartup * settings.SpeedY);
		PerlinNoise.SetBuffer(0, "gradients", gradients);
		PerlinNoise.SetFloat("passes", (float)settings.Passes);
		PerlinNoise.Dispatch(0, screenWidth / 8, screenWidth / 8, 1);
    }
    public void FillWithColor(Color fillColor, RenderTexture dest){
        Fill4.SetTexture(0,"Result", dest);
        Fill4.SetVector("FillValue", new Vector4(fillColor.r, fillColor.g, fillColor.b, fillColor.a));
        Fill4.Dispatch(0,screenWidth/8,screenHeight/8,1);
    }
    public void TransformWithLookup(Texture2D lookupTex, RenderTexture distanceTex, RenderTexture dest){
        TableLookup.SetTexture(0,"Result", dest);
        TableLookup.SetTexture(0,"DistanceData", distanceTex);
        TableLookup.SetTexture(0,"ColorLookup", lookupTex);
        TableLookup.SetInt("ColorLookupSize", samples-1);
        TableLookup.Dispatch(0,screenWidth/8,screenHeight/8,1);
    }

    public void AggregateDistanceFromBoids(RenderTexture dest){
        dest.Release();
        Fill1.SetTexture(0,"Result", dest);
        Fill1.SetFloat("FillValue",1.0f);
        Fill1.Dispatch(0,screenWidth/8,screenHeight/8,1);
        foreach(BoidBehavior boid in boidManagerRef.GetBoidEnumerable()){
            GetDistanceFromBoid(boid,dest);
        }
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
    private static Vector2 GetRandomDirection()
	{
		return new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
	}
}
