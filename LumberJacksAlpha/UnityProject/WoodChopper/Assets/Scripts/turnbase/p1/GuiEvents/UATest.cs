using UnityEngine;
using UnityEngine.Events;

public class UATest : MonoBehaviour {
	[SerializeField] public CustomEvent _customEvent;
	
	private void Start() {
		_customEvent.Invoke(true,"hehe");
	}
}

