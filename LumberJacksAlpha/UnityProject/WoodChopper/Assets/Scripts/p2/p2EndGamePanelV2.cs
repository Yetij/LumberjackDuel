using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class p2EndGamePanelV2 : MonoBehaviour {
	public Text result;
	public Text score1;
	public Text score2;
	public Text timerText;
	public float displayTime;

	float timer_trans_right;

	void Awake () {
		//timer_trans_right = timerBar.rectTransform.right;
	}

	public void ShowResult (bool win) {
		gameObject.SetActive(true);
		result.text = win? "YOU WIN":"YOU LOSE";
		foreach ( var p in p3Scene.Instance.players ) {
			if ( p.photonView.isMine ) {
				score1.text = p.totalWinTimes.ToString();
			} else {
				score2.text = p.totalWinTimes.ToString();
			}
		}
		StartCoroutine(_TimerBarRunRun() );
	}

	IEnumerator _TimerBarRunRun() {
		//while ( timerBar.rectTransform.right > 0 ) {
			//timerBar.rectTransform.right -= Time.deltaTime*timer_trans_right/displayTime;

		float timer= displayTime;
		while ( timer > 0 ) {
			timer -= Time.deltaTime;
			timerText.text = Mathf.CeilToInt(timer).ToString();
			yield return null;
		}

		//}

		p3Scene.Instance.OnRematch();
		gameObject.SetActive(false);
	}
}
