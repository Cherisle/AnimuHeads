using UnityEngine;
using System.Collections;
using System.Linq;

public class TileGenerator : MonoBehaviour
{
	public Object[] myPrefabs;
	public GameObject[,] array;
	private GameObject genLocation;
	private int colControl;
	private int colDestroy;
	private GameObject go;
	private int fallCounter;
	// Use this for initialization
	void Start ()
	{
		fallCounter = 0;
		colDestroy = 0;
		colControl = 5; // default row value for generator
		genLocation = transform.parent.GetComponent<GameBoundary> ().array2D [colControl, 0];
		myPrefabs = Resources.LoadAll ("Characters", typeof(GameObject)).Cast<GameObject> ().ToArray ();
		CreatePrefab();
	}
	void CreatePrefab()
	{
		go = (GameObject) myPrefabs[RandomNumber()];
		colControl = 5;
		genLocation = Instantiate (go, transform.parent.GetComponent<GameBoundary> ().array2D [colControl,0].transform.position, Quaternion.identity) as GameObject;
		Debug.Log ("[5,0] " + "contains " + genLocation.name);
		InvokeRepeating ("Falling", 0.6f, 0.6f);
	}
	void Falling()
	{
		if (fallCounter < 9)
		{	
			if (transform.parent.GetComponent<GameBoundary> ().array2D [colControl, fallCounter + 1].GetComponent<AnimuHead>() != null)
			{
				CancelInvoke ("Falling");
				if (fallCounter + 1 == 1)
				{
					//don't create new prefab
				}
				else
				{
					fallCounter = 0;
					CreatePrefab ();
				}
			}
			else
			{
				if (fallCounter == 0)
				{
					transform.parent.GetComponent<GameBoundary> ().array2D [colControl, fallCounter + 1] = genLocation;
				}
				else
				{
					for (int ii = 0; ii < transform.parent.GetComponent<GameBoundary> ().columns; ii++)
					{
						if (transform.parent.GetComponent<GameBoundary> ().array2D [ii, fallCounter].GetComponent<AnimuHead> () != null)
						{
							transform.parent.GetComponent<GameBoundary> ().array2D [colControl, fallCounter + 1] = transform.parent.GetComponent<GameBoundary> ().array2D [ii, fallCounter];
							colDestroy = ii;
							break;
						}
					}
				}
				//--------------------------------------------------------------------------------------------------------------------
				Destroy (transform.parent.GetComponent<GameBoundary> ().array2D [colControl, fallCounter + 1]); // destroy tile below
				transform.parent.GetComponent<GameBoundary>().array2D[colControl, fallCounter + 1] = Instantiate (transform.parent.GetComponent<GameBoundary>().array2D[colControl,fallCounter+1], new Vector2(colControl*2-9,(fallCounter+1)*-2+9), Quaternion.identity) as GameObject;
				Destroy (transform.parent.GetComponent<GameBoundary> ().array2D [colDestroy, fallCounter]); // destroy AnimuHead tile above
				transform.parent.GetComponent<GameBoundary>().array2D[colDestroy, fallCounter] = Instantiate (Resources.Load("Default/DefaultTile"), new Vector2(colControl*2-9,fallCounter*-2+9), Quaternion.identity) as GameObject;
				Debug.Log ("[" + colControl + "," + (fallCounter + 1) + "] contains " + transform.parent.GetComponent<GameBoundary>().array2D[colControl,fallCounter+1]);
				Debug.Log ("[" + colControl + "," + fallCounter + "] contains " + transform.parent.GetComponent<GameBoundary>().array2D[colControl,fallCounter].name);
				if (fallCounter == 0)
				{
					Destroy (genLocation);
				}
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