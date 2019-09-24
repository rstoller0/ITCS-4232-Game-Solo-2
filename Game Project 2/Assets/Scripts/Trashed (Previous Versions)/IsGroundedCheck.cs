using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGroundedCheck : MonoBehaviour
{
    public bool isGrounded = true;

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        //if on ground (tag is ground)
        if(other.tag == "ground")
        {
            //set isGrounded to true
            isGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if leaving the ground (tag is ground)
        if(other.tag == "ground")
        {
            //set isGrounded to false
            isGrounded = false;
        }
    }
}
