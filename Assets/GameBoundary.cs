using UnityEngine;
using System.Collections;

public class GameBoundary : MonoBehaviour
{
	public int rows;
	public int columns;
	public GameObject[,] array2D;
	public GameObject myObject;
	private float maxRayDistX, maxRayDistY;
	private float ctrXLoc, ctrYLoc;
	private Vector2 rectNWCorner,rectNECorner,rectSWCorner;
	private Ray2D northSide,eastSide,southSide,westSide;
	private RaycastHit2D[] hitNorth,hitEast,hitSouth,hitWest;
	private RaycastHit2D hitN,hitE,hitS,hitW;

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
		for(int ii=0;ii<columns;ii++)
		{
			for(int jj=0;jj<rows;jj++)
			{
				array2D [ii, jj] = Instantiate (myObject, new Vector2(ii*2-9,jj*-2+9), Quaternion.identity) as GameObject;
				myObject.name = "Default Tile [" + ii + "," + jj + "]";
				Debug.Log ("[" + ii + "," + jj + "] contains " + myObject.name);
			}
		}
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
	}
}
