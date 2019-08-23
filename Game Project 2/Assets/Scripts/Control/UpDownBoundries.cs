using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDownBoundries : MonoBehaviour
{
    //variable to hold stop points of bottom and top
    [SerializeField] private float lowerBound = 0;
    [SerializeField] private float upperBound = 2;

    // Update is called once per frame
    void Update()
    {
        //get player's position
        Vector3 boundPos = transform.position;
        //clamp the bound position within the bounds
        boundPos.z = Mathf.Clamp(boundPos.z, lowerBound, upperBound);
        //reset player's position to the bound position
        transform.position = boundPos;
    }
}
