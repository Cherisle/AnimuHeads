using UnityEngine;
using System.Collections;
using System.Linq;

public class GameBoundary : MonoBehaviour
{
	public const int rows = 10;
	public const int columns = 10;
	private const float fallDownDelay = 0.6f;
	public GameObject[,] gameGrid;
	public GameObject myObject;
	private float maxRayDistX, maxRayDistY;
	private float ctrXLoc, ctrYLoc;
	private Vector2 rectNWCorner,rectNECorner,rectSWCorner;
	private directions dir;
	private int comboCnt;
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
		comboCnt = 0; // initialize
		dir = directions.UNSET; // initialize
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
		int leftOfCol = col-1;
		int rightOfCol = col+1;
		int rowAbove = row-1;
		int fpIdentifier = identifier[row,col];
		bool checkWest = false;
		bool checkNorthWest = false;
		bool checkNorth = false;
		bool checkNorthEast = false;
		bool checkEast = false;
		if(col == 0 || col == 9)
		{
			return 100; //should check something though
		}
		else
		{
			for(int ii = leftOfCol; ii<=rightOfCol; ii++)
			{
				if(fpIdentifier == identifier[rowAbove,ii])
				{
					comboCnt++;
					if(ii == leftOfCol){ // matched with northwest AnimuHead 
						dir = directions.NORTHWEST;
						checkNorthWest = true;
						ContDirCheck(dir,rowAbove,leftOfCol);
						//Debug.Log("Found a match with northwest neighbor AnimuHead");
					}
					if(ii == col){ // matched with north AnimuHead
						dir = directions.NORTH;
						checkNorth = true;
						ContDirCheck(dir,rowAbove,col);
						//Debug.Log("Found a match with north neighbor AnimuHead");
					}
					if(ii == rightOfCol){ // matched with northeast AnimuHead
						dir = directions.NORTHEAST;
						checkNorthEast = true;
						ContDirCheck(dir,rowAbove,rightOfCol);
						//Debug.Log("Found a match with northeast neighbor AnimuHead");
					}
				}
			}
			if(fpIdentifier == identifier[row,leftOfCol])
			{
				comboCnt++;
				dir = directions.WEST;
				checkWest = true;
				ContDirCheck(dir,row,leftOfCol);
				//Debug.Log("Found a match with west neighbor AnimuHead");
			}
			if(fpIdentifier == identifier[row,rightOfCol])
			{
				comboCnt++;
				dir = directions.EAST;
				checkEast = true;
				ContDirCheck(dir,row,rightOfCol);
				//Debug.Log("Found a match with east neighbor AnimuHead");
			}
		}
		if(checkWest == true && checkEast == true) //instant combo 1st condition, w/o continual dirCheck
		{
			if(checkNorthWest != true && checkNorth != true && checkNorthEast != true) //northern neighbors don't match, then can break 3-5 in a row HORZ
			{
				if(comboCnt>3){ // must be 4 or 5 combo as a result of the above
					//break the combo'd heads with original focal point somewhere in the middle
				}
				else{ // must be a 3 combo with focal point directly in the middl
					Destroy(gameGrid[row,leftOfCol],fallDownDelay);
					Destroy(gameGrid[row,col],fallDownDelay);
					Destroy(gameGrid[row,rightOfCol],fallDownDelay);
					gameGrid[row,leftOfCol] = Instantiate(myObject,new Vector2(leftOfCol*2-9,row*-2+9), Quaternion.identity) as GameObject;
					gameGrid[row,col] = Instantiate(myObject,new Vector2(col*2-9,row*-2+9),Quaternion.identity) as GameObject;
					gameGrid[row,rightOfCol] = Instantiate(myObject,new Vector2(rightOfCol*2-9,row*-2+9),Quaternion.identity) as GameObject;
				}
			}
		}
		return comboCnt;
	}

	private void ContDirCheck(directions d, int fpRow, int fpCol) //continue direction check w/ UPDATED focal point
	{
		switch(d)
		{
			case directions.NORTH: ContNorthCheck(fpRow,fpCol); break;
			case directions.SOUTH: ContSouthCheck(fpRow,fpCol); break;
			case directions.WEST: ContWestCheck(fpRow,fpCol); break;
			case directions.EAST: ContEastCheck(fpRow,fpCol); break;
			case directions.NORTHWEST: ContNWCheck(fpRow,fpCol); break;
			case directions.NORTHEAST: ContNECheck(fpRow,fpCol); break;
			case directions.SOUTHWEST: ContSWCheck(fpRow,fpCol); break;
			case directions.SOUTHEAST: ContSECheck(fpRow,fpCol); break;
			case directions.UNSET: break; //does nothing
		}
	}

	private void ContNorthCheck(int fpRow, int fpCol)
	{
		//continue your northern combo check;
	}

	private void ContSouthCheck(int fpRow, int fpCol)
	{
		//continue your southern combo check;
	}

	private void ContWestCheck(int fpRow, int fpCol)
	{
		//continue your western combo check;
		if(identifier[fpRow,fpCol] == identifier[fpRow,fpCol-1])
		{
			comboCnt++;
			dir = directions.WEST;
			ContDirCheck(dir,fpRow,fpCol-1); // send in an updated focal point
			Debug.Log("Found continuous match with west neighbor AnimuHead");
		}
		else
		{
			//combo has ended in this direction, return if is 3 or more
		}
	}

	private void ContEastCheck(int fpRow, int fpCol)
	{
		//continue your eastern combo check;
	}

	private void ContNWCheck(int fpRow, int fpCol)
	{
		//continue your northwest combo check;
	}

	private void ContNECheck(int fpRow, int fpCol)
	{
		//continue your northeast combo check;
	}

	private void ContSWCheck(int fpRow, int fpCol)
	{
		//continue your southwest combo check;
	}

	private void ContSECheck(int fpRow, int fpCol)
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