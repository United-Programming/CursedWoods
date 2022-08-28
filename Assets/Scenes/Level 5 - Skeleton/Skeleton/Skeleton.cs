using Unity.VisualScripting;
using UnityEngine;

public class Skeleton : MonoBehaviour {
  public Level5 level;
  public Animator anim;
  public Transform BodyCenter;
  public float speed = 5;
  public LayerMask ArrowMask, PlayerMask;
  public AudioSource sounds;
  public AudioSource soundsL;
  public AudioSource soundsR;
  public AudioClip[] WalkSounds;
  public AudioClip RoarSound;
  public AudioClip DeathSound;
  public AudioClip HitSound;
  public SkeletonAnimEvents skelAnimEvent;
  Vector3 startPos, endPos, spawnPosition;

  public enum SkeletonStatus {
    Waiting, Walking, Chasing, Attack, Hitting, Defending, Dead
  };
  public SkeletonStatus status = SkeletonStatus.Waiting;

  internal void Init(Level5 l, float v, Vector3 spawnPosition) {
    skelAnimEvent.skeleton = this;
    this.spawnPosition = spawnPosition;
    startPos = spawnPosition;
    float angle = Random.Range(0, Mathf.PI * 2);
    float dist = Random.Range(5, 10f);
    endPos = spawnPosition + dist * Mathf.Sin(angle) * Vector3.forward + dist * Mathf.Cos(angle) * Vector3.right;
    endPos.y = l.Forest.SampleHeight(endPos);
    transform.position = spawnPosition;
    transform.LookAt(transform.position - l.Center.position);
    status = SkeletonStatus.Walking;
    speed = v;
    level = l;
    anim.SetBool("Move", true);
  }


  /*
    !Walk around the spawn position, not much far away in radious, do a movement like the bear
    !If player is visible, run to it and attack
    In case player shoot, draw the shield and stop. Shield will block arrows
    In case player shoots while not visible (and not hit) get alerted and attack the player.
   
   
   */

  float waitTime = 0;
  float chaseTime = 0;

  private void Update() {
    if (level == null) return;

    if (status == SkeletonStatus.Walking) {
      float dist, angle;
      if (Vector3.Distance(transform.position, endPos) < 1) {
        // make the standard wait for a few random seconds
        status = SkeletonStatus.Waiting;
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
      if (dist < 1) walkMultiplier = dist; // If we just started ramp up the speed
      dist = Vector3.Distance(transform.position, endPos);
      if (dist < 2) walkMultiplier = dist * .5f; // If we are close to the target, slow down the speed
      if (walkMultiplier == 0) walkMultiplier = 1;
      anim.speed = walkMultiplier;

      transform.SetPositionAndRotation(
        Vector3.MoveTowards(transform.position, endPos, walkMultiplier * speed * Time.deltaTime),
        Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(endPos - transform.position), 2.5f * Time.deltaTime));


      // do we see the player (and are we more far away from the center than the player)?
      if (Vector3.Distance(level.Player.position, transform.position) < 10) {
        angle = Vector3.SignedAngle(transform.forward, level.Player.position - transform.position, Vector3.up);
        if (-35f < angle && angle < 35) { // Yes -> chase
          StartChasing();
        }
      }
    }

    if (status == SkeletonStatus.Waiting) {
      waitTime -= Time.deltaTime;
      if (waitTime < 0) {
        startPos = endPos;
        float angle = Random.Range(0, Mathf.PI * 2);
        float dist = Random.Range(5, 10f);
        endPos = spawnPosition + dist * Mathf.Sin(angle) * Vector3.forward + dist * Mathf.Cos(angle) * Vector3.right;
        endPos.y = level.Forest.SampleHeight(endPos);
        status = SkeletonStatus.Walking;
        anim.SetBool("Move", true);
      }
    }

    if (status == SkeletonStatus.Chasing) {
      chaseTime += Time.deltaTime;
      if (chaseTime > 5) status = SkeletonStatus.Waiting;


      // Check if we are aiming at the skeleton
      if (level.Game.aiming == Controller.Aiming.ArrowReady) {
        float angle = Vector3.SignedAngle(level.Player.forward, transform.position - level.Player.position, Vector3.up);
        if (-35f < angle && angle < 35) { // Yes -> defend
          anim.speed = 1;
          anim.SetTrigger("Defend");
          status = SkeletonStatus.Waiting;
          waitTime = 2;
          anim.SetBool("Run", false);
          anim.SetBool("Move", false);
          return;
        }
      }


      endPos = level.Player.position;
      float dist;
      Vector3 pos = transform.position;
      pos.y = level.Forest.SampleHeight(pos);
      transform.position = pos;

      float walkMultiplier = 1;
      dist = Vector3.Distance(transform.position, startPos);
      if (dist < 1) walkMultiplier = dist; // If we just started ramp up the speed
      dist = Vector3.Distance(transform.position, endPos);
      if (dist < 1.1f) { // Attack
        status = SkeletonStatus.Attack;
        sounds.clip = RoarSound;
        sounds.Play();
        anim.speed = 1;
        if (Random.Range(0, 2) == 0) anim.SetTrigger("AttackL");
        else anim.SetTrigger("AttackH");
      }
      if (dist < 2f) walkMultiplier = dist * .5f; // If we are close to the target, slow down the speed
      if (walkMultiplier == 0) walkMultiplier = 1;
      anim.speed = walkMultiplier;

      transform.SetPositionAndRotation(
        Vector3.MoveTowards(transform.position, endPos, 2 * walkMultiplier * speed * Time.deltaTime),
        Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(endPos - transform.position), 2.5f * Time.deltaTime));
    }

    if (status == SkeletonStatus.Hitting) {
      if (Physics.CheckSphere(BodyCenter.position, .5f, PlayerMask)) { // FIXME check these values <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        status = SkeletonStatus.Waiting;
        waitTime = 10;
        level.PlayerDeath();
      }
    }

    if (status == SkeletonStatus.Defending) {
      if (level.Game.aiming != Controller.Aiming.ArrowReady) {
        startPos = transform.position;
        endPos = transform.position + transform.forward;
        endPos.y = level.Forest.SampleHeight(endPos);
        status = SkeletonStatus.Walking;
        anim.speed = 1;
        anim.Play("Idle");
        anim.SetBool("Move", true);
      }
    }
  }


  public void StartChasing() {
    status = SkeletonStatus.Chasing;
    chaseTime = 0;
    anim.SetBool("Run", true);
    anim.SetBool("Move", false);
  }

  public void StartDefending() {
    status = SkeletonStatus.Defending;
    anim.SetBool("Run", false);
    anim.SetBool("Move", false);
  }

  bool soundEmitter = false;
  public void PlayStepSound() {
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

  public void AttackStarted() {
    status = SkeletonStatus.Hitting;
  }
  public void AttackCompleted() {
    status = SkeletonStatus.Waiting;
    waitTime = Random.Range(1.5f, 3f);
  }


  private void OnTriggerEnter(Collider other) {
    int layer = 1 << other.gameObject.layer;

    if (status != SkeletonStatus.Dead && status != SkeletonStatus.Defending && (ArrowMask.value & layer) != 0) { // Have 3 levels of hits, and change the material at every hit
      status = SkeletonStatus.Dead;
      anim.speed = 1;
      anim.Play("Death");
      Destroy(other.transform.parent.gameObject); // Remove the arrow immediately
      level.KillEnemy(gameObject);
      sounds.clip = DeathSound;
      sounds.Play();
    }
  }

}

