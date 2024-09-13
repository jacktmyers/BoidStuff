using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IdTrackerManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int i = 0;
        foreach (IdTracker itracker in FindObjectsByType<IdTracker>(FindObjectsSortMode.None))
        {
            itracker.SetId(i++);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
