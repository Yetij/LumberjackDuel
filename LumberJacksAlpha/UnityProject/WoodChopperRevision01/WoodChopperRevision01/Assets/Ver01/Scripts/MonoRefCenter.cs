using UnityEngine;
using System.Collections;
using System;

public class MonoRefCenter : MonoBehaviour {
    static private MonoRefCenter _instance;

    static public MonoRefCenter instance
    {
        get
        {
            if ( _instance == null)
            {
                _instance = GameObject.FindObjectOfType<MonoRefCenter>();
            }
            return _instance;
        }
    }
    
    [SerializeField] VisualTree[] treeSeeds;

    public VisualTree Get(TreeType type)
    {
        foreach( var r in treeSeeds )
        {
            if (r.type == type) return r;
        }
        return null;
    }
}
