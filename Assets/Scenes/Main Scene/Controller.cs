using UnityEngine;
using UnityEngine.Rendering;

public class Controller : MonoBehaviour {

  public Transform player;
  public GameObject ArrowPlayer;
  public GameObject Arrow;
  public Animator anim;


  public float playerTargetAngle = 0; // public just to debug
  public float movement = 0; // public just to debug
  public float angle = 0; // public just to debug
  public float speed = .05f; // public just to debug
  public bool aiming = false; // public just to debug

  private void Update() {
    if (Input.GetMouseButtonDown(1)) { // Change aiming/no-aiming if we press the right mouse buton
      aiming = !aiming;
      anim.SetBool("Aim", aiming);
    }

    float x = Input.GetAxis("Horizontal");
    if (x != 0) { // If we move we cannto be aiming
      aiming = false; 
      anim.SetBool("Aim", false);
    }

    if (x == 0 || aiming) { // not moving
      // just keep the player angle and stop the run anim, but do not change the player rotation
      // Stop the rotation of the camera
      anim.SetBool("Moving", false);
      anim.SetBool("Run", false);
      movement = 0;
      angle = transform.rotation.eulerAngles.y;
      anim.speed = 1;
    }
    else { // Moving, change first player rotation, then move the camera
      // Align the local rotation of the player to the movement
      playerTargetAngle = Mathf.LerpAngle(playerTargetAngle, x > 0 ? 90 : -90, 6 * Time.deltaTime);
      float playerCurrentAngle = player.localEulerAngles.y;
      float dist = Mathf.Abs(playerTargetAngle - playerCurrentAngle);
      player.localRotation = Quaternion.Euler(0, Mathf.Lerp(playerCurrentAngle, playerTargetAngle, dist * 10 * Time.deltaTime), 0);

      // Depending on the angle magnitude, set the movement anim
      float absPAngle = Mathf.Abs(playerTargetAngle);
      anim.SetBool("Moving", absPAngle > 5);
      
      if (absPAngle > 85) {
        anim.SetBool("Run", true);
        float mult = Mathf.Sign(movement) == Mathf.Sign(x) ? 1.5f : 10f;
        anim.speed = .25f + Mathf.Abs(movement);
        movement += x * Time.deltaTime * mult;
        movement = Mathf.Clamp(movement, -1f, 1f);

        // Notate the camera and the player around the world
        angle += movement * speed;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, angle, 0), 6 * Time.deltaTime);
      }
      else anim.SetBool("Run", false);
    }

    if (aiming) {
      float aimH = 2f * (Input.mousePosition.x - Screen.width * .5f) / Screen.width;
      float aimV = 2f * (Input.mousePosition.y - Screen.height * .5f) / Screen.height;
      anim.SetFloat("AimH", aimH);
      anim.SetFloat("AimV", aimV);

      // When aiming, the local rotation of the player should be looking to the center, and move pretty quick
      playerTargetAngle = Mathf.LerpAngle(playerTargetAngle, aimH * 45, 8 * Time.deltaTime);
      float playerCurrentAngle = player.localEulerAngles.y;
      float dist = Mathf.Abs(playerTargetAngle - playerCurrentAngle);
      player.localRotation = Quaternion.Euler(0, Mathf.Lerp(playerCurrentAngle, playerTargetAngle, dist * 15 * Time.deltaTime), 0);
    }

  }

  private void OnApplicationFocus(bool focus) {
    Cursor.visible = !focus;
  }

}