using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]

public class Boss1_4 : MonoBehaviour
{
    //Probably need to set up x distance and z distance in Animator for better attack timing...

    //player variable
    [SerializeField] Transform target;
    [SerializeField] SpriteRenderer[] sprites;
    private bool isAggro = false;
    //ninja movement/location variables
    NavMeshAgent navMeshAgent;
    Rigidbody rb;

    //visual variables
    [SerializeField] private GameObject flipperObject;
    [SerializeField] private Animator anim;
    private bool facingRight = true;

    //range/combat variables
    [Range(0, 7)] [SerializeField] private float detectionDistance;
    [SerializeField] private bool isAttacking = false;
    private float timeBetweenAttacks = 2;
    private float attackCooldown = 0;

    private bool playerInDamageRange = false;

    private int currentPhase = 0;
    [SerializeField] private float daggerPoundDamage = 50;
    //dagger throw is 40 (as of now)
    [SerializeField] private float daggerSwipeDamage = 30;
    [SerializeField] GameObject daggerProjectile;///CREATE THIS AND ADD IT HERE!!!!!!!!!!!!
    [Range(0, 5)] [SerializeField] private float daggerThrowSpeed = 1;
    private Health healthScript;
    [SerializeField] private GameObject healthCanvas;
    [SerializeField] private GameObject destroyOnDeath;

    //stagger time variable to be changed by attackers weapon stats
    private float staggerTime = 0.25f;

    //variable for desired location for the ninja to move to
    private Vector3 targetVector;

    //audio variables
    [SerializeField] private AudioSource footstepsAudioSource;
    [SerializeField] private AudioClip daggerSwipeClip;
    [SerializeField] private AudioClip daggerPoundClip;
    [SerializeField] private AudioSource hitAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        //get the nav mesh agent at start and store it in the variable
        navMeshAgent = GetComponent<NavMeshAgent>();
        //stop the enemy from rotating as they move on the navmesh
        navMeshAgent.updateRotation = false;
        rb = GetComponent<Rigidbody>();

        //get dragonWarrior's animator at start
        //anim = GetComponent<Animator>();
        anim.speed = 1;

        //get dragonWarrior's health script at start
        healthScript = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        //to limit attack speed and allow movement after attack animation finishes but not allow to attack yet
        if (attackCooldown > 0)
        {
            //if the attack cooldown is still above 0 count down
            attackCooldown = Mathf.Max(attackCooldown - Time.deltaTime, 0);
        }

        anim.SetFloat("attackCooldown", attackCooldown);

        float playerDistance = Vector3.Distance(target.position, transform.position);
        float playerDistanceX = Mathf.Abs(target.position.x - transform.position.x);
        float playerDistanceZ = Mathf.Abs(target.position.z - transform.position.z);
        //anim.SetFloat("distanceFromPlayer", playerDistance);
        anim.SetFloat("distanceFromPlayerX", playerDistanceX);
        anim.SetFloat("distanceFromPlayerZ", playerDistanceZ);
        //always update animator variable
        UpdateAnimator();

        //Debug.Log("playerDistance: " + playerDistance + ", detectionDistance: " + detectionDistance);

        //sorting order layer changes
        #region
        //if player is in front of boss
        if (target.transform.position.z < transform.position.z)
        {
            foreach (SpriteRenderer sprite in sprites)
            {
                sprite.sortingLayerName = "Boss Layers Back";
            }
        }
        else
        {//else the player is behind the boss
            foreach (SpriteRenderer sprite in sprites)
            {
                sprite.sortingLayerName = "Boss Layers Front";
            }
        }
        #endregion

        //if health is not 0 (dragonWarrior is not dead), then allow movement and control to the AI
        if (healthScript.GetHealth() > 0)
        {

            //if player has walked within aggro distacne
            if (playerDistance < detectionDistance)
            {
                //aggro boss
                isAggro = true;
                anim.SetBool("isAggro", true);
            }

            //sprite direction determination
            #region
            //if moving right OR if the player is to the right of the ninja
            if (navMeshAgent.velocity.x > 0.05f || target.position.x - transform.position.x > 0)
            {
                //set x flip to false (face right)
                flipperObject.transform.rotation = Quaternion.Euler(30, 0, 0);
                facingRight = true;
            }//else if moving left OR if the player is to the left of the ninja
            else if (navMeshAgent.velocity.x < 0.05f || target.position.x - transform.position.x < 0)
            {
                //set x flip to true (face left)
                flipperObject.transform.rotation = Quaternion.Euler(-30, 180, 0);
                facingRight = false;
            }
            #endregion

            //if player has aggro'd boss
            #region
            if (isAggro)
            {
                if (currentPhase == 0 && !isAttacking)
                {
                    //get destination location
                    targetVector = target.position;

                    //log current distance
                    //Debug.Log(Vector3.Distance(playerTransform.position, transform.position));

                    //if boss is within attack range on the X axis AND is not waiting to attack again
                    if (playerDistanceX < 0.7f)
                    {
                        //remove X axis inputs
                        targetVector = new Vector3(transform.position.x, targetVector.y, targetVector.z);

                    }

                    StartMoveAction(targetVector);
                }
                else if (currentPhase == 2 && !isAttacking)
                {
                    //get destination location
                    targetVector = target.position;

                    //log current distance
                    //Debug.Log(Vector3.Distance(playerTransform.position, transform.position));

                    //if boss is within attack range on the X axis for ranged attack AND is not waiting to attack again
                    if (playerDistanceX < 1.7f)
                    {
                        //remove X axis inputs
                        targetVector = new Vector3(transform.position.x, targetVector.y, targetVector.z);

                    }

                    StartMoveAction(targetVector);
                }
                else if (currentPhase == 4 && !isAttacking)
                {
                    //get destination location
                    targetVector = target.position;

                    //log current distance
                    //Debug.Log(Vector3.Distance(playerTransform.position, transform.position));

                    //if boss is within attack range on the X axis AND is not waiting to attack again
                    if (playerDistanceX < 1f)
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
            }
            #endregion
        }
        else
        {//boss is dead
            //turn off health bar
            healthCanvas.SetActive(false);
            //start death animation
            anim.SetBool("isDying", true);
        }
    }

    public void Step()
    {
        footstepsAudioSource.Play();
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

    public void SetPhase(int phaseChange)
    {
        currentPhase = phaseChange;
        anim.SetInteger("phase", phaseChange);
    }

    //dagger swipe functions
    #region
    public void DaggerSwipe()
    {
        //dagger swipe function from Attack 3 animation
        //if player is in damage range and not blocking (this is the only blockable attack)
        if (playerInDamageRange)
        {
            //play sound regardless of if player is blocking or not
            hitAudioSource.clip = daggerSwipeClip;
            hitAudioSource.Play();
            if (target.GetComponent<PlayerController>().GetIsBlocking() == false)
            {
                target.GetComponent<Health>().DoDamage(daggerSwipeDamage);
                target.GetComponent<PlayerController>().Stagger(staggerTime);
            }
        }
    }
    #endregion

    //dagger pound functions
    #region
    public void DaggerPound()
    {
        //dagger pound function from Attack 1 animation
        //if player is in damage range (this attack is not blockable)
        if (playerInDamageRange)
        {
            hitAudioSource.clip = daggerPoundClip;
            hitAudioSource.Play();
            target.GetComponent<Health>().DoDamage(daggerPoundDamage);
            target.GetComponent<PlayerController>().Stagger(staggerTime);
        }
    }
    #endregion

    //dagger pound/swipe functions
    #region
    public void AttackStart()
    {
        isAttacking = true;
        attackCooldown = timeBetweenAttacks;
    }

    public void AttackStop()
    {
        isAttacking = false;
    }

    //if player is in the range of dagger pound/swipe damage
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            //Debug.Log("Player in Range");
            playerInDamageRange = true;
        }
    }

    //if player leaves range of dagger pound/swipe damage
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            //Debug.Log("Player out of Range");
            playerInDamageRange = false;
        }
    }
    #endregion

    //dagger throw functions
    #region
    public void ThrowDagger()
    {
        //dagger throw function from Attack 2 animation
        //determine what direction the boss is facing to attack that direction
        //if facing right
        if (facingRight)
        {
            GameObject dagger = Instantiate(daggerProjectile, transform.position + (transform.up * 0.5f) + (transform.right * 0.5f), Quaternion.LookRotation(transform.forward)) as GameObject;
            dagger.transform.Rotate(30, 0, 0);
            dagger.GetComponent<DaggerMovement>().SetProjectileSpeed(daggerThrowSpeed);
        }
        else
        { //else the boss is facing left
            GameObject dagger = Instantiate(daggerProjectile, transform.position + (transform.up * 0.5f) + (-transform.right * 0.5f), Quaternion.LookRotation(-transform.forward)) as GameObject;
            dagger.transform.Rotate(-30, 0, 0);
            dagger.GetComponent<DaggerMovement>().SetProjectileSpeed(daggerThrowSpeed);
        }
    }
    #endregion

    //death functions
    #region
    public void StayDead()
    {
        anim.SetBool("isDead", true);
        Destroy(destroyOnDeath);
        //transform.GetComponent<Boss1_4>().enabled = false;
    }
    #endregion
}
