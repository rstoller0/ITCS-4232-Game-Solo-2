﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]

public class DragonWarriorNavMesh : MonoBehaviour
{
    //player variable
    [SerializeField] Transform target;
    //ninja movement/location variables
    NavMeshAgent navMeshAgent;
    Rigidbody rb;

    //visual variables
    private Animator anim;
    private SpriteRenderer sr;
    private bool facingRight = true;

    //range/combat variables
    [Range(0, 7)] [SerializeField] private float detectionDistance;
    [SerializeField] private float upDownAttackRange = 0.3f;
    [SerializeField] private float attackRange = 0.4f;
    [SerializeField] private float attackDamage = 10;
    [SerializeField] private GameObject attackProjectile;
    [Tooltip("Time between attacks (Lower means faster attack speed)")]
    [Range(0, 5)] [SerializeField] private float timeBetweenAttacks = 3;
    private bool inAttackRange = false;
    private float attackCooldown = 0;
    private bool isAttacking = false;
    private bool isWaitingToAttack = false;
    private bool isStaggered = false;
    private Health healthScript;

    //variable for desired location for the ninja to move to
    private Vector3 targetVector;

    private void Start()
    {
        //get the nav mesh agent at start and store it in the variable
        navMeshAgent = GetComponent<NavMeshAgent>();
        //stop the enemy from rotating as they move on the navmesh
        navMeshAgent.updateRotation = false;
        //randomly increase/decrease the enemies movement slightly, to add variety to the enemy type
        navMeshAgent.speed += Random.Range(-0.25f, 0.25f);
        rb = GetComponent<Rigidbody>();

        //get dragonWarrior's animator at start
        anim = GetComponent<Animator>();

        //get dragonWarrior's sprite renderer at start
        sr = GetComponentInChildren<SpriteRenderer>();

        //get dragonWarrior's health script at start
        healthScript = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        //to limit attack speed and allow movement after attack animation finishes but not allow to attack yet
        if(attackCooldown > 0)
        {
            //if the attack cooldown is still above 0 count down
            attackCooldown = Mathf.Max(attackCooldown - Time.deltaTime, 0);
        }

        //if health is not 0 (dragonWarrior is not dead), then allow movement and control to the AI
        if (healthScript.GetHealth() > 0)
        {
            //if not staggered then allow all movement and combat
            if (!isStaggered)
            {
                //always update animator variable
                UpdateAnimator();

                //movement code
                #region
                //get destination location
                targetVector = target.position;

                //log current distance
                //Debug.Log(Vector3.Distance(playerTransform.position, transform.position));

                //if the player is within the skeletons detection distance
                if (Vector3.Distance(target.position, transform.position) < detectionDistance && !isAttacking)
                {
                    //if ninja is within attack range on the X axis AND is not waiting to attack again
                    if (Mathf.Abs(target.position.x - transform.position.x) < attackRange)
                    {
                        //remove X axis inputs
                        targetVector = new Vector3(transform.position.x, targetVector.y, targetVector.z);

                    }

                    StartMoveAction(targetVector);
                }
                else
                {
                    Cancel();
                }
                #endregion

                //Debug.Log(navMeshAgent.velocity.x);

                //sprite direction determination
                #region
                //if moving right OR if the player is to the right of the ninja
                if (navMeshAgent.velocity.x > 0 || target.position.x - transform.position.x > 0)
                {
                    //set x flip to false (face right)
                    sr.flipX = false;
                    facingRight = true;
                }//else if moving left OR if the player is to the left of the ninja
                else if (navMeshAgent.velocity.x < 0 || target.position.x - transform.position.x < 0)
                {
                    //set x flip to true (face left)
                    sr.flipX = true;
                    facingRight = false;
                }
                #endregion

                //dragonWarrior combat code
                #region
                //if ninja is within attack range on the X axis AND within 0.3f on the Z axis
                if (Mathf.Abs(target.position.x - transform.position.x) < attackRange && Mathf.Abs(target.position.z - transform.position.z) < upDownAttackRange)
                {
                    //set in attack range to true
                    inAttackRange = true;
                }
                else
                { //else the ninja is not close enough to player OR not aligned with player to attack
                  //set in attack range to true
                    inAttackRange = false;
                }

                //if ninja is in attacking range AND is able to attack again (cooldown is at 0)
                if (inAttackRange && attackCooldown == 0)
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
        { //else the dragonWarrior is dead

            //OPTION 1
            //rb.useGravity = false;
            //GetComponent<Collider>().enabled = false;
            //anim.enabled = false;

            //OPTION 2
            Destroy(this.gameObject);
        }
    }

    //movement functions (update animator speed function)
    #region
    private void UpdateAnimator()
    {
        //set velocity to global velocity
        //Vector3 velocity = navMeshAgent.velocity;
        //set localVelocity to local velocity from the global velocity
        //Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        //set speed to the forward movement of local velocity
        float speed = navMeshAgent.velocity.magnitude;
        //set animator's variable to current speed
        anim.SetFloat("speed", speed);
    }

    public void StartMoveAction(Vector3 destination)
    {
        //call action scheduler for cancelling action if neccessary
        //GetComponent<ActionScheduler>().StartAction(this);
        //call move to function (this is from a movement only action [No Combat])
        MoveTo(destination);
    }

    public void MoveTo(Vector3 destination)
    {
        //set destiantion for the NavMeshAgent (location to move to)
        navMeshAgent.destination = destination;
        //allow the NavMeshAgent to move
        navMeshAgent.isStopped = false;
    }

    public void Cancel()
    {
        //stop the NavMeshAgent
        navMeshAgent.isStopped = true;
    }
    #endregion

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
        //bit shift the index of the Player layer (10)
        int layerMask1 = 1 << 10;
        //int layerMask2 = 1 << 12;

        //this would cast rays only against colliders in layer 8 or against layer 9.
        //but instead we want to collide against everything except layer 8 and layer 9. The ~ operator does this, it inverts a bitmasks.
        //int layerMask3 = ~(layerMask1 | layerMask2);

        //set up variables for raycast detection of hitting enemies
        //RaycastHit hit;
        Vector3 rayDir;
        //Vector3 rayDir2; //IF I WANT TO ADD MORE HIT WIDTH
        //Vector3 rayDir3; //IF I WANT TO ADD MORE HIT WIDTH
        RaycastHit[] hits;
        //RaycastHit[] hits2; //IF I WANT TO ADD MORE HIT WIDTH
        //RaycastHit[] hits3; //IF I WANT TO ADD MORE HIT WIDTH

        //determine what direction the payer is facing to attack that direction
        //if facing right
        if (facingRight)
        {
            //set attack direction to right
            rayDir = transform.right;
            //rayDir2 = transform.right + new Vector3(0, 0, 0.5f); //IF I WANT TO ADD MORE HIT WIDTH
            //rayDir3 = transform.right + new Vector3(0, 0, -0.5f); //IF I WANT TO ADD MORE HIT WIDTH
        }
        else
        { //else the player is facing left
            //set attack direction to left
            rayDir = -transform.right;
            //rayDir2 = -transform.right + new Vector3(0, 0, 0.5f); //IF I WANT TO ADD MORE HIT WIDTH
            //rayDir3 = -transform.right + new Vector3(0, 0, -0.5f); //IF I WANT TO ADD MORE HIT WIDTH
        }

        hits = Physics.RaycastAll(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), rayDir, attackRange, layerMask1);
        //hits2 = Physics.RaycastAll(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), rayDir2, attackRange, layerMask1); //IF I WANT TO ADD MORE HIT WIDTH
        //hits3 = Physics.RaycastAll(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), rayDir3, attackRange, layerMask1); //IF I WANT TO ADD MORE HIT WIDTH

        //can hit multiple players at once
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.tag == "Player")
            {
                //do damage to that enemy
                hit.transform.GetComponent<Health>().DoDamage(attackDamage);

                //WORK ON THIS ASPECT, MAY NEED TO ADD A ENEMY PARENT SCRIPT THAT HAS THE STAGGER VARIABLES SO CAN BE ON ALL ENEMY TYPES AND NEED TO ADD ANIMATION STUFF FOR STAGGERS
                //ASLO HAVE NOT ADD A STAGGER ASPECT TO THE ENEMIES
                hit.transform.GetComponent<PlayerController>().Stagger();
                //Debug.DrawRay(transform.position, rayDir * 0.4f, Color.red, 50000);
                //Debug.DrawRay(transform.position, rayDir2 * 0.4f, Color.green, 50000); //IF I WANT TO ADD MORE HIT WIDTH??
                //Debug.DrawRay(transform.position, rayDir3 * 0.4f, Color.blue, 50000); //IF I WANT TO ADD MORE HIT WIDTH??
            }
        }
        #endregion
    }

    //firing the ranged attack
    private void FireProjectile()
    {
        //determine what direction the payer is facing to attack that direction
        //if facing right
        if (facingRight)
        {
            GameObject fireball = Instantiate(attackProjectile, transform.position + ((transform.up + transform.right) * 0.12f), Quaternion.LookRotation(transform.forward)) as GameObject;
            fireball.transform.Rotate(30, 0, 0);
        }
        else
        { //else the player is facing left
            GameObject fireball = Instantiate(attackProjectile, transform.position + ((transform.up - transform.right) * 0.12f), Quaternion.LookRotation(-transform.forward)) as GameObject;
            fireball.transform.Rotate(-30, 0, 0);
        }
    }

    //setting isAttacking to false
    private void StopAttack()
    {
        //set is attacking to false (this allows the player to move after the attack animation but before the player can attack again)
        isAttacking = false;
    }
    #endregion
}