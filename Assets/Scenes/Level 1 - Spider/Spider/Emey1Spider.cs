using UnityEngine;

public class Emey1Spider : MonoBehaviour {
  public Level1 controller;
  public Rigidbody rb;
  public Animator anim;

  public float speed = 10;

  private void Update() {
    // Are we far away from player? Destroy and tell controller to respawn
    if (Vector3.Distance(transform.position, controller.Player.position) > 135) {
      controller.DestroyEnemyAndRespawn(gameObject);
      Destroy(gameObject);
    }

    // Move to the player up to half the screen
    Vector3 dir = (controller.Player.position - transform.position).normalized + controller.CenterOfWorld.cam.transform.forward * .5f;
    Vector3 pos = transform.position;
    pos.y = controller.Forest.SampleHeight(pos);
    transform.position = pos;
    transform.rotation.SetLookRotation(dir);
    rb.MovePosition(transform.position + speed * Time.deltaTime * dir);
    anim.SetBool("Run", true);

  }


  /*
  
  After half of screen if player is aiming to us then flee
  If player is not aiming to us go close and bite
   If hit by arrow die adn notify the controlelr
   
   */

}