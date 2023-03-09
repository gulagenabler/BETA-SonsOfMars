using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBase : MonoBehaviour
{
    public static Dictionary<ulong, UnitBase> units = new Dictionary<ulong, UnitBase>();
    public delegate void OnClearTaskDelegate();
    public OnClearTaskDelegate OnClearTaskEvent;
    public static ulong idCounter = 0;
    public ulong id;

    private void Start()
    {
        id = idCounter++;
        units.Add(id, this);
    }

    private void OnDestroy()
    {
        if (OnClearTaskEvent != null) {
            OnClearTaskEvent();
        }
        units.Remove(id);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
