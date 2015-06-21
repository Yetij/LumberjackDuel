using UnityEngine;
using System.Collections;

public enum UIType { Canvas, Text, Image, Button } 
[System.Serializable] 
public class UIElement {
	public UIType type;
	public GameObject gameobject;
}
public class TestPanel : MonoBehaviour {
	void f () {
	}
	public UIElement[] elements;
}
