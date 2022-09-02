using UnityEngine;

public class Orb : MonoBehaviour {

  public MeshRenderer mr;
  public Gradient colors;
  public LayerMask ArrowMask;
  Level6 level;
  Ghost ghost;
  float t, live;
  Vector2 toff;
  Vector3 center;
  internal void Init(Level6 l, Ghost g) {
    level = l;
    ghost = g;
    center = l.GetLevelCenter();
    float angle = ghost.angle + Mathf.PI;
    Vector3 pos = center + new Vector3(Mathf.Sin(angle) * Random.Range(34f, 37f), 0, Mathf.Cos(angle) * Random.Range(34f, 37f));
    pos.y = l.Forest.SampleHeight(pos) + 2f;
    transform.position = pos;
  }

  float disappearing = 0;
  private void Update() {
    if (level == null) return;

    t += Time.deltaTime * .1f;
    if (t > 1) t -= 1;
    mr.material.SetColor("_EmissionColor", colors.Evaluate(t));

    toff.x = Mathf.Sin(Mathf.PI * (Time.time * 2 + toff.y) * .2f) * 3;
    toff.y = Mathf.Cos(Mathf.PI * (Time.time * .1f) * .1f) * 2 + toff.x * .5f;
    mr.material.SetTextureOffset("_BaseMap", toff);

    live += Time.deltaTime;

    if (disappearing > 0) {
      disappearing -= Time.deltaTime;
      transform.localScale = disappearing * .375f * Vector3.one;
      if (disappearing <= 0) {
        // Respawn at random position (glow to zero and make it disappear, then add new position and restart glowing)
        float angle = ghost.angle + Random.Range(Mathf.PI - .5f, Mathf.PI + .5f);
        Vector3 pos = center + new Vector3(Mathf.Sin(angle) * Random.Range(34f, 37f), 0, Mathf.Cos(angle) * Random.Range(34f, 37f));
        pos.y = level.Forest.SampleHeight(pos) + 4f;
        transform.position = pos;
        transform.localScale = Vector3.one * .75f;
        live = 0;
      }
    }
    else if (Vector3.Distance(center, transform.position) > 38 || live > 15) { // Change location after a while or when too far away from the center
      disappearing = 2;
      live = 0;
    }

    if (mr.isVisible) {
      ghost.Call(transform.position);
    }

  }

  private void OnTriggerEnter(Collider other) {
    int layer = 1 << other.gameObject.layer;

    if ((ArrowMask.value & layer) != 0) {
      // Damage the Ghost
      ghost.TakeDamage();
      disappearing = 2;
    }
  }

}