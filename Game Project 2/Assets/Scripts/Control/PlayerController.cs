using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UpDownBoundries))]
[RequireComponent(typeof(Health))]

public class PlayerController : MonoBehaviour
{
    [SerializeField] private string currentScene;

    //movement variable
    private Rigidbody rb;

    //range/combat variables
    [SerializeField] private float attackRange = 0.4f;
    [SerializeField] private float attackDamage = 10;
    [Tooltip("Time between attacks (Lower means faster attack speed)")]
    [Range(0, 5)] [SerializeField] private float timeBetweenAttacks = 1;
    private float attackCooldown = 0;
    private bool isAttacking = false;
    private bool isWaitingToAttack = false;
    private bool isStaggered = false;
    private Health healthScript;

    //movement variables
    [Range(0, 5)] [SerializeField] float playerSpeed = 2;
    //[SerializeField] float maxSpeed = 3; //(IF STATEMENT NOT NEEDED???) [movement code]
    [SerializeField] float currentSpeed = 0;

    //visiual variables
    private Animator anim;
    private SpriteRenderer sr;
    private bool facingRight = true;

    //variable for movement inputs
    Vector3 axisInputs;

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
        //to limit attack speed and allow movement after attack animation finishes but not allow to attack yet
        if (attackCooldown > 0)
        {
            //if the attack cooldown is still above 0 count down
            attackCooldown = Mathf.Max(attackCooldown - Time.deltaTime, 0);
        }

        //if health is not 0 (player is not dead), then allow movement and control of character
        if (healthScript.GetHealth() > 0)
        {
            //if not staggered then allow all movement and combat
            if (!isStaggered)
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
                //if (left mouse button OR Spacebar key is pressed) AND player is able to attack again (cooldown is at 0)
                if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && attackCooldown == 0)
                {
                    //set is attacking to true
                    isAttacking = true;
                    //set attack cooldown
                    attackCooldown = timeBetweenAttacks;
                    //set attack trigger in animator to true
                    anim.SetTrigger("attack");
                }
                    #endregion
            }
        }
        else
        { //else the player is dead
            //NEED TO CHANGE THIS [BUT FOR NOW IF YOU DIE IT JUST RELOADS THE LEVEL]
            SceneManager.LoadScene(currentScene);
        }
    }

    //stagger functions
    #region
    public void Stagger()
    {
        //set is staggered to true and trigger the animation
        isStaggered = true;
        anim.SetTrigger("stagger");
    }

    public void StopStagger()
    {
        //set is staggered to false
        isStaggered = false;

        //safegaurds to allow for movement and attacking shortly after being staggered...
        //this allows the player to move after being staggered during the attack animation
        //and sets the attack cooldown to a short duration [it might have been set to a high number]...
        //this ensure that the player is not unable to attck for a long time after coming out of a stagger...
        isAttacking = false;
        attackCooldown = 0.25f;
    }
    #endregion

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
        //RaycastHit hit;
        Vector3 rayDir;
        RaycastHit[] hits;

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

        hits = Physics.RaycastAll(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), rayDir, attackRange, layerMask1);

        //can hit multiple enemies at once
        foreach (RaycastHit hit in hits)
        {
            //do damage to that enemy
            hit.transform.GetComponent<Health>().DoDamage(attackDamage);

            //WORK ON THIS ASPECT, MAY NEED TO ADD A ENEMY PARENT SCRIPT THAT HAS THE STAGGER VARIABLES SO CAN BE ON ALL ENEMY TYPES AND NEED TO ADD ANIMATION STUFF FOR STAGGERS
            //ASLO HAVE NOT ADD A STAGGER ASPECT TO THE ENEMIES
            hit.transform.GetComponent<SkeletonNavMesh>().Stagger();
            //Debug.DrawRay(transform.position, rayDir * 0.4f, Color.red, 50000);
        }
        #endregion
    }

    //setting isAttacking to false
    private void StopAttack()
    {
        //set is attacking to false (this allows the player to move after the attack animation but before the player can attack again)
        isAttacking = false;
    }
    #endregion
}
