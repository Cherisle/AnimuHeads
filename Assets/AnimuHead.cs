using UnityEngine;
using System.Collections;

public class AnimuHead : MonoBehaviour
{
	public int headNum;
	// Use this for initialization
	void Start ()
	{
		headNum = 8; //default, unused value in createdHeads array
	}

	public void SetHeadNum(int num)
	{
		headNum = num;
	}

	public int GetHeadNum()
	{
		return headNum;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}