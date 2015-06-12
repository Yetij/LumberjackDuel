[System.Serializable]
public struct Int2 
{
	public int x,z;

	public static bool operator==(Int2 m, Int2 n ) {
		return m.x == n.x & m.z == n.z;
	}

	public static bool operator!=(Int2 m, Int2 n ) {
		return m.x != n.x | m.z != n.z;
	}
}

