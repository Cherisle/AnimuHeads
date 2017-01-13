using UnityEngine;
using System.Collections;
using System.Linq;

public class GameBoundary : MonoBehaviour
{
	public const int rows = 10;
	public const int columns = 10;
	public GameObject[,] array2D;
	public GameObject myObject;
	private float maxRayDistX, maxRayDistY;
	private float ctrXLoc, ctrYLoc;
	private Vector2 rectNWCorner,rectNECorner,rectSWCorner;
	private Ray2D northSide,eastSide,southSide,westSide;
	private RaycastHit2D[] hitNorth,hitEast,hitSouth,hitWest;
	private RaycastHit2D hitN,hitE,hitS,hitW;
	private int[,] debugGrid;
    public int[,] identifier;

	void FixedUpdate()
	{
		Debug.DrawLine (rectNWCorner, rectNWCorner + Vector2.right * maxRayDistX);
		Debug.DrawLine (rectNECorner, rectNECorner + Vector2.down * maxRayDistY);
		Debug.DrawLine (rectSWCorner, rectSWCorner + Vector2.right * maxRayDistX);
		Debug.DrawLine (rectNWCorner, rectNWCorner + Vector2.down * maxRayDistY);
	}

	void Start ()
	{
		maxRayDistX = GetComponent<RectTransform>().sizeDelta.x; // stretches the width
		maxRayDistY = GetComponent<RectTransform>().sizeDelta.y; // stretches the height
		ctrXLoc = transform.position.x;
		ctrYLoc = transform.position.y;
		rectNWCorner = new Vector2 (ctrXLoc - maxRayDistX / 2, ctrYLoc + maxRayDistY / 2);
		rectNECorner = new Vector2 (ctrXLoc + maxRayDistX / 2, ctrYLoc + maxRayDistY / 2);
		rectSWCorner = new Vector2 (ctrXLoc - maxRayDistX / 2, ctrYLoc - maxRayDistY / 2);
		northSide = new Ray2D (rectNWCorner, Vector2.right);
		eastSide = new Ray2D (rectNECorner, Vector2.down);
		southSide = new Ray2D (rectSWCorner, Vector2.right);
		westSide = new Ray2D (rectNWCorner, Vector2.down);
		//--------------------------------------------------------------------------------
		myObject = Resources.Load("Default/DefaultTile") as GameObject;
		array2D = new GameObject[columns,rows];
        identifier = new int[columns, rows];
		debugGrid = new int[rows,columns];
		for(int ii=0;ii<columns;ii++)
		{
			for(int jj=0;jj<rows;jj++)
			{
				array2D [ii, jj] = Instantiate (myObject, new Vector2(ii*2-9,jj*-2+9), Quaternion.identity) as GameObject;
				identifier[ii,jj] = 8; //default identifier, we use 0-7 as index
				debugGrid[ii, jj] = 0; //default value for debuggerGrid
				myObject.name = "Default Tile [" + ii + "," + jj + "]";
				//Debug.Log ("[" + ii + "," + jj + "] contains " + myObject.name);
			}
		}
	}

	public int CheckPillar(int row, int col)
	{
		int numMatches = 0; // value we want to return
		int leftOfCol = col-1;
		int rightOfCol = col+1;
		int rowAbove = row-1;
		int fpIdentifier = identifier[row,col];
		if(col == 0 || col == 9)
		{
			return 100; //should check something though
		}
		else
		{
			for(int ii = leftOfCol; ii<=rightOfCol; ii++)
			{
				if (array2D[rowAbove,ii].GetComponent<AnimuHead>() != null) // does AH script exist? in any of the northern neighbors?
				{
					if(fpIdentifier == identifier[rowAbove,ii])
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
			if(fpIdentifier == identifier[row,leftOfCol])
			{
				numMatches++;
				Debug.Log("Found a match with west neighbor AnimuHead");
			}
			if(fpIdentifier == identifier[row,rightOfCol])
			{
				numMatches++;
				Debug.Log("Found a match with east neighbor AnimuHead");
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
		return numMatches;
	}

	public void AHGridUpdate(int r, int c, GameObject g, Vector2 v, Quaternion q)
	{
		//transform.parent.GetComponent<GameBoundary>().array2D[colNum,fallCounter] = Instantiate(go,new Vector2(colNum*2-9,fallCounter*-2+9), Quaternion.identity) as GameObject;
		array2D[r,c] = Instantiate(g,v,q) as GameObject;
	}

	public void idUpdate(int r, int c, int hNum)
	{
		identifier[c,r] = hNum;
		Debug.Log("Identifier [" + c + "," + r + "] = " + hNum);
	}

	// Update is called once per frame
	void Update ()
	{
		/*hitNorth = Physics2D.RaycastAll (northSide.origin, northSide.direction,maxRayDistX);
		hitEast = Physics2D.RaycastAll (eastSide.origin, eastSide.direction,maxRayDistY);
		hitSouth = Physics2D.RaycastAll (southSide.origin, southSide.direction,maxRayDistX);
		hitWest = Physics2D.RaycastAll (westSide.origin, westSide.direction,maxRayDistY);
		for (int ii = 0; ii < hitEast.Length; ii++)
		{
			RaycastHit2D hitSingleEast = hitEast [ii];
			if (hitSingleEast != false && hitSingleEast.collider != null)
			{
				Debug.Log ("Detected collision with East side boundary");
			}
		}
		for (int ii = 0; ii < hitSouth.Length; ii++)
		{
			RaycastHit2D hitSingleSouth = hitSouth [ii];
			if (hitSingleSouth != false && hitSingleSouth.collider != null && hitSingleSouth.collider.GetComponent<AnimuHead>() != null
				&& hitSingleSouth.collider.name != "DefaultTile")
			{
				Debug.Log ("Detected collision with South side boundary");
			}
		}
		for (int ii = 0; ii < hitWest.Length; ii++)
		{
			RaycastHit2D hitSingleWest = hitWest [ii];
			if (hitSingleWest != false && hitSingleWest.collider != null)
			{
				Debug.Log ("Detected collision with West side boundary");
			}
		}*/

        Invoke("tileMaker", 3.0f);
	}

    void tileMaker()
    {
        for(int ii = 0;ii< rows;ii++)
        {
            for (int jj = 0; jj < columns; jj++)
            {
                if(array2D[ii,jj].GetComponent<AnimuHead>() != null)
                {
                    debugGrid[ii,jj] = 1;
                }
                else
                {
                    debugGrid[ii,jj] = 0;
                }
            }
        }

        /*for (int i = 0; i < debugGrid.GetLength(0); i++)
        {
            for (int j = 0; j < debugGrid.GetLength(1); j++)
            {
                Debug.Log(debugGrid[i, j]);
            }
            Debug.Log("\n");
        }*/
    }
}