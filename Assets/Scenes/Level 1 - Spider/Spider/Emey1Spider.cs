using UnityEngine;

public class Emey1Spider : MonoBehaviour {
  public Level1 controller;
  public Animator anim;
  public Transform BodyCenter;
  public float speed = 5;
  public LayerMask ArrowMask, PlayerMask;
  bool dead = false;
  public bool attack = false;

  private void Update() {
    if (dead) return;

    // Are we far away from player? Destroy and tell controller to respawn
    float dist = Vector3.Distance(transform.position, controller.Player.position);
    if (dist > 135) {
      controller.DestroyEnemyAndRespawn(gameObject);
      Destroy(gameObject);
    }

    if (attack) {
      if (Physics.CheckSphere(BodyCenter.position, .25f, PlayerMask)) {
        controller.PlayerDeath();
        Debug.Log("Spider hit player");
        dead = true;
      }
      return;
    }

    // Move to the player
    if (dist < 2f && !attack) {
      transform.rotation = Quaternion.LookRotation(transform.position - controller.Player.position, Vector3.up);
      anim.SetTrigger("Attack");
      anim.SetBool("Run", false);
      anim.SetBool("Move", false);
      attack = true;
    }
    else if (controller.CenterOfWorld.aiming && controller.CenterOfWorld.arrowLoaded) { // Is the player is aiming?
      // Flee
      Vector3 dir = (transform.position + controller.CenterOfWorld.cam.transform.forward * 2.5f - controller.Player.position).normalized;
      Vector3 pos = transform.position;
      pos += dir;
      pos.y = controller.Forest.SampleHeight(pos);
      transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(controller.Player.position - transform.position, Vector3.up), Time.deltaTime);
      transform.position = Vector3.Lerp(transform.position, pos, speed * .8f * Time.deltaTime);
      anim.SetBool("Move", true);
      anim.SetBool("Run", false);
    }
    else {
      transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.position - controller.Player.position, Vector3.up), 4 * Time.deltaTime);
      Vector3 dir = (controller.Player.position + controller.CenterOfWorld.cam.transform.forward * 1.5f - transform.position).normalized;
      Vector3 pos = transform.position;
      pos += dir;
      pos.y = controller.Forest.SampleHeight(pos);
      transform.position = Vector3.Lerp(transform.position, pos, speed * Time.deltaTime);
      anim.SetBool("Run", true);
      anim.SetBool("Move", false);
    }
  }

  public void AttackCompleted() {
    attack = false;
  }

  private void OnTriggerEnter(Collider other) {
    int layer = 1 << other.gameObject.layer;
    if ((ArrowMask.value & layer) != 0) {
      dead = true;
      anim.Play("Die");
      controller.DestroyEnemyAndRespawn(gameObject);
      Debug.Log("Spider hit by arrow");
    }
  }


  /*
  
  After half of screen if player is aiming to us then flee
  If player is not aiming to us go close and bite
   If hit by arrow die adn notify the controlelr
   
   */

}