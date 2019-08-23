using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UpDownBoundries))]

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] float playerSpeed = 15;
    [SerializeField] float maxSpeed = 3;
    [SerializeField] float currentSpeed = 0;
    [Range(1, 10)] [SerializeField] float jumpForce;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2;
    [SerializeField] private IsGroundedCheck isGroundedCheck;
    private Animator anim;
    private SpriteRenderer sr;

    private void Start()
    {
        //get player's rigidbody at start
        rb = GetComponent<Rigidbody>();

        //get player's animator at start
        anim = GetComponent<Animator>();

        //get player's sprite renderer at start
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        //update current speed every frame
        currentSpeed = rb.velocity.magnitude;

        //update animator's speed variable to currentSpeed
        anim.SetFloat("speed", currentSpeed);

        //if moving right
        if(rb.velocity.x > 0)
        {
            //set x flip to false (face right)
            sr.flipX = false;
        }//else if moving left
        else if(rb.velocity.x < 0)
        {
            //set x flip to true (face left)
            sr.flipX = true;
        }

        //if player is not grounded
        if (!isGroundedCheck.isGrounded)
        {
            //set player's grounded animator variable to false
            anim.SetBool("isGrounded", false);

            //AND if the player is falling
            if (rb.velocity.y < 0)
            {
                //amplify the player's fall speed over time
                rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            } //else if the player is traveling upward AND no longer holding spacebar
            else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                //apply the lowJumpMultiplier to the players Y velocity
                rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }//else the player is grounded
        else
        {
            //set player's grounded animator variable to false
            anim.SetBool("isGrounded", true);

            //AND if the player presses spacebar (jump button)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //apply force for jump
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    private void FixedUpdate()
    {
        //horizontal in the X for right and left
        //vertical in the Z for up and down (less speed for up and down movement)
        Vector3 axisInputs = (new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));

        //if the player is not currently moving at max speed
        if(currentSpeed < maxSpeed)
        {
            //add force based on inputs
            rb.AddForce(axisInputs * playerSpeed * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    }
}
