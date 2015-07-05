using UnityEngine;
using System.Collections;

public class p0TreeGenerator : MonoBehaviour
{	
	#region hide
	private static p0TreeGenerator _instance;
	public static p0TreeGenerator Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p0TreeGenerator)) as p0TreeGenerator;
			}
			return _instance;
		}
	}
	#endregion
	
	public void OnGameStart () {
		if ( PhotonNetwork.isMasterClient ) {
			StartCoroutine(update = _Update());
		}
	}
	public void OnGameEnd() {
		if ( PhotonNetwork.isMasterClient )
			StopCoroutine(update);
	}
	
	public float genTreeInterval;
	public int genMin,genMax;
	public int startNb;
	
	IEnumerator update;
	IEnumerator _Update () {
		yield return null;
		CellManager.Instance.OnGenTree(startNb);
		yield return null;
		while (true) {
			yield return new WaitForSeconds(genTreeInterval);
			CellManager.Instance.OnGenTree(Random.Range(genMin,genMax+1));
		}
	}
}

