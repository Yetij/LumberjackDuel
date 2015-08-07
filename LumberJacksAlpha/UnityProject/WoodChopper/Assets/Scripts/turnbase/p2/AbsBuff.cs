using UnityEngine;
using System.Collections;

public enum BuffTarget { Player, Area, Both }

public abstract class AbsBuff : MonoBehaviour
{
	public BuffTarget target;

	[HideInInspector] public AbsTree source;

	virtual public void OnTurnStartUpdate (p2Player p ) {
	}

	virtual public void OnPreActionUpdate(p2Player p ) {
		Activate(p);
	}

	virtual protected void Activate (p2Player p ) {
	}
}

