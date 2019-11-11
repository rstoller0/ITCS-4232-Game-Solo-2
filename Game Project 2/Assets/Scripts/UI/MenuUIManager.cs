using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager instance;
    private Canvas menu;
    [SerializeField] private bool setMenuAsEnabled = true;
    [SerializeField] private string levelToLoad = "Level1";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        menu = GetComponent<Canvas>();
        menu.enabled = setMenuAsEnabled;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !menu.enabled && SceneManager.GetActiveScene().name != "MainMenu")
        {
            //set the pause menu to enabled
            menu.enabled = true;
            //set GameManager's isPaused variable to true
            GameManager.instance.isPaused = true;
            //call change pause state
            ChangePauseState();
        }
    }

    public void ChangePauseState()
    {
        //if Time.timeScale == 0 set it to 1 otherwise set it to 0
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }

    public void Quit()
    {
        //if the current scene is the main menu
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            //quit the game
            Application.Quit();
        }
        else
        {//else we are on another scene and the game must be paused
            //so unpause and load the main menu
            ChangePauseState();
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void Play()
    {
        //hide the pause menu (regardless of if you click play from the main menu or the pause menu
        menu.enabled = false;
        //set GameManager's isPaused variable to false
        GameManager.instance.isPaused = false;

        //if the current scene is the main menu
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            //load level 1 (ONLY HAVE SANDBOX FOR NOW)
            SceneManager.LoadScene(levelToLoad);
        }
        else
        {//else we are on another scene and the game must be paused
            //so unpause
            //call change pause state
            ChangePauseState();
        }
    }

    public void ShowUI(GameObject ui)
    {
        ui.SetActive(true);
    }

    public void HideUI(GameObject ui)
    {
        ui.SetActive(false);
    }
}
