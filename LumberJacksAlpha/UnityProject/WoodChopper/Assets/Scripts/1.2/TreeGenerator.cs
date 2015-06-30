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
			CellManager.Instance.OnGenTree(startNb);
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
		while (true) {
			yield return new WaitForSeconds(genTreeInterval);
			CellManager.Instance.OnGenTree(Random.Range(genMin,genMax+1));
		}
	}
}

