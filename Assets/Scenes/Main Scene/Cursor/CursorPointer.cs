using UnityEngine;

public class CursorPointer : MonoBehaviour {
  public Camera cam;
  public LayerMask cursorLayer;
  private void Update() {
    Vector2 mp = Input.mousePosition;
    Cursor.visible = (mp.x < 0 || mp.y < 0 || mp.x > Screen.width || mp.y > Screen.height);

    Ray ray = cam.ScreenPointToRay(mp);
    if (Physics.Raycast(ray, out RaycastHit hit, 10, cursorLayer)) {
      transform.position = hit.point;
    }
    transform.LookAt(cam.transform.position);
  }
}
