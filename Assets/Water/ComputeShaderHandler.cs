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
using Unity.VisualScripting;
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
    public int WaterScreenScale;
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
    private int samples;
    public MeshRenderer WaterRenderer;
    public PerlinNoiseSettings PerlinNoiseSettings;
    private ComputeBuffer Gradients;
    private ComputeBuffer RandomRotations;
    public Texture2D ProbeTexture;
    private Vector2 movement;
    private int waterWidth;
    private int waterHeight;
    private Vector2 currentLoc;
    // Start is called before the first frame update
    void Start()
    {
        movement = new Vector2(0,0);
        currentLoc = new Vector2(0,0);
        boidManagerRef = FindObjectsOfType<BoidManager>().First();
        if (boidManagerRef == null){
            throw new Exception("No Object in Scene With Boid Manager Component");
        }
        Gradients = new ComputeBuffer(256, sizeof(float) * 2);
        Gradients.SetData(Enumerable.Range(0, 256).Select((i) => GetRandomDirection()).ToArray());
        RandomRotations = new ComputeBuffer(256, sizeof(float));
        RandomRotations.SetData(Enumerable.Range(0, 256).Select(i => Random.Range(-1.0f*PerlinNoiseSettings.Turbulence, PerlinNoiseSettings.Turbulence)).ToArray());
        waterWidth = Screen.width/WaterScreenScale;
        waterHeight = Screen.height/WaterScreenScale;
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
            outTexture = new RenderTexture(waterWidth, waterHeight, 24);
            outTexture.enableRandomWrite = true;
            outTexture.Create();
        }

        if (distanceTexture == null)
        {
            distanceTexture = new RenderTexture(waterWidth, waterHeight, 0, RenderTextureFormat.RFloat);
            distanceTexture.enableRandomWrite = true;
            distanceTexture.Create();
        }
        if (noiseTexture == null){
            noiseTexture = new RenderTexture(waterWidth, waterHeight, 0, RenderTextureFormat.RFloat);
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

        AggregateDistanceFromBoids(distanceTexture);
        RotateVectors(PerlinNoiseSettings, RandomRotations, Gradients);
        GeneratePerlinNoise(Gradients, PerlinNoiseSettings, distanceTexture, noiseTexture);
        TransformWithLookup(sampleTexture, noiseTexture, outTexture);
        WaterRenderer.material.mainTexture = outTexture;
        WaterRenderer.material.mainTexture.filterMode = FilterMode.Point;
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
    public void GeneratePerlinNoise(ComputeBuffer gradients, PerlinNoiseSettings settings, RenderTexture distances, RenderTexture dest){
        currentLoc += (settings.Speed + movement) * Time.deltaTime;
		PerlinNoise.SetTexture(0, "Result", dest);
		PerlinNoise.SetTexture(0, "distances", distances);
		PerlinNoise.SetFloat("res", (float)dest.height > dest.width? dest.height * settings.Scale : dest.width * settings.Scale);
		PerlinNoise.SetVector("t", currentLoc);
		PerlinNoise.SetBuffer(0, "gradients", gradients);
		PerlinNoise.SetFloat("passes", (float)settings.Passes);
        PerlinNoise.SetFloat("dropOffScale", settings.AddedDropOff);
		PerlinNoise.Dispatch(0, dest.width / 8, dest.height / 8, 1);
    }
    public void FillWithColor(Color fillColor, RenderTexture dest){
        Fill4.SetTexture(0,"Result", dest);
        Fill4.SetVector("FillValue", new Vector4(fillColor.r, fillColor.g, fillColor.b, fillColor.a));
        Fill4.Dispatch(0,dest.width/8,dest.height/8,1);
    }
    public void TransformWithLookup(Texture2D lookupTex, RenderTexture distanceTex, RenderTexture dest){
        TableLookup.SetTexture(0,"Result", dest);
        TableLookup.SetTexture(0,"DistanceData", distanceTex);
        TableLookup.SetTexture(0,"ColorLookup", lookupTex);
        TableLookup.SetInt("ColorLookupSize", samples-1);
        TableLookup.Dispatch(0,dest.width/8,dest.height/8,1);
    }

    public void AggregateDistanceFromBoids(RenderTexture dest){
        dest.Release();
        Fill1.SetTexture(0,"Result", dest);
        Fill1.SetFloat("FillValue", 1.0f);
        Fill1.Dispatch(0,dest.width/8,dest.height/8,1);
        ProbeTexture = toTexture2D(dest);
        /*
        int testPoint = ProbeTexture.GetPixelData<float>(0).IndexOf(0.0f);
        Debug.Log($"{testPoint % dest.width},{testPoint/dest.width}");
        */
        foreach(BoidBehavior boid in boidManagerRef.GetBoidEnumerable()){
            GetDistanceFromBoid(boid,dest);
        }
    }
    public void GetDistanceFromBoid(BoidBehavior boid, RenderTexture dest)
    {
        Func<Vector3,Vector2Int> stripZ = (Vector3 point) => new Vector2Int((int)point.x, (int)point.y);
        Vector2Int boidPixelLocation = stripZ(Camera.current.WorldToScreenPoint(new Vector3(boid.transform.position.x, boid.transform.position.y, 0)));

        // Scale for diagonal screen distance
        float scale = 1.0f/(float)Math.Sqrt(Math.Pow(Screen.width,2) + Math.Pow(Screen.height,2));

        DistanceFromBoid.SetTexture(0,"Result", dest);
        DistanceFromBoid.SetFloat("Scale",scale);
        DistanceFromBoid.SetInt("screenScale", WaterScreenScale);
        DistanceFromBoid.SetInt("BoidPositionX",boidPixelLocation.x);
        DistanceFromBoid.SetInt("BoidPositionY",boidPixelLocation.y);

        // For Testing
        //DistanceFromBoid.SetInt("BoidPositionX",Screen.width);
        //DistanceFromBoid.SetInt("BoidPositionY",Screen.height);

        DistanceFromBoid.Dispatch(0,dest.width/8,dest.height/8,1);
    }
    private static Vector2 GetRandomDirection()
	{
		return new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
	}
    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RFloat, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
    public void MovingOffScreen(Vector2 dir){
        movement = dir;
    }
}
