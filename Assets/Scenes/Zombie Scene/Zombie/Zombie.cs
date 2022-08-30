using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Zombie : MonoBehaviour {
    public ZombieLevel level;
    public Animator anim;
    public float speed = 5;
    public LayerMask ArrowMask, PlayerMask;
    public ZombieAnimationEvents zombieAnimEvent;

    public enum ZombieStatus {
        Waiting,
        Walking,
        Chasing,
        Attack,
        Hitting,
        Dead,
    }
    public ZombieStatus status = ZombieStatus.Waiting;

    private Vector3 startPos, endPos, spawnPosition;
    private float waitTime;
    private float chaseTime;

    private void Start() {
        Init(level, 1f, transform.position);
    }

    internal void Init(ZombieLevel level, float v, Vector3 spawnPos) {
        speed = v;
        this.level = level;
        zombieAnimEvent.zombie = this;
        spawnPosition = spawnPos;
        StartWalking(spawnPosition);
        transform.position = spawnPosition;
        transform.LookAt(transform.position - Vector3.zero);
    }

    private void Update() {
        if (level == null) return;

        if (status == ZombieStatus.Walking) {
            WalkingUpdate();
        }

        if (status == ZombieStatus.Waiting) {
            waitTime -= Time.deltaTime;
            if (waitTime <= 0) {
                StartWalking(endPos);
            }
        }
        
        if (status == ZombieStatus.Chasing) {
            ChasingUpdate();
        }
        
        /*if (status == ZombieStatus.Hitting) {
            if (Physics.CheckSphere(transform.position, .5f, PlayerMask)) { // FIXME check these values <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                status = ZombieStatus.Waiting;
                waitTime = 10;
                level.PlayerDeath();
            }
        }*/
        
    }

    private void CheckPlayerIsAimingZombie() {
        if (level.Game.aiming == Controller.Aiming.ArrowReady) {
            float angle = Vector3.SignedAngle(level.Player.forward, transform.position - level.Player.position, Vector3.up);
            if (-35f < angle && angle < 35) {
                // Yes -> defend
                /*anim.speed = 1;
                    anim.SetBool("Defend", true);
                    status = SkeletonStatus.Waiting;
                    waitTime = 2;
                    anim.SetInteger("Move", 0);
                    return;*/
                // TODO: Define if zombie will do something when player is aiming at it.
            }
        }
    }

    private void StartWalking(Vector3 startPosition) {
        startPos = startPosition;
        float angle = Random.Range(0, Mathf.PI * 2);
        float dist = Random.Range(5, 10f);
        endPos = spawnPosition + dist * Mathf.Sin(angle) * Vector3.forward + dist * Mathf.Cos(angle) * Vector3.right;
        endPos = SetYPosFromTerrainHeight(endPos);
        status = ZombieStatus.Walking;
        anim.SetInteger("Move", 1);
    }

    private void WalkingUpdate() {
        if (HasArrivedToEndPos()) {
            SetWaitingStatus(1f, 2.5f);
        }

        transform.position = SetYPosFromTerrainHeight(transform.position);

        float walkMultiplier = 1;
        float dist = Vector3.Distance(transform.position, startPos);
        if (dist < 1) walkMultiplier = dist; // If we just started ramp up the speed
        dist = Vector3.Distance(transform.position, endPos);
        if (dist < 2) walkMultiplier = dist * .5f; // If we are close to the target, slow down the speed
        if (walkMultiplier == 0) walkMultiplier = 1;
        anim.speed = walkMultiplier;

        transform.SetPositionAndRotation(
            Vector3.MoveTowards(transform.position, endPos, walkMultiplier * speed * Time.deltaTime),
            Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(endPos - transform.position),
                2.5f * Time.deltaTime));

        // If Zombie see the player (and are we more far away from the center than the player)?
        if (Vector3.Distance(level.Player.position, transform.position) < 10) {
            if (IsSeeingThePlayer()) {
                StartChasing();
            }
        }
    }

    private void StartChasing() {
        status = ZombieStatus.Chasing;
        chaseTime = 0;
        anim.SetInteger("Move", 2);
    }

    private void ChasingUpdate() {
        chaseTime += Time.deltaTime;
        if (chaseTime > 5) status = ZombieStatus.Waiting;
            
        // Check if we are aiming at the zombie
        // CheckPlayerIsAimingZombie();
            
        endPos = level.Player.position;
        transform.position = SetYPosFromTerrainHeight(transform.position);

        float walkMultiplier = 1;
        float dist = Vector3.Distance(transform.position, startPos);
        if (dist < 1) walkMultiplier = dist; // If we just started ramp up the speed
        dist = Vector3.Distance(transform.position, endPos);
        if (dist < 1.1f) { // Attack
            anim.SetInteger("Move", 0);
            status = ZombieStatus.Attack;
            anim.speed = 1;
            anim.SetTrigger("Attack");
        }
        if (dist < 2f) walkMultiplier = dist * .5f; // If we are close to the target, slow down the speed
        if (walkMultiplier == 0) walkMultiplier = 1;
        anim.speed = walkMultiplier;

        transform.SetPositionAndRotation(
            Vector3.MoveTowards(transform.position, endPos, 2 * walkMultiplier * speed * Time.deltaTime),
            Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(endPos - transform.position), 2.5f * Time.deltaTime));
    }

    private void SetWaitingStatus(float minWaitingTime, float maxWaitingTime) {
        status = ZombieStatus.Waiting;
        waitTime = Random.Range(minWaitingTime, maxWaitingTime);
        anim.SetInteger("Move", 0);
        anim.speed = 1;
    }

    private bool HasArrivedToEndPos() => Vector3.Distance(transform.position, endPos) < 1;

    private Vector3 SetYPosFromTerrainHeight(Vector3 positionToSet) {
        positionToSet.y = level.Forest.SampleHeight(positionToSet);
        return positionToSet;
    }
    
    private void OnTriggerEnter(Collider other) {
        int layer = 1 << other.gameObject.layer;

        if (status != ZombieStatus.Dead && (ArrowMask.value & layer) != 0) {
            /*if (status == SkeletonStatus.Defending) { // In case the arrow hits the shiled (skeleton is defending) then make the arrow bounce
                if (other.transform.parent.parent.TryGetComponent(out Rigidbody rb)) {
                    Vector3 vel = Vector3.Reflect(rb.velocity, transform.forward);
                    rb.velocity = vel * .8f;
                }
                sounds.clip = DefendingSound;
                sounds.Play();
                return;
            }*/ // Zombie can't defend (yet?)

            status = ZombieStatus.Dead;
            anim.speed = 1;
            if (Random.Range(0, 2) == 0) anim.Play("Death");
            else anim.Play("Death 2");
            Destroy(other.transform.parent.gameObject); // Remove the arrow immediately
            level.KillEnemy(gameObject);
            // sounds.clip = DeathSound;
            // sounds.Play();
        }
    }

    private bool IsSeeingThePlayer() {
        float angle = Vector3.SignedAngle(transform.forward, level.Player.position - transform.position, Vector3.up);
        return angle is > -35f and < 35f;
    }
    
    // ANIMATION EVENTS
    public void AttackStarted() {
        // sounds.clip = SwordSwingSound;
        // sounds.Play();
        status = ZombieStatus.Hitting;
    }
    public void AttackCompleted() {
        SetWaitingStatus(1.5f, 3f);
    }
    
    bool soundEmitter = false;
    public void PlayStepSound() {
        soundEmitter = !soundEmitter;
        if (soundEmitter) {
            // soundsL.clip = WalkSounds[Random.Range(0, WalkSounds.Length)];
            // soundsL.Play();
        }
        else {
            // soundsR.clip = WalkSounds[Random.Range(0, WalkSounds.Length)];
            // soundsR.Play();
        }
    }
}
