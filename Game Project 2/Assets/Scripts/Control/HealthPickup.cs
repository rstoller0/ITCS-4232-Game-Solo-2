using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Range(5, 250)] [SerializeField] private float healthAmount = 50;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Health healthScript = other.transform.GetComponent<Health>();

            //if player current health is lower than player max health
            if (healthScript.GetHealth() < healthScript.GetMaxHealth()) {
                other.transform.GetComponent<Health>().GainHealth(healthAmount);

                Destroy(gameObject);
            }
        }
    }
}
