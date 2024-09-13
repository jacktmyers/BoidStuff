using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraCaller : MonoBehaviour
{
    public UnityEvent RenderEvents;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnRenderImage(RenderTexture src, RenderTexture dest){
        Graphics.Blit(src,dest);
        RenderEvents.Invoke();
    }
}
