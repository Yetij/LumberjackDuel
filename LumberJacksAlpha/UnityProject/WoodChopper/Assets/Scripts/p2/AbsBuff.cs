using UnityEngine;
using System.Collections.Generic;
using StaticStructure;

public abstract class AbsBuff : MonoBehaviour
{
	public BuffType type;
	public TreeActivateTime buffTime;
	protected List<p2Player> targets;
	[HideInInspector] public AbsTree source;
	
	virtual protected void Awake () {
		source = GetComponent<AbsTree>();
		targets = new List<p2Player>();
	}
	
	abstract protected void Activate (p2Player _invoker );

	virtual public void ApplyBuff (p2Player _target ) {
		targets.Add (_target);
	}

	virtual public void OnBuffRemovedFromPlayer (p2Player p ) {
		targets.Remove(p);
	}

	//-------------------------- used by pool ---------------------------
	virtual public bool IsBeingUsed () {
		return targets.Count > 0;
	}
}

