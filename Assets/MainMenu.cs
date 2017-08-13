
using UnityEngine;
using UnityEngine.SceneManagement;


//tutorial used: https://www.sitepoint.com/adding-pause-main-menu-and-game-over-screens-in-unity/

public class MainMenu : MonoBehaviour {
    /// <summary>
    /// ////////////////
    /// </summary>
    public GameObject[] pauseObjects;

    /// Pause Menu
    /// ////////////////////////////
    /// </summary>
   
   public void Start()
    {
        Time.timeScale = 1; //time is passing in real time. 0 means paused
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        HidePaused();
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
