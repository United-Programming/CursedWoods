using Unity.VisualScripting;
using UnityEngine;

public class Enemy1Spider : MonoBehaviour {
  public Level1 controller;
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

  private void Update() {
    if (dead || controller == null) return;

    // Are we far away from player? Destroy and tell controller to respawn
    float dist = Vector3.Distance(transform.position, controller.Player.position);
    if (dist > 135) {
      controller.DestroyEnemyAndRespawn(gameObject, false);
      Destroy(gameObject);
    }

    if (attack) {
      if (Physics.CheckSphere(BodyCenter.position, .25f, PlayerMask)) {
        controller.PlayerDeath();
        dead = true;
      }
      return;
    }

    // Move to the player
    if (dist < 2f && !attack) {
      transform.rotation = Quaternion.LookRotation(transform.position - controller.Player.position, Vector3.up);
      anim.SetTrigger("Attack");
      anim.SetBool("Run", false);
      attack = true;
    }
    else if (controller.CenterOfWorld.aiming && controller.CenterOfWorld.arrowLoaded) { // Is the player is aiming?
      // Flee
      Vector3 dir = (transform.position + controller.CenterOfWorld.cam.transform.forward * 2.5f - controller.Player.position).normalized;
      Vector3 pos = transform.position;
      pos += dir;
      pos.y = controller.Forest.SampleHeight(pos);
      transform.SetPositionAndRotation(
        Vector3.Lerp(transform.position, pos, speed * .8f * Time.deltaTime), 
        Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(controller.Player.position - transform.position, Vector3.up), Time.deltaTime));
      anim.SetBool("Run", true);
      if (sounds.clip != WalkSound || !sounds.isPlaying) {
        sounds.clip = WalkSound;
        sounds.loop = true;
        sounds.Play();
      }
    }
    else {
      transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.position - controller.Player.position, Vector3.up), 4 * Time.deltaTime);
      Vector3 dir = (controller.Player.position + controller.CenterOfWorld.cam.transform.forward * 1.75f - transform.position).normalized;
      Vector3 pos = transform.position;
      pos += dir;
      pos.y = controller.Forest.SampleHeight(pos);
      float speedMultiplier = (dist - 2) * .05f + 1;
      transform.position = Vector3.Lerp(transform.position, pos, speedMultiplier * speed * Time.deltaTime);
      anim.SetBool("Run", true);
      if (sounds.clip != WalkSound || !sounds.isPlaying) {
        sounds.clip = WalkSound;
        sounds.loop = true;
        sounds.Play();
      }
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
      Destroy(other.gameObject); // Remove the arrow immediately
      controller.DestroyEnemyAndRespawn(gameObject, true);
      sounds.clip = DeathSound;
      sounds.loop = false;
      sounds.Play();
    }
  }

}