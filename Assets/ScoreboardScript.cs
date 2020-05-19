using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardScript : MonoBehaviour
{
    public static int scoreBoardPoints = 0;
    Text scoreBoardText;

    // Use this for initialization
    void Start()
    {
        scoreBoardText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreBoardText.text = "Score: " + scoreBoardPoints; //keeps scoreboard updated
    }

    public static int calculatePoints(int fpIdentifier, int comboCnt)
    {
        //      Debug.Log("Identifier: " +fpIdentifier); //used to ensure correct fpIdentifier was passed
        //      Debug.Log("Combo Count: " + comboCnt); //used to ensure correct comboCnt was passed 
        int headValue;
        double multiplier;
        switch (fpIdentifier) //assigns point value to head using fpIdentifier, values can change for balance
        {
            case 0:
                headValue = 1000;
                break;
            case 1:
                headValue = 500;
                break;
            case 2:
                headValue = 250;
                break;
            case 3:
                headValue = 100;
                break;
            case 4:
                headValue = 100;
                break;
            case 5:
                headValue = 100;
                break;
            default:
                headValue = 0;
                break;
        }
        switch (comboCnt) //assigns multiplier value depending on how many heads were combo'd, values can be changed for balance
        {
            case 3:
                multiplier = 1;
                break;
            case 4:
                multiplier = 1.25;
                break;
            case 5:
                multiplier = 2;
                break;
            default:
                multiplier = 0;
                break;
        }
        scoreBoardPoints += (int)((double)headValue * multiplier);
        return scoreBoardPoints;
    }

    //I don't think these are necessary unless we have more than one scoreboard
    public int getPoints()
    {
        return scoreBoardPoints;
    }

    public void setPoints(int newPoints)
    {
        scoreBoardPoints = newPoints;
    }
}
