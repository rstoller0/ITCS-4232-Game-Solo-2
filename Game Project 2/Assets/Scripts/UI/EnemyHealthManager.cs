using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthManager : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Health enemyHealthScript;
    [SerializeField] private GameObject healthUIGroup;
    [SerializeField] private Image healthRemaining;

    void Start()
    {
        healthUIGroup.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //have the healthbar track the enemy's location
        transform.position = target.position;

        if (enemyHealthScript.GetHealth() < enemyHealthScript.GetMaxHealth())
        {
            healthUIGroup.SetActive(true);
        }

        //fill based on the amount of health remaining devided by the max health
        healthRemaining.fillMethod = Image.FillMethod.Horizontal;
        healthRemaining.fillAmount = enemyHealthScript.GetHealth() / enemyHealthScript.GetMaxHealth();
    }
}