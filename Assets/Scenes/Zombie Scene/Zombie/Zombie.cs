using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Zombie : MonoBehaviour {

  public Level7 level;
  public Animator anim;
  public float speed = .5f;
  public float thresholdForStartAttackingTarget = 1.1f;
  public LayerMask ArrowMask, PlayerMask;
  public ZombieAnimationEvents zombieAnimEvent;
  public bool slowWalkSpeed;
  public float slowWalkTimer = 0f;
  public float slowWalkMaxTime = 0.5f;

  [Header("Sound Section")]
  public AudioSource sounds;
  public AudioSource soundsL;
  public AudioSource soundsR;
  public AudioClip[] WalkSounds;
  public AudioClip AttackSound;
  public AudioClip DeathSound;

  public Transform FistsL, FistsR;

  public enum ZombieStatus {
    Init,
    Waiting,
    Walking,
    Chasing,
    Attack,
    Hitting,
    Dead,
  }

  [Header("Status")] public ZombieStatus status = ZombieStatus.Init;

  private Vector3 startPos, endPos, spawnPosition;
  private float waitTime;

  private void Start() {
    Init(level, 0.5f, transform.position);
  }

  internal void Init(Level7 level, float v, Vector3 spawnPos) {
    speed = v;
    this.level = level;
    zombieAnimEvent.zombie = this;
    spawnPosition = spawnPos;
    transform.position = spawnPosition;
    transform.LookAt(transform.position);
    initTime = Random.Range(0, 2f);
  }

  float initTime = 100;
  private void Update() {
    if (level == null) return;

    if (status == ZombieStatus.Init) {
      initTime -= Time.deltaTime;
      if (initTime < 0) StartWalking(spawnPosition);
    }

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
      if (Physics.CheckSphere(FistsL.position, .5f, PlayerMask) || Physics.CheckSphere(FistsR.position, .5f, PlayerMask)) {
        status = ZombieStatus.Waiting;
        waitTime = 10;
        level.PlayerDeath();
      }
    }
  }

  private void OnTriggerEnter(Collider other) {
    int layer = 1 << other.gameObject.layer;

    if (status != ZombieStatus.Dead && (ArrowMask.value & layer) != 0) {
      status = ZombieStatus.Dead;
      anim.speed = 1;
      if (Random.Range(0, 2) == 0) anim.Play("Death");
      else anim.Play("Death 2");
      Destroy(other.transform.parent.gameObject); // Remove the arrow immediately
      level.KillEnemy(gameObject);
      sounds.clip = DeathSound;
      sounds.Play();
    }
  }


  private void StartWalking(Vector3 startPosition) {
    startPos = startPosition;
    float angle = Random.Range(0, Mathf.PI * 2);
    float dist = Random.Range(.5f, 4.5f);
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
    return angle is > -55f and < 55f;
  }

  private void UpdateSlowWalkTimer() {
    slowWalkTimer += Time.deltaTime;

    if (slowWalkTimer > slowWalkMaxTime) {
      slowWalkSpeed = false;
      slowWalkTimer = 0f;
    }
  }

  // ANIMATION EVENTS
  public void AttackStarted() {
    sounds.clip = AttackSound;
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
      soundsL.clip = WalkSounds[Random.Range(0, WalkSounds.Length)];
      soundsL.Play();
    }
    else {
      soundsR.clip = WalkSounds[Random.Range(0, WalkSounds.Length)];
      soundsR.Play();
    }
  }
}