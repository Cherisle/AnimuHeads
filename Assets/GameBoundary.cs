using UnityEngine;
using System.Collections;
using System.Linq;

public class GameBoundary : MonoBehaviour
{
	public const int rows = 10;
	public const int columns = 10;
	public GameObject[,] gameGrid;
	public GameObject myObject;
	private float maxRayDistX, maxRayDistY;
	private float ctrXLoc, ctrYLoc;
	private Vector2 rectNWCorner,rectNECorner,rectSWCorner;
    public int[,] identifier;

    //use this to do anything with directions
    public enum directions { UNSET, NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST };

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
		//--------------------------------------------------------------------------------
		myObject = Resources.Load("Default/DefaultTile") as GameObject;
		gameGrid = new GameObject[rows,columns];
        identifier = new int[rows,columns];
		for(int ii=0;ii<rows;ii++)
		{
			for(int jj=0;jj<columns;jj++)
			{
				gameGrid[ii, jj] = Instantiate (myObject,new Vector2(jj*2-9,ii*-2+9), Quaternion.identity) as GameObject;
				identifier[ii,jj] = 8; //default identifier, we use 0-7 as index
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
		directions dir = directions.UNSET; // unset direction path used for default
		if(col == 0 || col == 9)
		{
			return 100; //should check something though
		}
		else
		{
			for(int ii = leftOfCol; ii<=rightOfCol; ii++)
			{
				if (gameGrid[rowAbove,ii].GetComponent<AnimuHead>() != null) // does AH script exist? in any of the northern neighbors?
				{
					if(fpIdentifier == identifier[rowAbove,ii])
					{
						numMatches++;
						if(ii == leftOfCol){ // matched with northwest AnimuHead 
							dir = directions.NORTHWEST;
							Debug.Log("Found a match with northwest neighbor AnimuHead");
						}
						if(ii == col){ // matched with north AnimuHead
							dir = directions.NORTH;
							Debug.Log("Found a match with north neighbor AnimuHead");
						}
						if(ii == rightOfCol){ // matched with northeast AnimuHead
							dir = directions.NORTHEAST;
							Debug.Log("Found a match with northeast neighbor AnimuHead");
						}
					}
				}
			}
			if(fpIdentifier == identifier[row,leftOfCol])
			{
				numMatches++;
				dir = directions.WEST;
				Debug.Log("Found a match with west neighbor AnimuHead");
			}
			if(fpIdentifier == identifier[row,rightOfCol])
			{
				numMatches++;
				dir = directions.EAST;
				Debug.Log("Found a match with east neighbor AnimuHead");
			}
		}
		if(numMatches == 1)
		{
			//continue checking in the direction of the match once more
		}
		if(numMatches == 2)
		{
			//Debug.Log("We have a 3-combo! :D");
		}
		return numMatches;
	}

	private void ContDirCheck(directions d) //continue direction check
	{
		switch(d)
		{
			case directions.NORTH: ContNorthCheck(); break;
			case directions.SOUTH: ContSouthCheck(); break;
			case directions.WEST: ContWestCheck(); break;
			case directions.EAST: ContEastCheck(); break;
			case directions.NORTHWEST: ContNWCheck(); break;
			case directions.NORTHEAST: ContNECheck(); break;
			case directions.SOUTHWEST: ContSWCheck(); break;
			case directions.SOUTHEAST: ContSECheck(); break;
		}
	}

	private void ContNorthCheck()
	{
		//continue your northern combo check;
	}

	private void ContSouthCheck()
	{
		//continue your southern combo check;
	}

	private void ContWestCheck()
	{
		//continue your western combo check;
	}

	private void ContEastCheck()
	{
		//continue your eastern combo check;
	}

	private void ContNWCheck()
	{
		//continue your northwest combo check;
	}

	private void ContNECheck()
	{
		//continue your northeast combo check;
	}

	private void ContSWCheck()
	{
		//continue your southwest combo check;
	}

	private void ContSECheck()
	{
		//continue your southeast combo check;
	}

	public void idUpdate(int r, int c, int hNum)
	{
		identifier[r,c] = hNum;
		Debug.Log("Identifier [" + r + "," + c + "] = " + hNum);
	}
		
	void Update ()
	{
		
	}
}