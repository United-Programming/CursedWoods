using UnityEngine;

public class CursorPointer : MonoBehaviour {
  public Camera cam;
  public LayerMask cursorLayer;
  public Vector3 cursorPosition;
  public bool aimingCursor;
  private void Update() {
    if (aimingCursor) {
      transform.position = cursorPosition;
    }
    else {
      Vector2 mp = Input.mousePosition;
      Ray ray = cam.ScreenPointToRay(mp);
      if (Physics.Raycast(ray, out RaycastHit hit, 10, cursorLayer)) {
        transform.position = hit.point;
      }
    }
    transform.LookAt(cam.transform.position);
  }
}
