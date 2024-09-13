using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IdTracker : MonoBehaviour
{
    private string leaderId;
    public bool Initialized {get; private set;} = false;
    private DateTime refTime = new DateTime(1970, 1, 1);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetId(int id){
        leaderId = id.ToString();
        this.Initialized = true;
    }

    public string GetUniqueId() {
        return $"{leaderId}-{DateTime.Now.Subtract(refTime).TotalSeconds.ToString()}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
