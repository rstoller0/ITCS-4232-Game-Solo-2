using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private Health playerHealthScript;
    [SerializeField] private Image healthRemaining;
    [SerializeField] private Text healthNumber;

    // Update is called once per frame
    void Update()
    {
        //fill based on the amount of health remaining devided by the max health
        healthRemaining.fillMethod = Image.FillMethod.Horizontal;
        healthRemaining.fillAmount = playerHealthScript.GetHealth() / playerHealthScript.GetMaxHealth();
        healthNumber.text = "" + playerHealthScript.GetHealth();
    }
}