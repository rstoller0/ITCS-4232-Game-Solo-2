using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UpDownBoundries))]
[RequireComponent(typeof(Health))]

public class PlayerController : MonoBehaviour
{
    [SerializeField] private string currentScene;

    private Rigidbody rb;

    //range/combat variables
    [SerializeField] private float attackRange = 0.4f;
    [SerializeField] private float attackDamage = 10;
    private bool isAttacking = false;
    private bool isWaitingToAttack = false;
    private Health healthScript;

    //movement variables
    [Range(0, 5)] [SerializeField] float playerSpeed = 2;
    //[SerializeField] float maxSpeed = 3; //(IF STATEMENT NOT NEEDED???) [movement code]
    [SerializeField] float currentSpeed = 0;
    [Range(0, 10)] [SerializeField] float jumpForce = 5;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2;
    [SerializeField] private IsGroundedCheck isGroundedCheck;

    //visiual variables
    private Animator anim;
    private SpriteRenderer sr;
    private bool facingRight = true;

    //variable for movement inputs
    Vector3 axisInputs;

    //time counter variable
    private float stuckInAttackTimer = 0;

    private void Start()
    {
        //get player's rigidbody at start
        rb = GetComponent<Rigidbody>();

        //get player's animator at start
        anim = GetComponent<Animator>();

        //get player's sprite renderer at start
        sr = GetComponentInChildren<SpriteRenderer>();

        //get skeleton's health script at start
        healthScript = GetComponent<Health>();
    }

    private void Update()
    {
        //This will need to be adjusted for interrupting attacks... (i.e. if the enemy staggers the player in their attack animation, they would be stuck in isAttacking/isWaitingToAttack forever
        //if the player is attacking OR waiting to attack
        if (isAttacking || isWaitingToAttack)
        {
            //increment stuckInAttackTimer by the time that passes
            stuckInAttackTimer += Time.deltaTime;

            //if they are stuck in these states for more that (2) seconds
            if (stuckInAttackTimer > 2)
            {
                //reset stuckInAttackTimer
                stuckInAttackTimer = 0;
                //set isAttacking to false to get out of that state
                isAttacking = false;
                //set isWaitingToAttack to false to get out of that state
                isWaitingToAttack = false;
            }
        }

        //if health is not 0 (player is not dead), then allow movement and control of character
        if (healthScript.GetHealth() > 0)
        {
            //movement code
            #region
            //FOR SOME REASON SOMTIMES WITH YOU PRESS HORIZONTAL AND VERTICAL KEYS AT THE SAME TIME, PLAYER DOES NOT MOVE
            //horizontal in the X for right and left
            //vertical in the Z for up and down (less speed for up and down movement)
            axisInputs = (new Vector3(Input.GetAxis("Horizontal"), rb.velocity.y, Input.GetAxis("Vertical"))).normalized;

            //if the player is not currently moving at max speed (IF STATEMENT NOT NEEDED ???) [max speed variable]
            //if (currentSpeed < maxSpeed)
            //{
            if (!isAttacking)
            {
                //move player on X and Z axis based on inputs and keep the movement on Y axis as is
                rb.velocity = new Vector3(axisInputs.x * playerSpeed, rb.velocity.y, axisInputs.z * playerSpeed);
            }
            //}
            #endregion

            //update current speed every frame
            currentSpeed = rb.velocity.magnitude;

            //update animator's speed variable to currentSpeed
            anim.SetFloat("speed", currentSpeed);

            //sprite direction determination
            #region
            //if moving right
            if (rb.velocity.x > 0)
            {
                //set x flip to false (face right)
                sr.flipX = false;
                facingRight = true;
            }//else if moving left
            else if (rb.velocity.x < 0)
            {
                //set x flip to true (face left)
                sr.flipX = true;
                facingRight = false;
            }
            #endregion

            //combat code
            #region
            //if (left mouse button OR F key is pressed) AND player is not currently waiting to attack AND player is grounded
            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)) && !isWaitingToAttack && isGroundedCheck.isGrounded)
            {
                //set attack trigger in animator to true
                anim.SetTrigger("attack");
                //set is attacking to true
                isAttacking = true;
                //set is waiting to attack to true
                isWaitingToAttack = true;
            }
            #endregion

            //jump code
            #region
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

                //AND if the player presses spacebar (jump button) AND player is not attacking
                if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
                {
                    //apply force for jump
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                }
            }
            #endregion
        }
        else
        { //else the player is dead
            //NEED TO CHANGE THIS [BUT FOR NOW IF YOU DIE IT JUST RELOADS THE LEVEL]
            SceneManager.LoadScene(currentScene);
        }
    }

    private void FixedUpdate()
    {
        //ORIGINAL MOVEMENT SORT OF WONKY
        #region
        /*
        //if the player is not currently moving at max speed
        if(currentSpeed < maxSpeed)
        {
            //add force based on inputs
            //rb.AddForce(axisInputs * playerSpeed * Time.fixedDeltaTime, ForceMode.Impulse);
        }
        */
        #endregion
    }

    //attack animation events
    #region
    //raycasting for damage dealing
    private void Hit()
    {
        #region
        //bit shift the index of the Enemy layer (10)
        int layerMask1 = 1 << 10;
        //int layerMask2 = 1 << 9;

        //this would cast rays only against colliders in layer 8 or against layer 9.
        //but instead we want to collide against everything except layer 8 and layer 9. The ~ operator does this, it inverts a bitmasks.
        //int layerMask3 = ~(layerMask1 | layerMask2);

        //set up variables for raycast detection of hitting enemies
        RaycastHit hit;
        Vector3 rayDir;

        //determine what direction the payer is facing to attack that direction
        //if facing right
        if (facingRight)
        {
            //set attack direction to right
            rayDir = transform.right;
        }
        else
        { //else the player is facing left
            //set attack direction to left
            rayDir = -transform.right;
        }

        //if raycast hits an emeny
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), rayDir, out hit, attackRange, layerMask1))
        {
            //do damage to that enemy
            hit.transform.GetComponent<Health>().DoDamage(attackDamage);
            //Debug.DrawRay(transform.position, rayDir * 0.4f, Color.red, 50000);
        }
        #endregion
    }

    //setting isWaitingToAttacking to false after attack cooldown time to allow for new attack [For Agent_Attack, attackCooldown is set to 0.5f]
    private void StoppingAttack(float attackCooldown)
    {
        //set is attacking to false, enemy is not attacking anymore, but needs to wait to attack again
        isAttacking = false;
        //attackCooldown is set within the animation (Agent_Attack animation)
        Invoke("AttackStopped", attackCooldown);
    }

    private void AttackStopped()
    {
        //set isWaitingToAttacking to false to allow for another attack
        isWaitingToAttack = false;
    }
    #endregion
}
