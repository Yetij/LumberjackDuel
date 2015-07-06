using UnityEngine;
using System.Collections;

public class TreeGenerator : MonoBehaviour
{	
	#region hide
	private static TreeGenerator _instance;
	public static TreeGenerator Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(TreeGenerator)) as TreeGenerator;
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
		for ( int k = 0; k < startNb; k ++ ) {
			CellManager.Instance.OnGenTree(Random.Range(1,3));
			yield return new WaitForSeconds(Random.value*0.4f);
		}
		yield return null;
		while (true) {
			yield return new WaitForSeconds(genTreeInterval);
			CellManager.Instance.OnGenTree(Random.Range(genMin,genMax+1));
		}
	}
}

