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
  public AudioClip jumpSound;
  public AudioClip landSound;
  public AudioClip DeathSound;
  Vector3 startPos;


  public enum FrogStatus {
    Starting, Waiting, Jump, Land, GoBack, Crush, Death
  };
  public FrogStatus status = FrogStatus.Starting;
  float lastStatusChange = 0;

  internal void Init(Level3 l, float s, Vector3 spawnPosition) {
    startPos = spawnPosition;
    transform.position = spawnPosition;
    transform.LookAt(l.controller.transform.position, Vector3.up);
    speed = s;
    level = l;
    jumpTime = Random.Range(3f, 10f);
    croack = Random.Range(3f, 15f);
    status = FrogStatus.Starting;
    lastStatusChange = 0;
    anim.Play("Start");
  }


  public float forward = .2f;
  public float up = 3.15f;
  public float jumpTime, playerCheck = 2, croack;


  public float da, dd;

  private void Update() {
    if (level == null) return;
    lastStatusChange += Time.deltaTime;
    if (lastStatusChange > 15 && status != FrogStatus.Death && status != FrogStatus.Waiting) { // To reset in case something went wrong with rigidbody collisions
      rb.velocity = Vector3.zero;
      transform.position = startPos;
      status = FrogStatus.Waiting;
      lastStatusChange = 0;
      anim.Play("Idle");
      transform.LookAt(level.controller.transform.position, Vector3.up);
    }

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
    if (jumpTime < 0) {
      jumpTime = Random.Range(5f, 10f);
      Vector3 force = CalculateJumpForce(level.controller.transform.position + (transform.position - level.controller.transform.position).normalized * 21);
      rb.AddForce(force, ForceMode.Impulse);
      anim.SetTrigger("Jump");
      sounds.clip = jumpSound;
      sounds.Play();
      status = FrogStatus.Jump;
      lastStatusChange = 0;
    }
    if (playerCheck < 0) {
      playerCheck = Random.Range(.5f, 2f);
      float angle = Vector3.SignedAngle(level.controller.transform.position - transform.position, level.Player.position - transform.position, Vector3.up);
      float dist = Vector3.Distance(level.Player.position, transform.position);
      if (-7f < angle && angle < 7f && 35 < dist && dist < 45) {
        dd = dist;
        da = angle;
        Vector3 force = CalculateJumpForce(level.Player.position);
        rb.AddForce(force, ForceMode.Impulse);
        anim.SetTrigger("Jump");
        sounds.clip = jumpSound;
        sounds.Play();
        status = FrogStatus.Jump;
        lastStatusChange = 0;
      }
    }


    if (status == FrogStatus.Jump && rb.velocity.y < 0 && rb.position.y < level.Forest.SampleHeight(rb.position) + 8) {
      status = FrogStatus.Land;
      lastStatusChange = 0;
      anim.SetTrigger("Land");
      sounds.clip = landSound;
      sounds.loop = false;
      sounds.Play();
    }

    if (status == FrogStatus.Land) {
      if (Mathf.Abs(rb.velocity.y) < .01f) {
        status = FrogStatus.GoBack;
        lastStatusChange = 0;
        StartCoroutine(GoBack());
      }
      if (Physics.CheckSphere(BodyCenter.position, 1, PlayerMask)) {
        status = FrogStatus.Crush;
        lastStatusChange = 0;
        level.PlayerDeath(); // Add the blood and use a variant of the death anim for crushing
      }
    }

  }

  Vector3 CalculateJumpForce(Vector3 p) {
    Vector3 sp = transform.position;
    Vector3 dp = p;
    sp.y = 0;
    dp.y = 0;
    Vector3 direction = ((dp - sp).normalized + Vector3.up).normalized;
    float dist = (p - transform.position).magnitude;
    float force = .00004464676f * dist * dist - 0.00892161f * dist + 0.7800361f;
    return force * dist * rb.mass * direction;
  }


  IEnumerator GoBack() {
    yield return new WaitForSeconds(1);
    anim.Play("Exit");
  }

  public void GoBackCompleted() {
    transform.position = startPos;
    transform.LookAt(level.controller.transform.position, Vector3.up);
    anim.Play("Start");
    status = FrogStatus.Starting;
    lastStatusChange = 0;
  }

  public void StartingCompleted() {
    transform.position = startPos;
    status = FrogStatus.Waiting;
    lastStatusChange = 0;
  }



  private void OnTriggerEnter(Collider other) {
    int layer = 1 << other.gameObject.layer;
    if ((status == FrogStatus.Land || rb.velocity.sqrMagnitude > .2f) && (PlayerMask.value & layer) != 0) {
      status = FrogStatus.Crush;
      lastStatusChange = 0;
      level.PlayerDeath();
    }
    if (status != FrogStatus.Death && (ArrowMask.value & layer) != 0) {
      status = FrogStatus.Death;
      lastStatusChange = 0;
      anim.Play("Die");
      Destroy(other.transform.parent.gameObject); // Remove the arrow immediately
      level.KillEnemy(gameObject);
      sounds.clip = DeathSound;
      sounds.loop = false;
      sounds.Play();
      rb.velocity *= .75f;
    }
  }

}