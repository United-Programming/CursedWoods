using UnityEngine;

public class Bear : MonoBehaviour {
  public Level4 level;
  public Animator anim;
  public Transform BodyCenter;
  public float speed = 5;
  public LayerMask ArrowMask, PlayerMask;
  public AudioSource sounds;
  public AudioSource soundAttack;
  public AudioClip WalkSound;
  public AudioClip RoarSound;
  public AudioClip DeathSound;
  Vector3 startPos, endPos;


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
        angle = Random.Range(0, Mathf.PI * 2);
        dist = 25 + Random.Range(0, 30f);
        startPos = endPos;
        endPos = level.controller.transform.position + Vector3.forward * Mathf.Sin(angle) * dist + Vector3.right * Mathf.Cos(angle) * dist;
        endPos.y = level.Forest.SampleHeight(endPos);

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
      if (Vector3.Distance(level.controller.transform.position, transform.position) > 20) {
        angle = Vector3.SignedAngle(transform.forward, level.Player.position - transform.position, Vector3.up);
        if (-35f < angle && angle < 35) { // Yes -> roar and run
          status = BearStatus.Buffing;
          anim.Play("Buff");
          anim.SetBool("Run", false);
          anim.SetBool("Move", false);
        }
      }
    }

    if (status == BearStatus.Waiting) {
      waitTime -= Time.deltaTime;
      if (waitTime < 0) {
        status = BearStatus.Walking;
        anim.SetBool("Move", true);
      }
    }
    
    if (status == BearStatus.Chasing) {
      endPos = level.controller.player.position;
      float dist;
      if (Vector3.Distance(transform.position, endPos) < 1) { // Attack
      }
      Vector3 pos = transform.position;
      pos.y = level.Forest.SampleHeight(pos);
      transform.position = pos;

      float walkMultiplier = 1;
      dist = Vector3.Distance(transform.position, startPos);
      if (dist < 2) walkMultiplier = dist * .5f; // If we just started ramp up the speed
      dist = Vector3.Distance(transform.position, endPos);
      if (dist < 2) walkMultiplier = dist * .5f; // If we are close to the target, slow down the speed
      if (walkMultiplier == 0) walkMultiplier = 1;
      anim.speed = walkMultiplier;

      transform.SetPositionAndRotation(
        Vector3.MoveTowards(transform.position, endPos, 2 * walkMultiplier * speed * Time.deltaTime),
        Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(endPos - transform.position), 2.5f * Time.deltaTime));

    }
    

  }


  public void StartChasing() {
    status = BearStatus.Chasing;
    anim.SetBool("Run", true);
    anim.SetBool("Move", false);
  }

  public void AttackCompleted() {
    sounds.clip = WalkSound;
    sounds.loop = true;
    sounds.Play();
  }


  private void OnTriggerEnter(Collider other) {
    int layer = 1 << other.gameObject.layer;
    if (status != BearStatus.Dead && (ArrowMask.value & layer) != 0) { // Have 3 levels of hits, and change the material at every hit
      status = BearStatus.Dead;
      anim.Play("Death");
      gameObject.AddComponent<Rigidbody>();
      Destroy(other.transform.parent.gameObject); // Remove the arrow immediately
      level.KillEnemy(gameObject);
      sounds.clip = DeathSound;
      sounds.loop = false;
      sounds.Play();
    }
  }

}

