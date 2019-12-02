using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    void Update()
    {
        //if (Input.GetKey(KeyCode.Space))
        //{
            //if holding space down, scroll faster
            //transform.position += new Vector3(0, 5 * Time.deltaTime, 0);
        //}
        //else
        //{
            //move credits upwards
            //transform.position += new Vector3(0, 1 * Time.deltaTime, 0);
        //}
    }

    public void QuitToMenu()
    {
        MenuUIManager menuUIManager = GameObject.Find("Menu_Canvas").GetComponent<MenuUIManager>();

        menuUIManager.fromCredits = true;

        //load the main menu
        SceneManager.LoadScene("MainMenu");
    }
}
