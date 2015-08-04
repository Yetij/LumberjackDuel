using UnityEngine;
using System.Collections;

public class xTime : MonoBehaviour
{
	private static xTime _instance;
	public static xTime Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(xTime)) as xTime;
				if ( _instance == null ) throw new UnityException("Object of type xTime not found");
			}
			return _instance;
		}
	}

	IEnumerator currentUpdateFunction;
	double _time;

	public double time {
		get {
			return currentUpdateFunction == null ? -1 : _time;
		}
		private set {
			_time = value;
		}
	}
	public void OnGameStart () {
		if ( currentUpdateFunction != null ) {
			throw new UnityException("Error with xTime !! OnStart atm should be called only once !");
		}
		_time = 0;
		isPaused = false;
		StartCoroutine( currentUpdateFunction = _Update());
	}
	bool isPaused;

	System.DateTime last_pause;

	public void OnGamePause () {
		last_pause = System.DateTime.UtcNow;
		isPaused = true;
	}
	public void OnGameResume () {
		isPaused = false;
		time += System.DateTime.UtcNow.Subtract(last_pause).TotalSeconds;
	}

	IEnumerator _Update ()
	{
		while (true) {
			if ( isPaused ) yield return null;
			yield return new WaitForEndOfFrame();
			_time += Time.deltaTime;
			yield return null;
		}
	}
}

