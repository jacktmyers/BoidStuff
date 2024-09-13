using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAdjust : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0));
        this.transform.localScale = new Vector3(worldPoint.x*2, worldPoint.y*2, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
