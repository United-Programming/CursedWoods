using UnityEngine;

public class Controller : MonoBehaviour {


  public Animator anim;
  public float walkDir = 0;
  public float angle = 0;

  private void Update() {
    float x = Input.GetAxis("Horizontal");
    if (x == 0) walkDir *= 1 - 25 * Time.deltaTime;
    else {
      walkDir += x * Time.deltaTime * 2;
      walkDir = Mathf.Clamp(walkDir, -1f, 1f);
    }
    anim.SetFloat("Movement", walkDir);

    if (Mathf.Abs(walkDir) > .1f) {
      angle += walkDir * .1f;
      transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, angle, 0), 10 * Time.deltaTime);
    }
  }



}