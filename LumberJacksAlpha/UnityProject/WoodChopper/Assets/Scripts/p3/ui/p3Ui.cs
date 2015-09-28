using UnityEngine;
using UnityEngine.UI;

public class p3Ui : MonoBehaviour
{
	private static p3Ui _instance;
	public static p3Ui Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p3Ui)) as p3Ui;
				if ( _instance == null ) throw new UnityException("Object of type p3Ui not found");
			}
			return _instance;
		}
	}
	
	public p3UiPregame pregamePanel;
	public p3UiIngame ingamePanel;

	void Awake () {
		Debug.Log("p3Ui Awake");
	}

	void OnLevelWasLoaded(int level) {
		if( Application.loadedLevelName.Equals("p3s2") ) {
			Debug.Log("p3Ui OnLevelWasLoaded p3s2");
			PhotonNetwork.isMessageQueueRunning = true;
			pregamePanel.SetUp();
			ingamePanel.SetUp();

			pregamePanel.Initialize();
			pregamePanel.Run();
		}
	}

	public void AddListener ( IInputListener listener ) {
		ingamePanel.AddListener(listener);
	}
}

