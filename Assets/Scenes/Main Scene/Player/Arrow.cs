using UnityEngine;

public class Arrow : MonoBehaviour {
  public bool initialized = false;
  public Rigidbody rb;
  public Transform ArrowHead;
  private Terrain Ground;
  private Controller Game;


  private void FixedUpdate() {
    if (!initialized) return;

    transform.rotation = Quaternion.LookRotation(rb.velocity);

    if (ArrowHead.position.y < Ground.SampleHeight(ArrowHead.position) + .05f) {
      rb.velocity = Vector3.zero;
      rb.useGravity = false;
      rb.detectCollisions = false;
      initialized = false;
      Destroy(transform.parent.gameObject, 10);
      Game.ArrowHit(ArrowHead.position);
    }
  }

  private void OnTriggerEnter(Collider other) {
    if (!initialized) return;
    Debug.Log("Arrow hit: " + other.gameObject.name);
  }

  internal void Init(Vector3 position, Quaternion rotation, Vector3 velocity, Terrain ground, Controller game) {
    transform.parent.SetPositionAndRotation(position, rotation);
    rb.velocity = velocity;
    Ground = ground;
    Game = game;
    initialized = true;
  }
}
