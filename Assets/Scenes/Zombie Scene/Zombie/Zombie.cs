using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Zombie : MonoBehaviour {
    
    // Create a folder called Zombies inside the Music scene/Enemies folder put all files there.
    
    public ZombieLevel level;
    public Animator anim;
    public float speed = .5f;
    public float thresholdForStartAttackingTarget = 1.1f;
    public LayerMask ArrowMask, PlayerMask;
    public ZombieAnimationEvents zombieAnimEvent;
    public bool slowWalkSpeed;
    public float slowWalkTimer;
    public float slowWalkMaxTime = 0.5f;
    public bool playGrowlSFX;
    public float playGrowlSFXTimeMin = 5f, playGrowlSFXTimeMax = 10f;
    public float playGrowlSFXTime;
    public float playGrowlSFXTimer;
    
    [Header("Sound Section")] 
    public AudioSource sounds;
    public AudioSource soundsL;
    public AudioSource soundsR;
    public AudioClip[] walkSounds;
    public AudioClip[] attackSounds;
    public AudioClip hitSound;
    public AudioClip deathSound01;
    public AudioClip deathSound02;
    public AudioClip[] growlSounds;
    
    public enum ZombieStatus {
        Waiting,
        Walking,
        Chasing,
        Attack,
        Hitting,
        Dead,
    }
    
    [Header("Status")] public ZombieStatus status = ZombieStatus.Waiting;

    private Vector3 startPos, endPos, spawnPosition;
    private float waitTime;

    private void Start() {
        Init(level, 0.5f, transform.position);
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

        if (status == ZombieStatus.Hitting) {
            if (Physics.CheckSphere(transform.position, 1.2f, PlayerMask)) { // FIXME check these values <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                status = ZombieStatus.Waiting;
                waitTime = 10;
                level.PlayerDeath();
            }
        }

        CheckGrowlSFX();
    }
    
    private void OnTriggerEnter(Collider other) {
        int layer = 1 << other.gameObject.layer;

        if (status != ZombieStatus.Dead && (ArrowMask.value & layer) != 0) {
            status = ZombieStatus.Dead;
            StartDeathAnimSounds();
            Destroy(other.transform.parent.gameObject); // Remove the arrow immediately
            level.KillEnemy(gameObject);
        }
    }

    private void StartDeathAnimSounds() {
        anim.speed = 1;
        if (Random.Range(0, 2) == 0) {
            anim.Play("Death");
            sounds.clip = deathSound01;
        }
        else {
            anim.Play("Death 2");
            sounds.clip = deathSound02;
        }
        sounds.Play();
    }

    private void CheckPlayerIsAimingZombie() { // TODO: Define if zombie will do something when player is aiming at it.
        if (level.Game.aiming == Controller.Aiming.ArrowReady) {
            float angle = Vector3.SignedAngle(level.Player.forward, transform.position - level.Player.position,
                Vector3.up);
            if (-35f < angle && angle < 35) {
                // Yes -> defend
                /*anim.speed = 1;
                    anim.SetBool("Defend", true);
                    status = SkeletonStatus.Waiting;
                    waitTime = 2;
                    anim.SetInteger("Move", 0);
                    return;*/
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
        StartWalkingAnimation();
    }

    private void WalkingUpdate() {
        if (HasArrivedToEndPos()) {
            SetWaitingStatus(1f, 2.5f);
        }

        transform.position = SetYPosFromTerrainHeight(transform.position);

        MoveTransformForward(false);

        // If Zombie see the player (and are we more far away from the center than the player)?
        if (Vector3.Distance(level.Player.position, transform.position) < 20) {
            if (IsSeeingThePlayer()) {
                StartChasing();
            }
        }
    }

    private void StartChasing() {
        status = ZombieStatus.Chasing;
        StartWalkingAnimation();
    }

    private void StartWalkingAnimation() => anim.SetInteger("Move", 1);

    private void ChasingUpdate() {
        // Zombie chases player at the same speed as it walks. 

        endPos = level.Player.position;
        transform.position = SetYPosFromTerrainHeight(transform.position);
        
        MoveTransformForward(true);
    }

    private void MoveTransformForward(bool isChasing) {
        float walkMultiplier = 1;
        float distFromStartPos = Vector3.Distance(transform.position, startPos);
        if (distFromStartPos < 1) walkMultiplier = distFromStartPos; // If we just started ramp up the speed
        
        float distToEndPos = Vector3.Distance(transform.position, endPos);
        
        if (isChasing && distToEndPos < thresholdForStartAttackingTarget) {
            // Attack
            StopWalkingAnimToIdle();
            PlayAttackSound();
            status = ZombieStatus.Attack;
            anim.speed = 1;
            anim.SetTrigger("Attack");
        }

        if (distToEndPos < 2f) walkMultiplier = distToEndPos * .5f; // If we are close to the target, slow down the speed
        if (walkMultiplier == 0) walkMultiplier = 1;
        anim.speed = walkMultiplier;

        float stepSlowerMultiplier = 1f;
        if (slowWalkSpeed) {
            stepSlowerMultiplier = 0f;
            UpdateSlowWalkTimer();  
        }
        
        transform.SetPositionAndRotation(
            Vector3.MoveTowards(transform.position, endPos, stepSlowerMultiplier * walkMultiplier * speed * Time.deltaTime),
            Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(endPos - transform.position),
                2.5f * Time.deltaTime));
    }

    private void StopWalkingAnimToIdle() => anim.SetInteger("Move", 0);

    private void PlayAttackSound() {
        sounds.clip = attackSounds[Random.Range(0, attackSounds.Length)];
        sounds.Play();
    }
    
    private void SetWaitingStatus(float minWaitingTime, float maxWaitingTime) {
        status = ZombieStatus.Waiting;
        waitTime = Random.Range(minWaitingTime, maxWaitingTime);
        StopWalkingAnimToIdle();
        anim.speed = 1;
    }

    private bool HasArrivedToEndPos() => Vector3.Distance(transform.position, endPos) < 1;

    private Vector3 SetYPosFromTerrainHeight(Vector3 positionToSet) {
        positionToSet.y = level.Forest.SampleHeight(positionToSet);
        return positionToSet;
    }

    private bool IsSeeingThePlayer() {
        float angle = Vector3.SignedAngle(transform.forward, level.Player.position - transform.position, Vector3.up);
        return angle is > -35f and < 35f;
    }

    private void UpdateSlowWalkTimer() {
        slowWalkTimer += Time.deltaTime;
        
        if (slowWalkTimer > slowWalkMaxTime) {
            slowWalkSpeed = false;
            slowWalkTimer = 0f;
        }
    }

    private void CheckGrowlSFX() {
        playGrowlSFXTimer += Time.deltaTime;
        if (ShouldReproduceGrowlSFX()) {
            sounds.clip = growlSounds[Random.Range(0, growlSounds.Length)];
            sounds.Play();
            
        }
    }

    private bool ShouldReproduceGrowlSFX() {
        if (playGrowlSFX) {
            playGrowlSFX = false;
            return true;
        }
        if (playGrowlSFXTimer > playGrowlSFXTime) {
            playGrowlSFXTimer = 0f;
            playGrowlSFXTime = Random.Range(playGrowlSFXTimeMin, playGrowlSFXTimeMax);
            return true;
        }

        return false;
    }

    // ANIMATION EVENTS
    public void AttackStarted() {
        sounds.clip = hitSound;
        sounds.Play();
        status = ZombieStatus.Hitting;
    }

    public void AttackCompleted() {
        SetWaitingStatus(1.5f, 3f);
    }

    public void Step() {
        slowWalkSpeed = true;
        PlayStepSound();
    }

    bool soundEmitter = false;

    private void PlayStepSound() {
        soundEmitter = !soundEmitter;
        if (soundEmitter) {
            soundsL.clip = walkSounds[Random.Range(0, walkSounds.Length)];
            soundsL.Play();
        }
        else {
            soundsR.clip = walkSounds[Random.Range(0, walkSounds.Length)];
            soundsR.Play();
        }
    }
}