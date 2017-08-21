using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {


    private Button startButton;
    private Button optionsButton;
    private Button creditsButton;
    private Button exitButton;
 
    public void Start()
    {
        //changing the text names for the buttons 
        GameObject.Find("StartButton").GetComponentInChildren<Text>().text = "Start";
        GameObject.Find("OptionsButton").GetComponentInChildren<Text>().text = "Options";
        GameObject.Find("CreditsButton").GetComponentInChildren<Text>().text = "Credits";
        GameObject.Find("ExitButton").GetComponentInChildren<Text>().text = "Exit Game";
        //finished changing text names for buttons

        //adding OnClick listeners to the buttons
        startButton = GameObject.Find("StartButton").GetComponent<Button>();
        startButton.onClick.AddListener(startButtonOnClick);

        optionsButton = GameObject.Find("OptionsButton").GetComponent<Button>();
        optionsButton.onClick.AddListener(optionsButtonOnClick);

        creditsButton = GameObject.Find("CreditsButton").GetComponent<Button>();
        creditsButton.onClick.AddListener(creditsButtonOnClick);

        exitButton = GameObject.Find("ExitButton").GetComponent<Button>();
        exitButton.onClick.AddListener(exitButtonOnClick);


        //------------------------------------------------------------------------

        Time.timeScale = 1; //time is passing in real time. 0 means paused
 
    }

    public void startButtonOnClick()
    {
        Debug.Log("Start Button Clicked");
        StartGame();
    }

    public void optionsButtonOnClick()
    {
        Debug.Log("Options Button Clicked");
        //Todo
    }

    public void creditsButtonOnClick()
    {
        Debug.Log("Main Menu Button Clicked");
        //Todo
    }

    public void exitButtonOnClick()
    {
        Debug.Log("Exit Button Clicked");
        Application.Quit();
    }
    // Update is called once per frame
    public void Update()
    {

    }



    //Starts the game
    public void StartGame()
    {
        SceneManager.LoadScene("Test");
    }

 
  
}
