
namespace StaticStructure {
	public enum TreeActivateTime { 
		None, 
		BeforeChop, 
		AfterChop, 
		BeforeMove, 
		AfterMove,
		BeforePlant, 
		AfterPlant,
		BeforeAnyAction, 
		AfterAnyAction,
		BeforeTurn, 
		AfterTurn 
	}

	public enum TreeType : byte { 
		Basic,
		Ethereal,
		Swamp,
		Stone,
		BonusAc,
		Monumental
	} 
	
	public enum TreeState { 
		InSeed, 
		Growing,  
		Grown, 
		WaitDomino,
		Falling 
	} 

	[System.Serializable]
	public class p2PlayerParameters 
	{
		public int actionPoints;
		public int plantCost;
		public int chopCost;
		public int moveCost;
		public int hp;
		public float turnTime;
	}
	
	[System.Serializable]
	public class TreeWeight 
	{
		public TreeType type;
		public int weight;
	}
	public enum BuffType { 
		SingleTarget, 
		MultiTarget 
	}

	public class Angle {
		static UnityEngine.Quaternion q_10 = UnityEngine.Quaternion.Euler(0,0,90);
		static UnityEngine.Quaternion q10 = UnityEngine.Quaternion.Euler(0,0,270);
		static UnityEngine.Quaternion q01 = UnityEngine.Quaternion.Euler(90,0,0);
		static UnityEngine.Quaternion q0_1 = UnityEngine.Quaternion.Euler(270,0,0);

		static UnityEngine.Quaternion q11 = UnityEngine.Quaternion.Euler(90,45,0);
		static UnityEngine.Quaternion q_11 = UnityEngine.Quaternion.Euler(90,315,0);
		static UnityEngine.Quaternion q_1_1 = UnityEngine.Quaternion.Euler(0,315,90);
		static UnityEngine.Quaternion q1_1 = UnityEngine.Quaternion.Euler(0,45,270);

		static UnityEngine.Quaternion[,] map = { { q_1_1, q_10, q_11 }  ,{ q0_1, UnityEngine.Quaternion.identity, q01 } , { q1_1, q10, q11 } };

		public static UnityEngine.Quaternion Convert (int fx, int fz ) {
			return map[fx+1,fz+1];
		}
	}
}

