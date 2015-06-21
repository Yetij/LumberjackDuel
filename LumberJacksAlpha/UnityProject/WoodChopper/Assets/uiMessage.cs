using System.Collections;
using UnityEngine;
using System.Text;

public class uiMessage 
{
	public enum MessageType { Static, Dynamic };

	public string text {
		get {
			return _type == MessageType.Static ? _s : _s + dots[_d];
		}
	}

	string _s;
	float _t;
	int _d;
	MessageType _type;

	static float interval = 0.35f;
	static string[] dots = { ".", "..", "...", "...." };

	StringBuilder sb;

	public uiMessage ( string message, MessageType type ) {
		_type = type;
		_t = 0;
		_d = 0;
		_s = message;
	}

	public uiMessage ( string message ) 
		: this(message, MessageType.Static) {}

	public void Update (float delta)
	{
		if ( _type == MessageType.Static ) return ;
		_t += delta;
		_d  = (int)(_t/interval);
		_d %= dots.Length;
	}
}

