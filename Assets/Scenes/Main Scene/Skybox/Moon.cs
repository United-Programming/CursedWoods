using UnityEngine;


public class Moon : MonoBehaviour {
  Transform cam;
  public Vector3 direction;
  public float distance;
  public float scale;
  Vector3 scalev;

  private void Start() {
    cam = Camera.main.transform;
    scalev = new Vector3(scale, scale, scale);
    transform.localScale = scalev;
  }
  private void Update() {
    transform.position = cam.position + direction * distance;
    transform.LookAt(cam.position);
    scalev.x = scale;
    scalev.y = scale;
    transform.localScale = scalev;
  }
}