using UnityEngine;

public class Controller : MonoBehaviour {

  public Transform player;
  public Animator anim;
  public float walkDir = 0;
  public float angle = 0;
  public float speed = .05f;

  private void Update() {
    float x = Input.GetAxis("Horizontal");
    if (x == 0) walkDir *= 1 - 25 * Time.deltaTime;
    else {
      walkDir += x * Time.deltaTime * .5f;
      walkDir = Mathf.Clamp(walkDir, -1f, 1f);
    }
    float absSpeed = Mathf.Abs(walkDir);
    anim.SetBool("Run", absSpeed > .01f);
    anim.speed = Mathf.Clamp(absSpeed, .5f, 1);

    if (absSpeed > .01f) {
      angle += walkDir * speed;
      transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, angle, 0), 6 * Time.deltaTime);
      player.localRotation = Quaternion.Lerp(player.localRotation, Quaternion.Euler(0, walkDir < 0 ? -90 : 90, 0), 6 * Time.deltaTime);
    }
    else {
      anim.speed = Mathf.Lerp(anim.speed, 1, Time.deltaTime * 10);
      player.localRotation = Quaternion.Lerp(player.localRotation, Quaternion.Euler(0, 0, 0), 10 * Time.deltaTime);
    }
  }



}