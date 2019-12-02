using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] ninjas;
    [SerializeField] private GameObject[] dragonWarriors;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject ninja in ninjas)
        {
            ninja.GetComponentInChildren<SpriteRenderer>().enabled = false;
            ninja.GetComponent<NinjaNavMesh>().enabled = false;
            ninja.GetComponent<Collider>().enabled = false;
        }

        foreach (GameObject dragonWarrior in dragonWarriors)
        {
            dragonWarrior.GetComponentInChildren<SpriteRenderer>().enabled = false;
            dragonWarrior.GetComponent<DragonWarriorNavMesh>().enabled = false;
            dragonWarrior.GetComponent<Collider>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (GameObject ninja in ninjas)
            {
                ninja.GetComponentInChildren<SpriteRenderer>().enabled = true;
                ninja.GetComponent<NinjaNavMesh>().enabled = true;
                ninja.GetComponent<Collider>().enabled = true;
            }

            foreach (GameObject dragonWarrior in dragonWarriors)
            {
                dragonWarrior.GetComponentInChildren<SpriteRenderer>().enabled = true;
                dragonWarrior.GetComponent<DragonWarriorNavMesh>().enabled = true;
                dragonWarrior.GetComponent<Collider>().enabled = true;
            }

            Destroy(gameObject);
        }
    }
}
