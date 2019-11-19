using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyReplacement : MonoBehaviour
{
    [SerializeField] private GameObject dyingEnemy;
    [SerializeField] private GameObject[] gameObjects;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (dyingEnemy == null)
        {
            foreach (GameObject go in gameObjects)
            {
                go.SetActive(true);
            }

            Destroy(gameObject);
        }
    }
}
