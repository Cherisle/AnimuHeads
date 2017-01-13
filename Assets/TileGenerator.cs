using UnityEngine;
using Object = UnityEngine.Object;
using System;
using System.Collections;
using System.Linq;

public class TileGenerator : MonoBehaviour
{
	private const int headMax = 8;
	public Object[] myPrefabs;
	private bool nameMatch;
	private int colNum;
	private int fallCounter;
	//-----------------------------------------------
	private GameObject genLocation;
	private GameObject go;
	private GameObject goBelow;
	private GameObject goCurrent;
	private GameObject goHorz;
	private GameObject rowZeroClone;
	//---Vars Below Here Used For Combo Algorithm----
	private string[] createdHeads;
	private int fpRow; //focal point row value
	private int fpCol; //focal point col value
	private int goGridCnt; // gameObject grid count

	// Use this for initialization
	void Start ()
	{
		createdHeads = new string[headMax];
		fpRow = 0;
		fpCol = 0;
		goGridCnt = 0;
		fallCounter = 0; // default fall counter, 0 means at the very top row, e.g. row 0
		nameMatch = false; // default value for boolean to check for matching gameObject names
		colNum = 5; // default row value for generator
		myPrefabs = Resources.LoadAll ("Characters", typeof(GameObject)).Cast<GameObject> ().ToArray ();
		CreatePrefab();
	}
	void CreatePrefab()
	{
		colNum = 5; // resetting purposes
		nameMatch = false; //resetting purposes
		go = (GameObject) myPrefabs[RandomNumber()]; //randomly generated GameObject "go"
		if(createdHeads.Length == 0)
		{
			createdHeads[0] = go.name;
			go.tag = "" + 0;
		}
		else // array contains previously existing charHead names
		{
			for(int ii=0;ii<createdHeads.Length;ii++) // loop to check through createdHeads name array, and set boolean value accordingly
			{
				if(go.name == createdHeads[ii]) // matching charHead names detected
				{
					nameMatch = true;
					go.tag = "" + ii;
				}
			}
			if(nameMatch == false && createdHeads.Length < headMax)
			{
				createdHeads[createdHeads.Length] = go.name; // adds new AnimuHead name into string array, previous DNE
				go.tag = "" + createdHeads.Length;
				Debug.Log("Added new AnimuHead tag");
			}
		}
		rowZeroClone = Instantiate(go,new Vector2(colNum*2f-9,fallCounter*-2+9),Quaternion.identity) as GameObject;
		rowZeroClone.tag = go.tag;
		InvokeRepeating ("Falling", 0.6f, 0.6f);
	}
	void Falling()
	{
		if (fallCounter < 9)
		{	
			if (transform.parent.GetComponent<GameBoundary> ().array2D [colNum, fallCounter + 1].GetComponent<AnimuHead>() != null) // AnimuHead below exists
			{
				//Debug.Log("Detected AnimuHead at fall counter " + fallCounter);
				transform.parent.GetComponent<GameBoundary>().array2D[colNum,fallCounter] = Instantiate(go,new Vector2(colNum*2-9,fallCounter*-2+9), Quaternion.identity) as GameObject;
				transform.parent.GetComponent<GameBoundary>().array2D[colNum,fallCounter].tag = go.tag;
				Debug.Log(transform.parent.GetComponent<GameBoundary>().array2D[colNum,fallCounter].tag);
				goGridCnt++; // AnimuHead stamped on game grid, this line registers the AnimuHead count
				if(goGridCnt >=3)
				{
					fpRow = fallCounter;
					fpCol = colNum;
					Debug.Log("Focal Point R,C is " + fpRow + "," + fpCol);
					SurroundCheck(fpRow,fpCol);
				}
				//Debug.Log ("AnimuHead Grid Count: " + goGridCnt);
				CancelInvoke ("Falling");
				if (fallCounter == 0)
				{
					Debug.Log("Game should be over");
				}
				else
				{
					Destroy(goCurrent);
					goCurrent = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (colNum*2-9, fallCounter*-2+9), Quaternion.identity) as GameObject;
					fallCounter = 0;
					CreatePrefab ();
				}
			}
			else // tile below is NOT an AnimuHead, then is DEFAULT
			{
				//Debug.Log(fallCounter);
				goCurrent = goBelow; //goBelow was previous below, now's current
				if (fallCounter == 0 && rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				if (fallCounter == 8) // final iteration of THIS else loop
				{
					transform.parent.GetComponent<GameBoundary>().array2D[colNum,fallCounter+1] = Instantiate(go,new Vector2(colNum*2f-9,(fallCounter+1)*-2+9), Quaternion.identity) as GameObject;
					transform.parent.GetComponent<GameBoundary>().array2D[colNum,fallCounter+1].tag = go.tag;
					Debug.Log(go.tag);
					Debug.Log("Stamped Tag is " + transform.parent.GetComponent<GameBoundary>().array2D[colNum,fallCounter].tag);
					goGridCnt++;
					if(goGridCnt >=3)
					{
						fpRow = fallCounter+1;
						fpCol = colNum;
						Debug.Log("Focal Point R,C is " + fpRow + "," + fpCol);
						SurroundCheck(fpRow,fpCol);
					}
					//Debug.Log ("AnimuHead Grid Count: " + goGridCnt);
				}
				else
				{
					// instantiate GameObject tile below current tile
					goBelow = Instantiate (go, new Vector2 (colNum*2f-9, (fallCounter + 1) * -2 + 9), Quaternion.identity) as GameObject;
				}
				// destroys current tile to prepare for new instantiation
				Destroy(goCurrent);
				//--------------------------------------------------------------------------------------------------------------------------
				// instantiate current tile to default (transparent)
				goCurrent = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (colNum*2f-9, fallCounter*-2+9), Quaternion.identity) as GameObject;
				//--------------------------------------------------------------------------------------------------------------------------
				fallCounter++;
			}
		} 
		else
		{
			goCurrent = goBelow; //goBelow was previous below, now's current
			// destroys current tile to prepare for new instantiation
			Destroy(goCurrent);
			//--------------------------------------------------------------------------------------------------------------------------
			// instantiate current tile to default (transparent)
			goCurrent = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (colNum*2f-9, fallCounter*-2+9), Quaternion.identity) as GameObject;
			//--------------------------------------------------------------------------------------------------------------------------
			CancelInvoke ("Falling");
			fallCounter = 0;
			CreatePrefab();
		}

	}

	void SurroundCheck(int row, int col) //parameters are of focal point "fp"
	{
		if(row==9)
		{
			CheckPillar(9,col);
		}
		/*else
		{
			CheckBox(row,col);
		}*/
	}

	void CheckPillar(int row, int col)
	{
		int numMatches = 0;
		int leftOfCol = col-1;
		int rightOfCol = col+1;
		int rowAbove = row-1;
		string fpTag = transform.parent.GetComponent<GameBoundary>().array2D[row,col].tag;
		if(col == 0 || col == 9)
		{
			return; //should check something though
		}
		else
		{
			for(int ii = leftOfCol; ii<=rightOfCol; ii++)
			{
				if (transform.parent.GetComponent<GameBoundary> ().array2D [row-1,ii].GetComponent<AnimuHead>() != null) // north AnimuHead neighbors exist
				{
					if(fpTag == transform.parent.GetComponent<GameBoundary> ().array2D [row-1,ii].tag)
					{
						numMatches++;
						if(ii == leftOfCol) // matched with northwest AnimuHead
						{
							Debug.Log("Found a match with northwest neighbor AnimuHead");
						}
						if(ii == col) // matched with north AnimuHead
						{
							Debug.Log("Found a match with north neighbor AnimuHead");
						}
						if(ii == rightOfCol) // matched with northeast AnimuHead
						{
							Debug.Log("Found a match with northeast neighbor AnimuHead");
						}
					}
				}
			}
			if (transform.parent.GetComponent<GameBoundary> ().array2D [row,leftOfCol].GetComponent<AnimuHead>() != null) // west AH neighbor exists
			{
				if(fpTag.Equals(transform.parent.GetComponent<GameBoundary> ().array2D [row,leftOfCol]))
				{
					numMatches++;
					Debug.Log("Found a match with west neighbor AnimuHead");
				}
			}
			if (transform.parent.GetComponent<GameBoundary> ().array2D [row,rightOfCol].GetComponent<AnimuHead>() != null) // east AH neighbor exists
			{
				if(fpTag.Equals(transform.parent.GetComponent<GameBoundary> ().array2D [row,rightOfCol]))
				{
					numMatches++;
					Debug.Log("Found a match with west neighbor AnimuHead");
				}
			}

		}
		if(numMatches == 1)
		{
			//continue checking in the direction of the match once more
		}
		if(numMatches == 2)
		{
			Debug.Log("We have a 3-combo! :D");
		}
	}

	/*void CheckBox(int row, int col)
	{
		//stuff
	}*/

	int RandomNumber()
	{
		System.Random rand = new System.Random();
		return rand.Next(0,myPrefabs.Length);
	}

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.LeftArrow) && colNum > 0 && fallCounter < 9)
		{
            //prevents overlapping
            if (transform.parent.GetComponent<GameBoundary>().array2D[colNum - 1, fallCounter].GetComponent<AnimuHead>() == null)
			{
				if (fallCounter == 0 && rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				Destroy(goBelow);
				goBelow = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (colNum*2f-9, fallCounter*-2+9), Quaternion.identity) as GameObject;
				colNum -= 1;
				goHorz = Instantiate(go,new Vector2(colNum*2f-9,fallCounter*-2+9),Quaternion.identity) as GameObject; // make left/right GameObject
				goBelow = goHorz; // make current Gameobject now left/right GameObject
			}
            else
			{
                Debug.Log("You hit an animu head on the left D:");  //debugging purposes, delete later
			}
        }
		if (Input.GetKeyDown (KeyCode.RightArrow) && colNum < 9 && fallCounter < 9)
		{
            //prevents overlapping
            if (transform.parent.GetComponent<GameBoundary>().array2D[colNum + 1, fallCounter].GetComponent<AnimuHead>() == null)
			{
				if (fallCounter == 0 && rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				Destroy(goBelow);
				goBelow = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (colNum*2f-9, fallCounter*-2+9), Quaternion.identity) as GameObject;
				colNum += 1;
				goHorz = Instantiate(go,new Vector2(colNum*2f-9,fallCounter*-2+9),Quaternion.identity) as GameObject;
				goBelow = goHorz;
			}
            else
			{
                Debug.Log("You hit an animu head on the right!!!");  //debugging purposes, delete later
			}
        }
	}		
}