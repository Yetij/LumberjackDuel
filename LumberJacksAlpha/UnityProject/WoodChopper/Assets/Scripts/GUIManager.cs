using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {

	public Text scoreText;
	int score;
	GameManager managerScript;

	void Awake()
	{
		GameObject manager = GameObject.Find ("GameManagerObject");
		managerScript = manager.GetComponent<GameManager> ();

		//scoreText = GetComponent<Text> ();
		score = 0;
	}
	
	// Update is called once per frame
	void Update ()
	{
		score = managerScript.PlayerPoints;
		scoreText.text = "Score: " + score;
	}

	public void ReturnFunction()
	{
		Application.LoadLevel (0);
	}

	public void RestartFunction()
	{
		Application.LoadLevel (1);
	}
}
