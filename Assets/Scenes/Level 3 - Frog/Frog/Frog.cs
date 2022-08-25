using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Frog : MonoBehaviour {
  public Level3 level;
  public Animator anim;
  public Transform BodyCenter;
  public Rigidbody rb;
  public float speed = 5;
  public LayerMask ArrowMask, PlayerMask;
  public AudioSource sounds;
  public AudioClip[] CroackSounds;
  public AudioClip JumpSound;
  public AudioClip landSound;
  public AudioClip DeathSound;
  Vector3 startPos;


  public enum FrogStatus {
    Starting, Waiting, Jump, Land, GoBack, Crush, Death
  };
  public FrogStatus status = FrogStatus.Starting;

  internal void Init(Level3 l, float s, Vector3 spawnPosition) {
    startPos = spawnPosition;
    transform.position = spawnPosition;
    speed = s;
    level = l;
    jumpTime = Random.Range(10f, 15f);
    croack = Random.Range(3f, 15f);
    status = FrogStatus.Starting;
    anim.Play("Start");
  }


  public float dist;
  public float forward = .2f;
  public float up = 3.15f;
  public float jumpTime, playerCheck = 2, croack;
  public float angle;

  private void Update() {
    if (level == null) return;

    // We should jump about randomly, but also if the player is in front of us

    if (status == FrogStatus.Waiting) {
      jumpTime -= Time.deltaTime;
      playerCheck -= Time.deltaTime;
      croack -= Time.deltaTime;
    }
    if (croack < 0) {
      croack = Random.Range(5f, 20f);
      sounds.clip = CroackSounds[Random.Range(0, CroackSounds.Length)];
      sounds.loop = false;
      sounds.Play();
    }
    if (false && jumpTime < 0) {
      jumpTime = Random.Range(15f, 20f);
      Vector3 f = (level.controller.transform.position - transform.position).normalized * forward;
      f.y = f.magnitude * up;
      rb.AddForce(f, ForceMode.Impulse);
      anim.SetTrigger("Jump");
      status = FrogStatus.Jump;
    }
    if (false && playerCheck < 0) {
      playerCheck = Random.Range(.1f, 1f);
      angle = Vector3.SignedAngle(level.controller.transform.position - transform.position, level.Player.position - transform.position, Vector3.up);
      if (-5f < angle && angle < 5f) {
        Vector3 f = (level.Player.position - transform.position).normalized * forward;
        f.y = f.magnitude * up;
        rb.AddForce(f, ForceMode.Impulse);
        anim.SetTrigger("Jump");
        status = FrogStatus.Jump;
      }
    }

    if (Input.GetKeyDown(KeyCode.O)) { // FIXME
      Vector3 force = CalculateJumpForce(level.Player.position, 45f);
      rb.AddForce(force, ForceMode.Impulse);
      anim.SetTrigger("Jump");
      status = FrogStatus.Jump;
      sounds.clip = JumpSound;
      sounds.loop = false;
      sounds.Play();
    }





    if (status == FrogStatus.Jump && rb.velocity.y < 0 && rb.position.y < level.Forest.SampleHeight(rb.position) + 8) {
      status = FrogStatus.Land;
      anim.SetTrigger("Land");
      sounds.clip = landSound;
      sounds.loop = false;
      sounds.Play();
    }

    if (status == FrogStatus.Land) {
      if (Mathf.Abs(rb.velocity.y) < .01f) {
        status = FrogStatus.GoBack;
        StartCoroutine(GoBack());
      }
      if (Physics.CheckSphere(BodyCenter.position, 1, PlayerMask)) {
        status = FrogStatus.Crush;
        level.PlayerDeath(); // Add the blood and use a variant of the death anim for crushing
      }
    }
  }

  Vector3 CalculateJumpForce(Vector3 p, float angle) {
    float gravity = Physics.gravity.magnitude;
    
    angle *= Mathf.Deg2Rad; // Selected angle in radians

    // Positions of this object and the target on the same plane
    Vector3 planarTarget = new(p.x, 0, p.z);
    Vector3 planarPostion = new(transform.position.x, 0, transform.position.z);

    // Planar distance between objects
    float distance = Vector3.Distance(planarTarget, planarPostion);
    // Distance along the y axis between objects
    float yOffset = transform.position.y - p.y;
    float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
    Vector3 velocity = new(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

    // Rotate our velocity to match the direction between the two objects
    float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion);
    Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

    return finalVelocity * rb.mass;
  }


  IEnumerator GoBack() {
    yield return new WaitForSeconds(1);
    anim.Play("Exit");
  }

  public void GoBackCompleted() {
    transform.position = startPos;
    anim.Play("Start");
    status = FrogStatus.Starting;
  }

  public void StartingCompleted() {
    transform.position = startPos;
    status = FrogStatus.Waiting;
  }



  private void OnTriggerEnter(Collider other) {
    int layer = 1 << other.gameObject.layer;
    if (status == FrogStatus.Land && (PlayerMask.value & layer) != 0) {
      status = FrogStatus.Crush;
      level.PlayerDeath();
    }
    if (status != FrogStatus.Death && (ArrowMask.value & layer) != 0) {
      status = FrogStatus.Death;
      anim.Play("Die");
      Destroy(other.transform.parent.gameObject); // Remove the arrow immediately
      level.KillEnemy(gameObject);
      sounds.clip = DeathSound;
      sounds.loop = false;
      sounds.Play();
    }
  }

}