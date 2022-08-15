using UnityEngine;

public class Controller : MonoBehaviour {


  public Animator anim;
  public float walkDir = 0;


  private void Update() {
    float x = Input.GetAxis("Horizontal");
    if (x == 0) walkDir *= 1 - 5 * Time.deltaTime;
    else {
      walkDir += x * Time.deltaTime * 2;
      walkDir = Mathf.Clamp(walkDir, -1f, 1f);
    }
    anim.SetFloat("Movement", walkDir);
  }



}