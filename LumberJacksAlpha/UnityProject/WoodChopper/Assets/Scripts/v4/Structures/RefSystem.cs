using UnityEngine;
using System.Collections.Generic;

public class RefSystem {
	Dictionary<string, MonoBehaviour > refs;
	public RefSystem (int cap ) { 
		refs = new Dictionary<string, MonoBehaviour >(cap);
	}
	public RefSystem () : this(4) {}
	
	public void Add (string s, MonoBehaviour mono ){
		refs.Add(s,mono);
	}
	public T GetRef<T> (string s ) where T: MonoBehaviour {
		MonoBehaviour m = null;
		refs.TryGetValue(s,out m);
		return (T)m;
	}
}

