using UnityEngine;
using System.Collections;

public class ParticleDestructor : MonoBehaviour {

	ParticleSystem dieParticle;
	bool playDone = false;
	
	void Start() 
	{
		dieParticle = GetComponent<ParticleSystem>();
	}
	
	void Update() 
	{
		if(dieParticle && playDone == true)
		{
			if(!dieParticle.IsAlive())
			{
				Destroy(gameObject);
			}
		}
	}

	public void dieParticlePlay()
	{
		dieParticle.Play ();
		playDone = true;
	}
}
