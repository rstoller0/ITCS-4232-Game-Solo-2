using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UpDownBoundries))]
[RequireComponent(typeof(Health))]

public class SkeletonController : MonoBehaviour
{
    //target variables
    [SerializeField] Transform playerTransform;

    //range/combat variables
    [Range(0, 7)] [SerializeField] private float detectionDistance;
    [SerializeField] private float attackRange = 0.4f;
    [SerializeField] private float attackDamage = 10;
    private bool inAttackRange = false;
    private bool isAttacking = false;
    private bool isWaitingToAttack = false;
    private bool isStaggered = false;
    private float staggerTimer = 0;
    private Health healthScript;

    //movement variables
    private Rigidbody rb;
    [Range(0, 5)] [SerializeField] private float skeletonSpeed = 1.5f;
    //[SerializeField] float maxSpeed = 3; //(IF STATEMENT NOT NEEDED???) [movement code]
    [SerializeField] private float currentSpeed = 0;
    [Range(1, 10)] [SerializeField] private float jumpForce = 5; //jump code (ninja code)
    private float currentJumpCooldown = 0; //jump code (ninja code)
    private float jumpCooldown = 1; //jump code (ninja code)
    [SerializeField] private float fallMultiplier = 2.5f; //jump code (player code)
    [SerializeField] private float lowJumpMultiplier = 2; //jump code (player code)
    [SerializeField] private IsGroundedCheck isGroundedCheck; //jump code (ninja code)

    //visual variables
    private Animator anim;
    private SpriteRenderer sr;
    private bool facingRight = true;

    //variable for movement inputs
    Vector3 axisInputs;

    //time counter variables
    private float stuckInAttackTimer = 0;

    private void Start()
    {
        //get ninja's rigidbody at start
        rb = GetComponent<Rigidbody>();

        //get ninja's animator at start
        anim = GetComponent<Animator>();

        //get ninja's sprite renderer at start
        sr = GetComponentInChildren<SpriteRenderer>();

        //get ninja's health script at start
        healthScript = GetComponent<Health>();
    }

    private void Update()
    {
        //This will need to be adjusted for interrupting attacks... (i.e. if the player staggers the enemy in their attack animation, they would be stuck in isAttacking/isWaitingToAttack forever
        //if the ninja is attacking OR waiting to attack
        if (isAttacking || isWaitingToAttack)
        {
            //increment stuckInAttackTimer by the time that passes
            stuckInAttackTimer += Time.deltaTime;

            //if they are stuck in these states for more that (3) seconds
            if (stuckInAttackTimer > 3)
            {
                //reset stuckInAttackTimer
                stuckInAttackTimer = 0;
                //set isAttacking to false to get out of that state
                isAttacking = false;
                //set isWaitingToAttack to false to get out of that state
                isWaitingToAttack = false;
            }
        }

        //if health is not 0 (ninja is not dead), then allow movement and control to the AI
        if (healthScript.GetHealth() > 0)
        {
            //if not staggered then allow all movement and combat
            if (!isStaggered)
            {
                //movement code
                #region
                //get the direction the ninja needs to move to get to the player
                //set target Y axis of target position to 0 (so no flying)
                axisInputs = (playerTransform.position - transform.position).normalized;

                //log current distance
                //Debug.Log(Vector3.Distance(playerTransform.position, transform.position));

                //if the player is within the skeletons detection distance
                if (Vector3.Distance(playerTransform.position, transform.position) < detectionDistance && !isAttacking)
                {
                    //if ninja is within attack range on the X axis AND is not waiting to attack again
                    if (Mathf.Abs(playerTransform.position.x - transform.position.x) < attackRange /*&& !isWaitingToAttack*/)
                    {
                        //remove X axis inputs
                        axisInputs = new Vector3(0, axisInputs.y, axisInputs.z);

                    }//else if the ninja is waiting to attack
                    else if (isWaitingToAttack)
                    {
                        //run away from player at 75% speed
                        axisInputs = new Vector3(-axisInputs.x * 0.75f, axisInputs.y, -axisInputs.z * 0.75f);
                    }

                    //if the ninja is not currently moving at max speed (IF STATEMENT NOT NEEDED???) [max speed variable]
                    //if (currentSpeed < maxSpeed)
                    //{
                    //move ninja on X and Z axis based on inputs and keep the movement on Y axis as is
                    //Debug.Log(axisInputs);
                    rb.velocity = new Vector3(axisInputs.x * skeletonSpeed, rb.velocity.y, axisInputs.z * skeletonSpeed);
                    //}
                }
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

                //ninja combat code
                #region
                //if ninja is within attack range on the X axis AND within 0.1f on the Z axis
                if (Mathf.Abs(playerTransform.position.x - transform.position.x) < attackRange && Mathf.Abs(playerTransform.position.z - transform.position.z) < 0.1f)
                {
                    //set in attack range to true
                    inAttackRange = true;
                }
                else
                { //else the ninja is not close enough to player OR not aligned with player to attack
                  //set in attack range to true
                    inAttackRange = false;
                }

                //if ninja is in attacking range AND is not currently waiting to attack AND ninja is grounded 
                if (inAttackRange && !isWaitingToAttack && isGroundedCheck.isGrounded)
                {
                    //set is attacking to true
                    isAttacking = true;
                    //set is waiting to attack to true
                    isWaitingToAttack = true;
                    //set attack trigger in animator to true
                    anim.SetTrigger("attack");
                }
                #endregion

                //jump code (ninja code) [REMOVE?] (anim set is required without jump code, but will remove jump animation if removing the code)
                anim.SetBool("isGrounded", true);
                #region
                /*
                //if jump is on cooldown
                if (currentJumpCooldown > 0)
                {
                    //pick the bigger of cooldown left - time AND 0
                    currentJumpCooldown = Mathf.Max(currentJumpCooldown - Time.deltaTime, 0);
                }

                //if ninja is not grounded
                if (!isGroundedCheck.isGrounded)
                {
                    //set ninja's grounded animator variable to false
                    anim.SetBool("isGrounded", false);

                    //AND if the ninja is falling
                    if (rb.velocity.y < 0)
                    {
                        //amplify the ninja's fall speed over time
                        rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
                    } //else if the ninja is traveling upward AND no longer holding spacebar
                    else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
                    {
                        //apply the lowJumpMultiplier to the skeletons Y velocity
                        rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
                    }
                }//else the ninja is grounded
                else
                {
                    //set ninja's grounded animator variable to false
                    anim.SetBool("isGrounded", true);

                    //AND if the ninja is within attack range of player AND the player is above the ninja AND jump is off cooldown AND ninja is not attacking
                    //THIS IS JUST MIMICING PRETTY MUCH [THIS WHOLE SCRIPT IS JUST MIMICING PRETTY MUCH]
                    if (inAttackRange && (playerTransform.position.y - transform.position.y) > 0.5f && currentJumpCooldown == 0 && !isAttacking)
                    {
                        //apply force for jump
                        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                        //set jump to be on cooldown (this is to stop enemy from applying this force more than once each jump)
                        currentJumpCooldown = jumpCooldown;
                    }
                }
                */
                #endregion
            }
            else
            {//is staggered
                //if staggerTimer is still active
                if (staggerTimer > 0)
                {
                    //lower timer
                    staggerTimer = Mathf.Max(staggerTimer - Time.deltaTime, 0);
                }
                else
                {//else not staggered anymore
                    //set isStaggered to false
                    isStaggered = false;
                }
            }
        }
        else
        { //else the ninja is dead

            //OPTION 1
            //rb.useGravity = false;
            //GetComponent<Collider>().enabled = false;
            //anim.enabled = false;

            //OPTION 2
            Destroy(this.gameObject);
        }
    }

    public void Stagger(float timeToStagger)
    {
        isStaggered = true;
        staggerTimer = timeToStagger;
    }

    private void FixedUpdate()
    {
        //ORIGINAL MOVEMENT KIND OF WONKY
        #region
        /*
        //log current distance
        //Debug.Log(Vector3.Distance(playerTransform.position, transform.position));

        //if the player is within the skeletons detection distance
        if (Vector3.Distance(playerTransform.position, transform.position) < detectionDistance)
        {
            //if ninja is within attack range on the X axis
            if (Mathf.Abs(playerTransform.position.x - transform.position.x) < attackRange)
            {
                //remove X axis inputs
                axisInputs = new Vector3(0, axisInputs.y, axisInputs.z);
                //add force based on inputs
                rb.AddForce(axisInputs * skeletonSpeed * Time.fixedDeltaTime, ForceMode.Impulse);

            }//else not within attack range on the X axis AND if the ninja is not currently moving at max speed
            else if (currentSpeed < maxSpeed)
            {
                //add force based on inputs
                rb.AddForce(axisInputs * skeletonSpeed * Time.fixedDeltaTime, ForceMode.Impulse);
            }
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
        //bit shift the index of the Player layer (10)
        int layerMask1 = 1 << 9;
        //int layerMask2 = 1 << 10;

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

        //can hit multiple players at once
        foreach (RaycastHit hit in hits)
        {
            //do damage to that enemy
            hit.transform.GetComponent<Health>().DoDamage(attackDamage);
            //Debug.DrawRay(transform.position, rayDir * 0.4f, Color.red, 50000);
        }

        /*
        //if raycast hits an emeny
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), rayDir, out hit, attackRange, layerMask1))
        {
            //do damage to that enemy
            hit.transform.GetComponent<Health>().DoDamage(attackDamage);
            //Debug.DrawRay(transform.position, rayDir * 0.4f, Color.red, 50000);
        }
        */
        #endregion
    }

    //setting isWaitingToAttacking to false after attack cooldown time to allow for new attack [For Skeleton_Attack, attackCooldown is set to 2f]
    private void StoppingAttack(float attackCooldown)
    {
        //set is attacking to false, enemy is not attacking anymore, but needs to wait to attack again
        isAttacking = false;
        //attackCooldown is set within the animation (Skeleton_Attack animation)
        Invoke("AttackStopped", attackCooldown);
    }

    private void AttackStopped()
    {
        //set isWaitingToAttacking to false to allow for another attack
        isWaitingToAttack = false;
    }
    #endregion
}
