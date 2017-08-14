
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//tutorial used: https://www.sitepoint.com/adding-pause-main-menu-and-game-over-screens-in-unity/

public class PauseMenu : MonoBehaviour
{
    /// <summary>
    /// ////////////////
    /// </summary>
    public GameObject[] pauseObjects;

    /// Pause Menu
    /// ////////////////////////////
    /// </summary>

	private Button playButton;
	private Button restartButton;
	private Button mainMenuButton;
   
   public void Start()
    {
        //changing the text names for the buttons 
		GameObject.Find("PlayButton").GetComponentInChildren<Text>().text = "Resume";
        GameObject.Find("RestartButton").GetComponentInChildren<Text>().text = "Restart";      
        GameObject.Find("MainMenuButton").GetComponentInChildren<Text>().text = "Main Menu";
		//finished changing text names for buttons

		//adding OnClick listeners to the buttons
		playButton = GameObject.Find("PlayButton").GetComponent<Button>();
		playButton.onClick.AddListener(playButtonOnClick);
		restartButton = GameObject.Find("RestartButton").GetComponent<Button>();
		restartButton.onClick.AddListener(restartButtonOnClick);
		mainMenuButton = GameObject.Find("MainMenuButton").GetComponent<Button>();
		mainMenuButton.onClick.AddListener(mainMenuButtonOnClick);
		//------------------------------------------------------------------------

        Time.timeScale = 1; //time is passing in real time. 0 means paused
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        HidePaused();
    }

	public void playButtonOnClick()
	{
		Debug.Log("Resume Button Clicked");
        PauseControl();
	}

	public void restartButtonOnClick()
	{
		Debug.Log("Restart Button Clicked");
        Restart();
	}

	public void mainMenuButtonOnClick()
	{
		Debug.Log("Main Menu Button Clicked");
	}

    // Update is called once per frame
    public void Update()
    {
        //uses the p button to pause and resume the game
     /*   if (Input.GetKeyDown(KeyCode.P))
        {
            
            //Game is Paused
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0; 
                ShowPaused();
            }
            //Game is resumed
            else if (Time.timeScale == 0)
            {
                Debug.Log("high");
                Time.timeScale = 1;
                HidePaused();
            }
        }*/
    }



    //Restarts the game
    public void Restart()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);

    }

    //controls the pausing of the scene
    public void PauseControl()
    {
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0;
            ShowPaused();
        }
        else if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            HidePaused();
        }
    }

    //shows objects with ShowOnPause tag
    public void ShowPaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
        }
    }

    //hides objects with ShowOnPause tag
    public void HidePaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
        }
    }
    /*
    //loads inputted level
    public void LoadLevel(string level)
    {
        Application.LoadLevel(level);
    }*/
}
