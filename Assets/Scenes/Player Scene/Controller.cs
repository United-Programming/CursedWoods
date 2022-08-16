using UnityEngine;

public class Controller : MonoBehaviour {

  public Transform player;
  public GameObject Bow;
  public Animator anim;


  public float pangle = 0;


  public float movement = 0;
  public float angle = 0;
  public float speed = .05f;
  public bool aiming = false;

  private void Update() {

    /*
     not aiming: lr will just change the rotation
     aiming: we move and run
     
    when moving to a direction:
      start by chaning the player rotation and start playing slowly the run anim. This for max .2 seconds
      the start rotating the camera in the direction of the player rotation
     
     
     */

    if (Input.GetKeyDown(KeyCode.Space)) aiming = true; // FIXME here we should load the arrow with right mb or aim with rmb in case arrow is loaded, and shoot with lmb
    if (Input.GetKeyUp(KeyCode.Space)) aiming = false;

    float x = Input.GetAxis("Horizontal");
    if (x == 0) { // not moving
      // just keep the player angle and stop the run anim, but do not change the player rotation
      // Stop the rotation of the camera
      anim.SetBool("Moving", false);
      anim.SetBool("Run", false);
      movement = 0;
      angle = transform.rotation.eulerAngles.y;
      anim.speed = 1;
    }
    else { // Moving, change first player rotation, then move the camera
      pangle = Mathf.LerpAngle(pangle, x > 0 ? 90 : -90, 6 * Time.deltaTime);
      float ya = player.localEulerAngles.y;
      float dist = Mathf.Abs(pangle - ya);

      player.localRotation = Quaternion.Euler(0, Mathf.Lerp(ya, pangle, dist * 10 * Time.deltaTime), 0);
      float absPAngle = Mathf.Abs(pangle);
      anim.SetBool("Moving", absPAngle > 5);
      

      if (absPAngle > 85) {
        anim.SetBool("Run", true);
        float mult = Mathf.Sign(movement) == Mathf.Sign(x) ? 1.5f : 10f;
        anim.speed = .25f + Mathf.Abs(movement);
        movement += x * Time.deltaTime * mult;
        movement = Mathf.Clamp(movement, -1f, 1f);
        angle += movement * speed;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, angle, 0), 6 * Time.deltaTime);
      }
      else anim.SetBool("Run", false);
    }
  }
}