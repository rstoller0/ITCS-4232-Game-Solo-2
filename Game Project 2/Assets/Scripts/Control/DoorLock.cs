using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLock : MonoBehaviour
{
    private int numEnemies;
    private int deadEnemies;
    [SerializeField] private GameObject[] gameObjects;

    // Start is called before the first frame update
    void Start()
    {
        numEnemies = gameObjects.Length;
    }

    // Update is called once per frame
    void Update()
    {
        if (deadEnemies != numEnemies)
        {
            deadEnemies = 0;

            foreach (GameObject go in gameObjects)
            {
                if (go == null)
                {
                    deadEnemies++;
                }
            }
        }
        else
        {
            gameObject.transform.position += new Vector3(0, -0.5f*Time.deltaTime, 0);

            if (gameObject.transform.position.y < -2)
            {
                Destroy(gameObject);
            }
        }
    }
}
