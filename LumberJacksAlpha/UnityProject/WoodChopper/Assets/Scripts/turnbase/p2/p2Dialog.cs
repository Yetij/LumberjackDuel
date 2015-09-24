using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class p2Dialog : MonoBehaviour {
	public Text text;
	public Animator animator;

	static int hashShowLog = Animator.StringToHash("dialogShow");

	public void PopUp ( string s ) {
		text.text = s;
		animator.Play(hashShowLog,-1,0f);
	}

}
