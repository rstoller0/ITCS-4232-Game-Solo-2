using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float health;

    public float GetHealth()
    {
        return health;
    }

    public void DoDamage(float damage)
    {
        health = Mathf.Max(health - damage, 0f);
    }
}
