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
    [SerializeField] private float attackRange = 0.55f;
    [SerializeField] private float attackDamage = 25;
    private float swordDamage = 20;
    private float axeDamage = 35;
    private float scytheDamage = 15;
    [Tooltip("Time between attacks (Lower means faster attack speed)")]
    [Range(0, 5)] [SerializeField] private float timeBetweenAttacks = 1.35f;
    private float attackCooldown = 0;
    private float swordCooldown = 1.5f;
    private float axeCooldown = 1.5f;
    private float scytheCooldown = 1.5f;
    [Tooltip("Time enemy will be staggered (i.e. not able to attack after being hit)")]
    [Range(0, 5)] [SerializeField] private float staggerStat = 0.25f;
    private float swordStaggerStat = 0.15f;
    private float axeStaggerStat = 0.35f;
    private float scytheStaggerStat = 0.05f;
    private bool isAttacking = false;
    private bool isWaitingToAttack = false;
    private bool isStaggered = false;
    private bool isBlocking = false;
    ///private bool startBlocking = false;
    [SerializeField]private float maxBlockTime = 1.0f;
    [SerializeField]private float blockTimer = 0;
    [SerializeField]private float timeBetweenBlocks = 0.5f;
    [SerializeField]private float blockCooldown = 0;
    private Health healthScript;
    [SerializeField] private float timeBetweenWeaponSwaps;
    private float weaponSwapCooldown = 0;
    [SerializeField] private bool axeAquired = false;
    [SerializeField] private bool scytheAquired = false;
    private bool hasSword = true;
    private bool hasAxe = false;
    private bool hasScythe = false;
    private int attackChain = 0;
    private float attackChainTimer = 0;
    private float attackChainLossTime = 1.3f;

    //stagger time variable to be changed by attackers weapon stats
    private float staggerTime = 0.25f;

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

    //audio variables
    [SerializeField] private AudioSource footstepsAudioSource;
    [SerializeField] private AudioClip swordHitClip;
    [SerializeField] private AudioClip axeHitClip;
    [SerializeField] private AudioClip scytheHitClip;
    [SerializeField] private AudioSource hitAudioSource;

    public bool GetIsBlocking()
    {
        return isBlocking;
    }

    private void Start()
    {
        //get player's rigidbody at start
        rb = GetComponent<Rigidbody>();

        //get player's animator at start
        anim = GetComponent<Animator>();

        //get player's sprite renderer at start
        sr = GetComponentInChildren<SpriteRenderer>();

        //get ninja's health script at start
        healthScript = GetComponent<Health>();

        //start with sword attack and stagger stat
        attackDamage = swordDamage;
        staggerStat = swordStaggerStat;
    }

    private void Update()
    {
        //if the game is not paused
        if (!GameManager.instance.isPaused)
        {
            //START OF UPDATE [INSIDE if (!GameManager.instance.isPaused)]

            //to limit attack speed and allow movement after attack animation finishes but not allow to attack yet
            if (attackCooldown > 0)
            {
                //if the attack cooldown is still above 0 count down
                attackCooldown = Mathf.Max(attackCooldown - Time.deltaTime, 0);
            }

            //if just swapped weapons (time is greater than 0)
            if (weaponSwapCooldown > 0)
            {
                //if the weapon swap cooldown is still above 0 count down
                weaponSwapCooldown = Mathf.Max(weaponSwapCooldown - Time.deltaTime, 0);
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
                    //if plater is not attacking AND is not blocking
                    if (!isAttacking && !isBlocking)
                    {
                        //move player on X axis at full player speed and Z axis at half player speed (FOR MOVEMENT BALANCE) based on inputs and keep the movement on Y axis as is
                        //[REMEOVE (/ 2) IF NEED FASTER VERTICAL MOVEMENT]
                        rb.velocity = new Vector3(axisInputs.x * playerSpeed, rb.velocity.y, axisInputs.z * (playerSpeed / 2));
                    }
                    //}
                    #endregion

                    //update current speed every frame
                    currentSpeed = rb.velocity.magnitude;

                    //update animator's speed variable to currentSpeed
                    anim.SetFloat("speed", currentSpeed);

                    //Debug.Log("currentSpeed: " + currentSpeed);

                    if (currentSpeed > 0.1f)
                    {
                        //if currently walking, then play the footsteps audio
                        //footstepsAudioSource.volume = 0.25f;
                    }
                    else
                    {
                        //else note currently walking, stop the footsteps audio
                        //footstepsAudioSource.volume = 0;
                    }

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
                    //if Q key is pressed AND swap cooldown is 0 AND not attacking AND not blocking, swap weapons
                    #region
                    if (Input.GetKeyDown(KeyCode.Q) && weaponSwapCooldown == 0 && !isAttacking && !isBlocking)
                    {
                        if (axeAquired)
                        {
                            //if the axe has been aquired
                            if (scytheAquired)
                            {
                                //if the axe and the scythe have been aquired
                                if (hasSword)
                                {
                                    hasSword = false;
                                    hasAxe = true;
                                    attackDamage = axeDamage;
                                    timeBetweenAttacks = axeCooldown;
                                    staggerStat = axeStaggerStat;
                                    hitAudioSource.clip = axeHitClip;
                                    hasScythe = false;
                                }
                                else if (hasAxe)
                                {
                                    hasSword = false;
                                    hasAxe = false;
                                    hasScythe = true;
                                    attackDamage = scytheDamage;
                                    timeBetweenAttacks = scytheCooldown;
                                    staggerStat = scytheStaggerStat;
                                    hitAudioSource.clip = scytheHitClip;
                                }
                                else
                                {
                                    hasSword = true;
                                    attackDamage = swordDamage;
                                    timeBetweenAttacks = swordCooldown;
                                    staggerStat = swordStaggerStat;
                                    hitAudioSource.clip = swordHitClip;
                                    hasAxe = false;
                                    hasScythe = false;
                                }
                            }
                            else
                            {
                                //else only the axe has been aquired
                                if (hasSword)
                                {
                                    hasSword = false;
                                    hasAxe = true;
                                    attackDamage = axeDamage;
                                    timeBetweenAttacks = axeCooldown;
                                    staggerStat = axeStaggerStat;
                                    hitAudioSource.clip = axeHitClip;
                                    hasScythe = false;
                                }
                                else
                                {
                                    hasSword = true;
                                    attackDamage = swordDamage;
                                    timeBetweenAttacks = swordCooldown;
                                    staggerStat = swordStaggerStat;
                                    hitAudioSource.clip = swordHitClip;
                                    hasAxe = false;
                                    hasScythe = false;
                                }
                            }
                        }
                        //else axe is not aquired, so neither is the scythe

                        anim.SetBool("hasSword", hasSword);
                        anim.SetBool("hasAxe", hasAxe);
                        anim.SetBool("hasScythe", hasScythe);

                        //set weapon swap cooldown
                        weaponSwapCooldown = timeBetweenWeaponSwaps;
                    }
                    #endregion

                    //attack chain code
                    #region
                    //if in an attack chain, add time to timer
                    if (attackChain != 0)
                    {
                        Debug.Log("attackChainTimer: " + attackChainTimer);
                        attackChainTimer += Time.deltaTime;
                    }

                    if (attackChainTimer > attackChainLossTime)
                    {
                        attackChain = 0;
                        attackChainTimer = 0;
                    }
                    #endregion

                    //if (left mouse button OR Spacebar key is pressed) AND player is able to attack again (cooldown is at 0)
                    if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && !isAttacking && attackCooldown == 0 && !isBlocking)
                    {
                        //set is attacking to true
                        isAttacking = true;

                        //cooldown stuff for different weapons
                        if(hasAxe)
                        {
                            //set attack cooldown
                            attackCooldown = timeBetweenAttacks;
                        }
                        else if(hasSword)
                        {
                            attackChain += 1;
                            attackChainTimer = 0;
                            if (attackChain >= 2)
                            {
                                //set attack cooldown
                                attackCooldown = timeBetweenAttacks;
                            }
                        }
                        else if (hasScythe)
                        {
                            attackChain += 1;
                            attackChainTimer = 0;
                            if (attackChain >= 3)
                            {
                                //set attack cooldown
                                attackCooldown = timeBetweenAttacks;
                            }
                        }
                        
                        //set attack trigger in animator to true
                        anim.SetTrigger("attack");
                    }

                    //here we keep track of block cooldown (when blockCooldown reaches 0 player can block again)
                    if (blockCooldown > 0)
                    {
                        blockCooldown = Mathf.Max(0, blockCooldown - Time.deltaTime);
                    }

                    //if left shift is pressed AND player is not moving AND player is not staggered AND player is not attacking AND block is not on cooldown
                    if (Input.GetKey(KeyCode.LeftShift) && currentSpeed < 0.01 && !isStaggered && !isAttacking && blockCooldown == 0)
                    {
                        //set player to blocking
                        ///startBlocking = true;
                        anim.SetBool("isBlocking", true);
                    }

                    //here we keep track of time spent blocking (when block timer reaches maxBlockTime it will stop blocking)
                    if (isBlocking)
                    {
                        blockTimer += Time.deltaTime;
                    }

                    //if left shift is released OR blockTimer has reached maxBlockTime
                    if (Input.GetKeyUp(KeyCode.LeftShift) || blockTimer > maxBlockTime)
                    {
                        StopBlocking();
                    }
                    #endregion
                }
            }
            else
            { //else the player is dead
              //NEED TO CHANGE THIS [BUT FOR NOW IF YOU DIE IT JUST RELOADS THE LEVEL]
                SceneManager.LoadScene(currentScene);
            }

            //END OF UPDATE [INSIDE if (!GameManager.instance.isPaused)]
        }
    }

    public void Step()
    {
        footstepsAudioSource.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "axePickup")
        {
            axeAquired = true;
            Destroy(other.gameObject);
        }

        if (other.tag == "scythePickup")
        {
            scytheAquired = true;
            Destroy(other.gameObject);
        }
    }

    //block functions
    #region
    //stop blocking function called in code
    private void StopBlocking()
    {
        //set player to not blocking
        ///startBlocking = false;
        isBlocking = false;
        anim.SetBool("isBlocking", false);

        //reset blockTimer to 0
        blockTimer = 0;
        //set block cooldown to time between blocks
        blockCooldown = timeBetweenBlocks;
    }
    
    //used in animation for blocking
    public void Block()
    {
        //set is blocking to true and trigger the animation
        isBlocking = true;
    }
    #endregion

    //stagger functions
    #region
    public void Stagger(float timeToStagger)
    {
        //set is blocking to false (so that it does not jump back to blocking after stagger animation plays through)
        StopBlocking();

        //set is staggered to true and trigger the animation
        isStaggered = true;
        staggerTime = timeToStagger;
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
        attackCooldown = staggerTime;
    }
    #endregion

    //attack animation events
    #region
    //raycasting for damage dealing
    private void Hit()
    {
        #region
        //bit shift the index of the Enemy layer (12) and EnemyProjectile layer (13)
        int layerMask1 = 1 << 12;
        //int layerMask2 = 1 << 10;
        int layerMask3 = 1 << 13;

        //this would cast rays only against colliders in layer 8 or against layer 9.
        //but instead we want to collide against everything except layer 8 and layer 9. The ~ operator does this, it inverts a bitmasks.
        //int layerMask4 = ~(layerMask1 | layerMask2);
        int layerMask5 = (layerMask1 | layerMask3);

        //set up variables for raycast detection of hitting enemies
        //RaycastHit hit;
        Vector3 rayDir;
        //Vector3 rayDir2; //IF I WANT TO ADD MORE HIT WIDTH
        //Vector3 rayDir3; //IF I WANT TO ADD MORE HIT WIDTH
        RaycastHit[] hits;
        //RaycastHit[] hits2; //IF I WANT TO ADD MORE HIT WIDTH
        //RaycastHit[] hits3; //IF I WANT TO ADD MORE HIT WIDTH
        Vector3 rayStartPosition;

        //determine what direction the payer is facing to attack that direction
        //if facing right
        if (facingRight)
        {
            //set attack direction to right
            rayDir = transform.right;
            //rayDir2 = transform.right + new Vector3(0, 0, 0.5f); //IF I WANT TO ADD MORE HIT WIDTH
            //rayDir3 = transform.right + new Vector3(0, 0, -0.5f); //IF I WANT TO ADD MORE HIT WIDTH

            //move the start of the ray to the left side of the player's collider (this is to avoid hits not registering if the enemy is right on the player)
            //rayStartPosition = new Vector3(transform.position.x - 0.125f, transform.position.y + 0.25f, transform.position.z);
            rayStartPosition = new Vector3(transform.position.x - 0.125f, transform.position.y + 0.1f, transform.position.z);
        }
        else
        { //else the player is facing left
            //set attack direction to left
            rayDir = -transform.right;
            //rayDir2 = -transform.right + new Vector3(0, 0, 0.5f); //IF I WANT TO ADD MORE HIT WIDTH
            //rayDir3 = -transform.right + new Vector3(0, 0, -0.5f); //IF I WANT TO ADD MORE HIT WIDTH

            //move the start of the ray to the right side of the player's collider (this is to avoid hits not registering if the enemy is right on the player)
            //rayStartPosition = new Vector3(transform.position.x + 0.125f, transform.position.y + 0.25f, transform.position.z);
            rayStartPosition = new Vector3(transform.position.x + 0.125f, transform.position.y + 0.1f, transform.position.z);
        }

        Debug.DrawRay(rayStartPosition, rayDir * attackRange, Color.green, 50000);
        //hits = Physics.RaycastAll(rayStartPosition, rayDir, attackRange, layerMask1);
        hits = Physics.RaycastAll(rayStartPosition, rayDir, attackRange, layerMask5);
        //hits = Physics.RaycastAll(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), rayDir, attackRange, layerMask1);
        //hits2 = Physics.RaycastAll(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), rayDir2, attackRange, layerMask1); //IF I WANT TO ADD MORE HIT WIDTH
        //hits3 = Physics.RaycastAll(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), rayDir3, attackRange, layerMask1); //IF I WANT TO ADD MORE HIT WIDTH

        bool hitAnEnemy = false;

        //can hit multiple enemies at once
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.tag == "Enemy")
            {
                if (hit.transform.GetComponent<DragonWarriorNavMesh>() != null)
                {
                    //do damage to that enemy
                    hit.transform.GetComponent<Health>().DoDamage(attackDamage);

                    //WORK ON THIS ASPECT, MAY NEED TO ADD A ENEMY PARENT SCRIPT THAT HAS THE STAGGER VARIABLES SO CAN BE ON ALL ENEMY TYPES AND NEED TO ADD ANIMATION STUFF FOR STAGGERS
                    //ASLO HAVE NOT ADD A STAGGER ASPECT TO THE ENEMIES
                    hit.transform.GetComponent<DragonWarriorNavMesh>().Stagger(staggerStat);
                    //Debug.DrawRay(transform.position, rayDir * attackRange, Color.red, 50000);
                    //Debug.DrawRay(transform.position, rayDir2 * attackRange, Color.green, 50000); //IF I WANT TO ADD MORE HIT WIDTH??
                    //Debug.DrawRay(transform.position, rayDir3 * attackRange, Color.blue, 50000); //IF I WANT TO ADD MORE HIT WIDTH??
                }
                else if (hit.transform.GetComponent<NinjaNavMesh>() != null)
                {
                    //do damage to that enemy
                    hit.transform.GetComponent<Health>().DoDamage(attackDamage);

                    //WORK ON THIS ASPECT, MAY NEED TO ADD A ENEMY PARENT SCRIPT THAT HAS THE STAGGER VARIABLES SO CAN BE ON ALL ENEMY TYPES AND NEED TO ADD ANIMATION STUFF FOR STAGGERS
                    //ASLO HAVE NOT ADD A STAGGER ASPECT TO THE ENEMIES
                    hit.transform.GetComponent<NinjaNavMesh>().Stagger(staggerStat);
                    //Debug.DrawRay(transform.position, rayDir * 0.4f, Color.red, 50000);
                    //Debug.DrawRay(transform.position, rayDir2 * 0.4f, Color.green, 50000); //IF I WANT TO ADD MORE HIT WIDTH??
                    //Debug.DrawRay(transform.position, rayDir3 * 0.4f, Color.blue, 50000); //IF I WANT TO ADD MORE HIT WIDTH??
                }
                else if (hit.transform.GetComponent<Boss1_4>() != null)
                {
                    //do damage to that enemy
                    hit.transform.GetComponent<Health>().DoDamage(attackDamage);

                    //WORK ON THIS ASPECT, MAY NEED TO ADD A ENEMY PARENT SCRIPT THAT HAS THE STAGGER VARIABLES SO CAN BE ON ALL ENEMY TYPES AND NEED TO ADD ANIMATION STUFF FOR STAGGERS
                    //ASLO HAVE NOT ADD A STAGGER ASPECT TO THE ENEMIES
                    ///hit.transform.GetComponent<Boss1_4>().Stagger(staggerStat);
                    //Debug.DrawRay(transform.position, rayDir * 0.4f, Color.red, 50000);
                    //Debug.DrawRay(transform.position, rayDir2 * 0.4f, Color.green, 50000); //IF I WANT TO ADD MORE HIT WIDTH??
                    //Debug.DrawRay(transform.position, rayDir3 * 0.4f, Color.blue, 50000); //IF I WANT TO ADD MORE HIT WIDTH??
                }

                //set enemy hit so it will only play the hit sounds once
                hitAnEnemy = true;
            }
            else if (hit.transform.tag == "EnemyProjectile")
            {
                //Debug.Log("Hit Projectile");

                if (hit.transform.GetComponent<ProjectileMovement>() != null)
                {
                    //destry enemy projectile
                    hit.transform.GetComponent<ProjectileMovement>().DestroyFireball();
                }
                else if (hit.transform.GetComponent<DaggerMovement>() != null)
                {
                    //destry enemy projectile
                    hit.transform.GetComponent<DaggerMovement>().DestroyDagger();
                }

                //destroy enemy projectile
                //Destroy(hit.transform.gameObject);
            }
        }

        if (hitAnEnemy)
        {
            //play hit sound
            hitAudioSource.Play();
        }
        else
        {
            //play miss sound [IF I WANT TO IMPLEMENT A MISS SOUND]
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
