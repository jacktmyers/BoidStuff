using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFilters : MonoBehaviour
{
    public ComputeShader PixelShader;
    public int PixelOutWidth;
    private RenderTexture outTexture;
    public bool EnablePixelFilter;
    public bool initialized {get; private set;} = false;
    // Start is called before the first frame update
    void Start()
    {
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnRenderImage (RenderTexture source, RenderTexture destination){
        if (outTexture == null){
            outTexture = new RenderTexture(Screen.width, Screen.height, source.depth, source.format);
            outTexture.enableRandomWrite = true;
        }
        if (EnablePixelFilter){
            PixelShader.SetTexture(0,"In",source);
            PixelShader.SetTexture(0,"Out",outTexture);
            PixelShader.SetInt("screenWidth", PixelOutWidth);
            PixelShader.SetFloat("invScreenWidth",1.0f/(float)PixelOutWidth);
            PixelShader.Dispatch(0, Screen.width / 8, Screen.width / 8, 1);
            Graphics.Blit(outTexture, destination);
        }
        else{
            Graphics.Blit(source, destination);
        }
    }
}
