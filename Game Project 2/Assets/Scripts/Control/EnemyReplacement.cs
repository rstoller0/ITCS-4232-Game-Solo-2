using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyReplacement : MonoBehaviour
{
    [SerializeField] private GameObject dyingEnemy;
    [SerializeField] private GameObject[] ninjas;
    [SerializeField] private GameObject[] dragonWarriors;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject ninja in ninjas)
        {
            ninja.GetComponentInChildren<SpriteRenderer>().enabled = false;
            ninja.GetComponent<NinjaNavMesh>().enabled = false;
        }

        foreach (GameObject dragonWarrior in dragonWarriors)
        {
            dragonWarrior.GetComponentInChildren<SpriteRenderer>().enabled = false;
            dragonWarrior.GetComponent<DragonWarriorNavMesh>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (dyingEnemy == null)
        {
            foreach (GameObject ninja in ninjas)
            {
                ninja.GetComponentInChildren<SpriteRenderer>().enabled = true;
                ninja.GetComponent<NinjaNavMesh>().enabled = true;
            }

            foreach (GameObject dragonWarrior in dragonWarriors)
            {
                dragonWarrior.GetComponentInChildren<SpriteRenderer>().enabled = true;
                dragonWarrior.GetComponent<DragonWarriorNavMesh>().enabled = true;
            }

            Destroy(gameObject);
        }
    }
}
