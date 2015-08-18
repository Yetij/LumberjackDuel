using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class p2EndGamePanel : MonoBehaviour {

	public Sprite[] winImages;
	public Sprite[] loseImages;
	public Image message;

	public void ShowResult (bool win) {
		gameObject.SetActive(true);
		message.sprite = win? winImages[Random.Range(0,winImages.Length )]:loseImages[Random.Range(0,loseImages.Length )];
	}

	public void OnResetButtonClicked () {
		p2Scene.Instance.OnRematch();
		gameObject.SetActive(false);
	}

	public void OnQuitButtonClicked () {
		Application.Quit();
	}
}
