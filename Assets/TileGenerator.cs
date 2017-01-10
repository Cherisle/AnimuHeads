using UnityEngine;
using System.Collections;
using System.Linq;

public class TileGenerator : MonoBehaviour
{
	public Object[] myPrefabs;
	public GameObject[,] array;
	private GameObject genLocation;
	private int colControl;
	private GameObject go;
	private int fallCounter;
	// Use this for initialization
	void Start ()
	{
		fallCounter = 0;
		colControl = 5; // default row value for generator
		genLocation = transform.parent.GetComponent<GameBoundary> ().array2D [colControl, 0];
		myPrefabs = Resources.LoadAll ("Characters", typeof(GameObject)).Cast<GameObject> ().ToArray ();
		CreatePrefab();
	}
	void CreatePrefab()
	{
		go = (GameObject) myPrefabs[RandomNumber()];
		colControl = 5;
		transform.parent.GetComponent<GameBoundary> ().array2D[colControl,fallCounter] = Instantiate(go,new Vector2(colControl*2-9,fallCounter*-2+9),Quaternion.identity) as GameObject;
		InvokeRepeating ("Falling", 0.6f, 0.6f);
	}
	void Falling()
	{
		if (fallCounter < 9)
		{	
			if (transform.parent.GetComponent<GameBoundary> ().array2D [colControl, fallCounter + 1].GetComponent<AnimuHead>() != null) // AnimuHead below exists
			{
				CancelInvoke ("Falling");
				if (fallCounter == 0)
				{
					//don't create new prefab
				}
				else
				{
					fallCounter = 0;
					CreatePrefab ();
				}
			}
			else // tile below is NOT an AnimuHead, then is DEFAULT
			{
				/*if(fallCounter==0)
				{
					Destroy (transform.parent.GetComponent<GameBoundary> ().array2D [colControl, fallCounter]);
				}*/
				Destroy (transform.parent.GetComponent<GameBoundary> ().array2D [colControl, fallCounter + 1]); // destroy tile below
				transform.parent.GetComponent<GameBoundary>().array2D[colControl, fallCounter + 1] = Instantiate (go, new Vector2(colControl*2-9,(fallCounter+1)*-2+9), Quaternion.identity) as GameObject;
				Destroy (transform.parent.GetComponent<GameBoundary> ().array2D [colControl, fallCounter]); // destroy current tile
				transform.parent.GetComponent<GameBoundary>().array2D[colControl, fallCounter] = Instantiate (Resources.Load("Default/DefaultTile"), new Vector2(colControl*2-9,fallCounter*-2+9), Quaternion.identity) as GameObject;
				Debug.Log ("[" + colControl + "," + (fallCounter + 1) + "] contains " + transform.parent.GetComponent<GameBoundary>().array2D[colControl,fallCounter+1]);
				Debug.Log ("[" + colControl + "," + fallCounter + "] contains " + transform.parent.GetComponent<GameBoundary>().array2D[colControl,fallCounter].name);
				fallCounter++;
			}
		} 
		else
		{
			CancelInvoke ("Falling");
			fallCounter = 0;
			CreatePrefab();
		}

	}
	int RandomNumber()
	{
		System.Random rand = new System.Random();
		return rand.Next(0,myPrefabs.Length);
	}
	void Update()
	{
		if (Input.GetKeyDown (KeyCode.LeftArrow) && colControl > 0)
		{
			colControl = colControl - 1;
		}
		if (Input.GetKeyDown (KeyCode.RightArrow) && colControl < 9)
		{
			colControl = colControl + 1;
		}
	}
				
}