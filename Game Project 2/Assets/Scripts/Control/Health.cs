using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;

    void Start()
    {
        health = maxHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealth()
    {
        return health;
    }

    public void DoDamage(float damage)
    {
        health = Mathf.Max(health - damage, 0f);
    }
}
