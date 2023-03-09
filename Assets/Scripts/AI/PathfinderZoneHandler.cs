using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfinderZoneHandler : MonoBehaviour
{
    Collider attachedCollider;
    // Start is called before the first frame update
    void Start()
    {
        attachedCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (attachedCollider.GetType() == typeof(BoxCollider))
        {
            BoxCollider box = (BoxCollider)attachedCollider;
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }
}
