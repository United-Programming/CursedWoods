using UnityEngine;

public class Arrow : MonoBehaviour {
  public bool initialized = false;
  public Rigidbody rb;


  private void FixedUpdate() {
    if (!initialized) return;

    Vector3 vel = rb.velocity;
    transform.rotation = Quaternion.LookRotation(vel);
    float vely = vel.y;
    vel.y = 0;
    float magn = vel.magnitude;
    magn *= 1 - 1.5f * Time.fixedDeltaTime;
    vel = vel.normalized * magn;
    vel.y = vely;
    rb.velocity = vel; ;

    if (transform.position.y < -2) {
      Destroy(gameObject);
    }
  }

  private void OnTriggerEnter(Collider other) {
    if (!initialized) return;
    Debug.Log("Arrow hit: " + other.gameObject.name);
  }

  internal void Init(Vector3 position, Quaternion rotation, Vector3 velocity) {
    transform.parent.SetPositionAndRotation(position, rotation);
    rb.velocity = velocity;
    initialized = true;
  }
}
