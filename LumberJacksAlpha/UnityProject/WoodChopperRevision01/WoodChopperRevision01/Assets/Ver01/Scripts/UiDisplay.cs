using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UiDisplay : MonoBehaviour {
	Text ac, time, player1_points, player2_points;

	void Start () {
		ac = GameObject.Find("_#TextAc").GetComponent<Text>();
		time = GameObject.Find ("_#TextTime").GetComponent<Text>();
		player1_points = GameObject.Find("_#TextPlayer1Points").GetComponent<Text>();
		player2_points = GameObject.Find ("_#TextPlayer2Points").GetComponent<Text>();
		if (ac == null | time == null | player1_points == null | player2_points == null) {
			throw new UnityException("Something is missing, check names of ui elements in Hierarchy");
		}
	}

	public void UpdateAc ( int ac_remain) {
		ac.text = ac_remain.ToString();

	}

	public void UpdatePoints ( PLAY er, int points ) {
		if (er == PLAY.ER1) {
			player1_points.text = points.ToString();
		}
		if (er == PLAY.ER2) {
			player2_points.text = points.ToString();
		}
	}

	public void SetTime ( int i ) {
		time.text = i.ToString();
	}
}
