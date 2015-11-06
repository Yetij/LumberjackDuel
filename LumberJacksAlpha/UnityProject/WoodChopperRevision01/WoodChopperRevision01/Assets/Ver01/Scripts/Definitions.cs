using UnityEngine;
using System.Collections;

public class Definitions : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

public enum PLAY : int { ER1 = 0, ER2 = 1 };

public enum TreeType : int {
    None, 
    Basic,
    BonusAc,
    Ethereal,
    Monumental,
    Stone,
    Swamp
}

