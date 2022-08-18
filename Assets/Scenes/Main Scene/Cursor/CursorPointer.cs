using UnityEngine;

public class CursorPointer : MonoBehaviour {
  public Camera cam;
  public LayerMask cursorLayer;
    private void Update() {
    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit, 10, cursorLayer)) {
      transform.position = hit.point;
    }
    transform.LookAt(cam.transform.position);
  }
}
