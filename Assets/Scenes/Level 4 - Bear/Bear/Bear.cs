using System.Collections.Generic;
using UnityEngine;
using static Frog;

public class Bear : MonoBehaviour {
  public Level4 level;
  public Animator anim;
  public Transform BodyCenter;
  public float speed = 5;
  public LayerMask ArrowMask, PlayerMask;
  public AudioSource sounds;
  public AudioSource sounds2;
  public AudioSource soundAttack;
  public AudioClip[] WalkSounds;
  public AudioClip RoarSound;
  public AudioClip DeathSound;
  public AudioClip HitSound;
  public Material[] SkinMaterials;
  public SkinnedMeshRenderer Skin;
  Vector3 startPos, endPos;
  int hits = 0;

  public enum BearStatus {
    Waiting, Walking, Buffing, Chasing, Attack, Dead
  };
  public BearStatus status = BearStatus.Waiting;

  internal void Init(Level4 l, float v, Vector3 spawnPosition) {
    startPos = spawnPosition;
    float angle = Random.Range(0, Mathf.PI * 2);
    float dist = 25 + Random.Range(0, 30f);
    endPos = l.controller.transform.position + Vector3.forward * Mathf.Sin(angle) * dist + Vector3.right * Mathf.Cos(angle) * dist;
    endPos.y = l.Forest.SampleHeight(endPos);
    transform.position = spawnPosition;
    status = BearStatus.Walking;
    speed = v;
    level = l;
    anim.SetBool("Move", true);
    hits = 0;
    Skin.sharedMaterial = SkinMaterials[hits];
  }


  /*
    Walk around, including the center of the area, max distance 50 from center
    If the player is close enoug, and the distance from the center is > the player distance from center:
    - roar
    - run against the player
    - if close enough hit
    - if hit by arrow stun and change material
   
   
   */

  float waitTime = 0;

  private void Update() {
    if (level == null) return;

    if (status == BearStatus.Walking) {
      float dist, angle;
      if (Vector3.Distance(transform.position, endPos) < 1) {
        // make the standard wait for a few random seconds
        status = BearStatus.Waiting;
        waitTime = Random.Range(1, 2.5f);
        anim.SetBool("Move", false);
        anim.SetBool("Run", false);
        anim.speed = 1;
      }
      Vector3 pos = transform.position;
      pos.y = level.Forest.SampleHeight(pos);
      transform.position = pos;

      float walkMultiplier = 1;
      dist = Vector3.Distance(transform.position, startPos);
      if (dist < 3) walkMultiplier = dist * .33333334f; // If we just started ramp up the speed
      dist = Vector3.Distance(transform.position, endPos);
      if (dist < 5) walkMultiplier = dist * .2f; // If we are close to the target, slow down the speed
      if (walkMultiplier == 0) walkMultiplier = 1;
      anim.speed = walkMultiplier;

      transform.SetPositionAndRotation(
        Vector3.MoveTowards(transform.position, endPos, walkMultiplier * speed * Time.deltaTime),
        Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(endPos - transform.position), 2.5f * Time.deltaTime));


      // do we see the player (and are we more far away from the center than the player)?
      if (Vector3.Distance(level.controller.transform.position, transform.position) > 18) {
        angle = Vector3.SignedAngle(transform.forward, level.Player.position - transform.position, Vector3.up);
        if (-35f < angle && angle < 35) { // Yes -> roar and run
          status = BearStatus.Buffing;
          anim.Play("Buff");
          anim.SetBool("Run", false);
          anim.SetBool("Move", false);
          sounds.clip = RoarSound;
          sounds.Play();
        }
      }
    }

    if (status == BearStatus.Waiting) {
      waitTime -= Time.deltaTime;
      if (waitTime < 0) {
        float angle = Random.Range(0, Mathf.PI * 2);
        float dist = 25 + Random.Range(0, 30f);
        startPos = endPos;
        endPos = level.controller.transform.position + Vector3.forward * Mathf.Sin(angle) * dist + Vector3.right * Mathf.Cos(angle) * dist;
        endPos.y = level.Forest.SampleHeight(endPos);
        status = BearStatus.Walking;
        anim.SetBool("Move", true);
      }
    }

    if (status == BearStatus.Chasing) {
      endPos = level.controller.player.position;
      float dist;
      Vector3 pos = transform.position;
      pos.y = level.Forest.SampleHeight(pos);
      transform.position = pos;

      float walkMultiplier = 1;
      dist = Vector3.Distance(transform.position, startPos);
      if (dist < 2) walkMultiplier = dist * .5f; // If we just started ramp up the speed
      dist = Vector3.Distance(transform.position, endPos);
      if (dist < 2.1f) { // Attack
        status = BearStatus.Attack;
        sounds.clip = RoarSound;
        sounds.Play();
        anim.SetBool("Run", false);
        anim.SetBool("Move", false);
        anim.Play("Attack");
      }
      if (dist < 3) walkMultiplier = dist * .3f; // If we are close to the target, slow down the speed
      if (walkMultiplier == 0) walkMultiplier = 1;
      anim.speed = walkMultiplier;

      transform.SetPositionAndRotation(
        Vector3.MoveTowards(transform.position, endPos, 2 * walkMultiplier * speed * Time.deltaTime),
        Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(endPos - transform.position), 2.5f * Time.deltaTime));
    }

    if (status == BearStatus.Attack) {
      if (Physics.CheckSphere(BodyCenter.position, .7f, PlayerMask)) {
        status = BearStatus.Waiting;
        waitTime = 10;
        level.PlayerDeath();
      }
    }
  }


  public void StartChasing() {
    status = BearStatus.Chasing;
    anim.SetBool("Run", true);
    anim.SetBool("Move", false);
  }

  bool soundEmitter = false;
  public void PlayStepSound() {
    soundEmitter = !soundEmitter;
    if (soundEmitter) {
      sounds.clip = WalkSounds[Random.Range(0, WalkSounds.Length)];
      sounds.Play();
    }
    else {
      sounds2.clip = WalkSounds[Random.Range(0, WalkSounds.Length)];
      sounds2.Play();
    }
  }

  public void AttackCompleted() {
    status = BearStatus.Waiting;
    waitTime = 10;
  }


  private void OnTriggerEnter(Collider other) {
    int layer = 1 << other.gameObject.layer;

    if (status != BearStatus.Dead && (ArrowMask.value & layer) != 0) { // Have 3 levels of hits, and change the material at every hit
      hits++;
      if (hits < 4) {
        level.KillEnemy(null);
        sounds.clip = HitSound;
        sounds.Play();
        Skin.sharedMaterial = SkinMaterials[hits];
        anim.Play("Stunned");
        status = BearStatus.Waiting;
        waitTime = 2;
        return;
      }

      status = BearStatus.Dead;
      anim.Play("Death");
      Destroy(other.transform.parent.gameObject); // Remove the arrow immediately
      level.KillEnemy(gameObject);
      sounds.clip = DeathSound;
      sounds.Play();
    }
  }

}

