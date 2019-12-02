using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    [SerializeField] private float timeTilDestroy = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timeTilDestroy > 0)
        {
            timeTilDestroy = Mathf.Max(timeTilDestroy - Time.deltaTime, 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
