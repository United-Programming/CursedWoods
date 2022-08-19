using UnityEngine;

public class Arrow : MonoBehaviour {
  public bool initialized = false;
  public Rigidbody rb;
  public Transform ArrowHead;
  private Terrain ground;


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

    if (ArrowHead.position.y < ground.SampleHeight(ArrowHead.position) + .05f) {
      rb.velocity = Vector3.zero;
      rb.useGravity = false;
      rb.detectCollisions = false;
      initialized = false;
      //Destroy(transform.parent.gameObject, 3);
    }
  }

  private void OnTriggerEnter(Collider other) {
    if (!initialized) return;
    Debug.Log("Arrow hit: " + other.gameObject.name);
  }

  internal void Init(Vector3 position, Quaternion rotation, Vector3 velocity, Terrain ground) {
    transform.parent.SetPositionAndRotation(position, rotation);
    rb.velocity = velocity;
    initialized = true;
    this.ground = ground;
  }
}
