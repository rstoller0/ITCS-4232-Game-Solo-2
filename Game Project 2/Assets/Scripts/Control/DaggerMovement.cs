using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerMovement : MonoBehaviour
{
    [SerializeField] private GameObject spriteObject;
    [Tooltip("The distance the projectile will travel before disappearing")]
    [SerializeField] private float distanceToTravel = 3.0f;
    private float projectileSpeed = 1.0f;
    [SerializeField] private float projectileDamage = 20.0f;
    [Tooltip("Time player will be staggered (i.e. not able to attack after being hit)")]
    [Range(0, 5)] [SerializeField] private float staggerStat = 0.25f;
    private Rigidbody rb;
    private Vector3 maxRange;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        maxRange = transform.position + (transform.right * distanceToTravel);
        //Debug.Log("This: " + transform.position + ", MaxRange: " + maxRange);
        //projectileSpeed = Random.Range(1.0f, 3.0f);
    }

    public void SetProjectileSpeed(float speed)
    {
        projectileSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = transform.right * projectileSpeed;
        spriteObject.transform.Rotate(new Vector3(0, 0, 360 * Time.deltaTime));

        if (Vector3.Distance(maxRange, transform.position) < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            //if the player is not blocking
            ///block only works with Melee Attacks (this is not a Melee Attack)
            ///if (other.transform.GetComponent<PlayerController>().GetIsBlocking() == false) {
            //do damage to that enemy
            other.transform.GetComponent<Health>().DoDamage(projectileDamage);

            //WORK ON THIS ASPECT, MAY NEED TO ADD A ENEMY PARENT SCRIPT THAT HAS THE STAGGER VARIABLES SO CAN BE ON ALL ENEMY TYPES AND NEED TO ADD ANIMATION STUFF FOR STAGGERS
            //ASLO HAVE NOT ADD A STAGGER ASPECT TO THE ENEMIES
            other.transform.GetComponent<PlayerController>().Stagger(staggerStat);
            ///}

            Destroy(gameObject);
        }
        else if (other.transform.tag == "Enemy" || other.transform.tag == "EnemyProjectile" || other.transform.tag == "EnemyAttackRange" || other.transform.tag == "Item")
        {
            //else if hitting another enemy, do nothing
        }
        else
        {
            //else hit a wall or something so destroy the projectile
            Destroy(gameObject);
        }
    }
}
