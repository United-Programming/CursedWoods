using UnityEngine;

public class Bee : MonoBehaviour {
  public Level2 level;
  public Animator anim;
  public Transform BodyCenter;
  public float speed = 5;
  public LayerMask ArrowMask, PlayerMask;
  bool dead = false;
  public bool attack = false;
  public AudioSource sounds;
  public AudioClip WalkSound;
  public AudioClip AttakSound;
  public AudioClip DeathSound;
  Vector3 startPos, endPos;

  private void Start() {
    startPos = transform.position;
    endPos = startPos + Random.Range(-2f, 2f) * Vector3.forward + Random.Range(-2f, 2f) * Vector3.right + Random.Range(-.5f, .5f) * Vector3.up;
  }
  public float dist;
  private void Update() {
    if (level == null) return;
    if (dead) {
      float vdist = transform.position.y - level.Forest.SampleHeight(transform.position) + .2f;
      if (vdist < 0) {
        DestroyImmediate(GetComponent<Rigidbody>());
        transform.position -= Vector3.up * vdist;
      }
      return;
    }

    dist = Vector3.Distance(transform.position, level.Player.position);
    float dest = Vector3.Distance(transform.position, endPos);
    if (dest < .5f && !attack) {
      if (dist > 10) { // Random movement
        endPos = startPos + Random.Range(-5f, 5f) * Vector3.forward + Random.Range(-5f, 5f) * Vector3.right + Random.Range(-.5f, .5f) * Vector3.up;
        anim.SetBool("Run", true);
      }
      else if (dist > 3.5f) { // Random but try to go closer to the player
        endPos = level.Player.position + Random.Range(-2f, 2f) * Vector3.forward + Random.Range(-2f, 2f) * Vector3.right + Random.Range(1.15f, 1.25f) * Vector3.up;
        Vector3 playerPos = level.Player.position + (transform.position - level.Player.position).normalized * 1.5f;
        Vector3 playerForwardPos = level.Player.position + (level.Player.position - level.controller.transform.position).normalized * 2.5f;
        endPos = (3 * playerPos + 2 * playerForwardPos + endPos) * .1666667f;
        endPos.y = level.Forest.SampleHeight(endPos) + Random.Range(1.15f, 1.35f);
        anim.SetBool("Run", true);
      }
      else if (dist > 2f) { // Prepare for attak
        endPos = level.Player.position + (transform.position - level.Player.position).normalized * 1.9f;
        endPos.y = level.Forest.SampleHeight(endPos) + Random.Range(1.5f, 2f);
        anim.SetBool("Run", true);
      }
      else {
        anim.SetTrigger("Attack");
        anim.SetBool("Run", false);
        attack = true;
      }
    }
    if (attack && dist > 5) {
      attack = false;
      endPos = transform.position;
    }
    Quaternion look = (endPos == transform.position) ? transform.rotation : Quaternion.Euler(0, Quaternion.LookRotation(endPos - transform.position, Vector3.up).eulerAngles.y, 0);

    transform.SetPositionAndRotation(
      Vector3.Lerp(transform.position, endPos, Time.deltaTime * .2f * speed),
      Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 15));

    if (attack) {
      look = Quaternion.Euler(0, Quaternion.LookRotation(level.Player.position - transform.position, Vector3.up).eulerAngles.y, 0);
      transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 25);
      if (Physics.CheckSphere(BodyCenter.position, .25f, PlayerMask)) {
        level.PlayerDeath();
        dead = true;
      }
      return;
    }
  }

  public void AttackCompleted() {
    attack = false;
  }

  public void PlayAttackSound() {
    sounds.clip = AttakSound;
    sounds.loop = false;
    sounds.Play();
  }

  private void OnTriggerEnter(Collider other) {
    int layer = 1 << other.gameObject.layer;
    if (!dead && (ArrowMask.value & layer) != 0) {
      dead = true;
      anim.Play("Die");
      gameObject.AddComponent<Rigidbody>();
      Destroy(other.transform.parent.gameObject); // Remove the arrow immediately
      level.KillEnemy(gameObject);
      sounds.clip = DeathSound;
      sounds.loop = false;
      sounds.Play();
    }
  }

}