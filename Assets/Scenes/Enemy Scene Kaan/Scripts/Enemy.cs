using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class Enemy : MonoBehaviour
{
    //Encapsulation
    //Inheritance

    //references
    [Header("References")]
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected GameObject player;
    [SerializeField] protected PatrolPath patrolPath;
    [SerializeField] protected Animator animator;
    //[SerializeField] ParticleSystem dealDamageParticle;

    //Variables
    [Header("Attack Variables")]
    //Attack
    [SerializeField] protected int health;
    [SerializeField] protected int minDamage;
    [SerializeField] protected int maxDamage;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float cooldown;
    protected bool canAttack;
    protected bool canDealDamage = true;
    protected float timer;

    [Header("Movement Variables")]
    //Movement
    [SerializeField] protected float speed = 2;
    [SerializeField] protected float walkSpeed;
    [SerializeField] protected float followSpeed;
    [SerializeField] protected float observationSphereRadius;
    [SerializeField] protected float suspiciousTime;
    [SerializeField] bool invertForward;
    protected float timeSinceLastSawPlayer;
    protected bool isTriggered;
    Vector3 startPosition;
    protected Transform currentTarget;

    [Header("Patrol Variables")]
    //Patrol
    [SerializeField] float wayPointTolerance = 1f;
    [SerializeField] float wayPointWaitTime;
    int currentWayPointIndex;
    float timeSinceArrivedWayPoint;


    private void Start()
    {
        startPosition = transform.position;
    }
    private void Update()
    {
        isTriggered = ObservationZone();
    }

    //Abstractions
    //Polymorphisms
    //Encapsulations
    //General Methods

    protected abstract void Movement(Transform target);
    protected float DistanceBetweenPlayer()
    {
        return Vector3.Distance(transform.position, player.transform.position);
    }
    bool ObservationZone()
    {
        if (DistanceBetweenPlayer() < observationSphereRadius)
        {
            return true;
        }
        return false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, observationSphereRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    protected void SetForwardToTarget(Transform target)
    {
        float rotateSpeedMultiplier = 4;
        if (!invertForward)
        {
            transform.forward = Vector3.Slerp(transform.forward, TargetDirection(target), Time.deltaTime * rotateSpeedMultiplier);

        }
        else
        {
            transform.forward = Vector3.Slerp(transform.forward, -TargetDirection(target), Time.deltaTime * rotateSpeedMultiplier);
        }
    }


    //Attack Methods
    protected abstract void Attack();
    protected virtual void DealDamage()
    {
        TestPlayerHealth.health -= Random.Range(minDamage, maxDamage);
        Debug.Log("Health: " + TestPlayerHealth.health);
        //Instantiate(dealDamageParticle, transform.position, Quaternion.identity);
        canAttack = false;
        if (TestPlayerHealth.health < 0)
        {
            TestPlayerHealth.health = 0;
        }
    }
    protected virtual void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health < 0)
        {
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, 1);
        }
    }

    //Patrol Methods
    protected void Patrol()
    {
        if (patrolPath == null)
        {
            return;
        }
        if (AtWayPoint())
        {
            timeSinceArrivedWayPoint = 0;
            CycleWayPoint();
        }

        if (timeSinceArrivedWayPoint > wayPointWaitTime)
        {
            Movement(GetNextWayPoint());
        }
        timeSinceArrivedWayPoint += Time.deltaTime;
    }
    private Transform GetNextWayPoint()
    {
        return patrolPath.GetWayPointPosition(currentWayPointIndex);
    }
    private void CycleWayPoint()
    {
        currentWayPointIndex = patrolPath.GetNextIndex(currentWayPointIndex);
    }
    private bool AtWayPoint()
    {
        float distance = Vector3.Distance(transform.position, GetNextWayPoint().position);
        return distance < wayPointTolerance;
    }

    //Target overloads

    //Polymorphism
    protected Vector3 TargetDirection(GameObject target)
    {
        return Vector3.Normalize(target.transform.position - transform.position);
    }
    protected Vector3 TargetDirection(Transform target)
    {
        currentTarget = target;

        return Vector3.Normalize(target.position - transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        transform.position = startPosition;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (canDealDamage)
            {
                DealDamage();
                canDealDamage = false;
            }
        }
    }
}


