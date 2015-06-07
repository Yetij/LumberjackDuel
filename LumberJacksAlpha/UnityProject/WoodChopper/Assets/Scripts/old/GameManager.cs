using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Coordinates
{
	public int i;
	public int j;
	public GameObject tree;
	
	public Coordinates(int I, int J, GameObject Tree)
	{
		i = I;
		j = J;
		tree = Tree;
	}
}

public class GameManager : MonoBehaviour {

	bool freeSpotCon = false;
	List<bool> freeSpots = new List<bool>();

	public int PlayerPoints = 0;

	public GameObject tree;
	public List<Coordinates> treeFree = new List<Coordinates>();
	public List<Coordinates> treePlaced = new List<Coordinates>();

	float cooldown = 0.2f;
	float timer;
	GameObject[] players;

	void Start ()
	{

		players = GameObject.FindGameObjectsWithTag ("Player");//GameObject.Find ("Player");
		for(int i = 0; i < 24; i++)
		{
			for(int j = 0; j < 13; j++)
			{
				Coordinates obj = new Coordinates(i, j, tree);
				treeFree.Add(obj);
			}
		}
	}
	
	void Update ()
	{
		timer += Time.deltaTime;

		if(cooldown < timer)
		{
			timer = 0;
			int place = Random.Range (0,treeFree.Count);
	
			float xMax = treeFree[place].i + 1.0f;
			float xMin = treeFree[place].i - 1.0f;
			float zMax = treeFree[place].j + 1.0f;
			float zMin = treeFree[place].j - 1.0f;

			foreach(GameObject bob in players)
			{
				if(bob.transform.position.x >= xMax || bob.transform.position.x <= xMin || bob.transform.position.z >= zMax 
				   || bob.transform.position.z <= zMin)
				{
					freeSpots.Add(true);
				}
				else
				{
					freeSpots.Add(false);
					Debug.Log("Cannot place a tree");
				}
			}

			//placing tree is available only if players are not standing there
			foreach(bool condition in freeSpots)
			{
				if(condition == true)
					freeSpotCon = true;
				else
				{
					freeSpotCon = false;
					break;
				}
			}

			freeSpots.Clear();

			if(freeSpotCon == true)
			{
				GameObject tempOb = (GameObject)Instantiate(treeFree[place].tree, new Vector3(treeFree[place].i, 0, 
				                                                                              treeFree[place].j), Quaternion.identity);
				treePlaced.Add(treeFree[place]);
				
				//zapisuje obiekt klasy w stworzonym drzewie, żeby potem to drzewo mogło oddać cały obiekt do listy
				tempOb.GetComponent<Tree>().treeCoords = treeFree[place];
				
				treeFree.Remove(treeFree[place]);
			}
		}
	}

	public void Points(float TreePoints)
	{
		PlayerPoints += (int)TreePoints;
	}
}