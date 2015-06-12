using UnityEngine;
using System.Collections;

public class v4ConstValue : MonoBehaviour
{
	public Prefabs prefabNames;
	public ConnectionSettings settings;

	private static v4ConstValue _instance;
	public static v4ConstValue Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v4ConstValue)) as v4ConstValue;
				if ( _instance == null ) throw new UnityException("Object of type v4ConstValue not found");
			}
			return _instance;
		}
	}

}

