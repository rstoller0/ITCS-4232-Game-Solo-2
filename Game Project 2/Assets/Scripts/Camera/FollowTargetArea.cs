using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetArea : MonoBehaviour
{
    [SerializeField] private Transform lookAt;
    [SerializeField] private float boundX = 1.5f;
    [SerializeField] private float boundZ = 1.5f;

    private void LateUpdate()
    {
        Vector3 delta = Vector3.zero;

        float dx = lookAt.position.x - transform.position.x;
        if(dx > boundX || dx < -boundX)
        {
            if(transform.position.x < lookAt.position.x)
            {
                delta.x = dx - boundX;
            }
            else
            {
                delta.x = dx + boundX;
            }
        }

        transform.position = transform.position + delta;
    }
}
