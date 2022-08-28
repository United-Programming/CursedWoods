using System.Collections;
using UnityEngine;

public class Fog : MonoBehaviour {
  public SpriteRenderer[] fogs;
  public Vector3[] origin;
  public Vector3[] startPos;
  public Vector3[] endPos;
  public float[] delays;
  public float[] dists;
  public bool fogEnabled = false;

  private void Start() {
    origin = new Vector3[fogs.Length];
    startPos = new Vector3[fogs.Length];
    endPos = new Vector3[fogs.Length];
    delays = new float[fogs.Length];
    dists = new float[fogs.Length];
    for (int i = 0; i < fogs.Length; i++) {
      origin[i] = fogs[i].transform.localPosition;
    }
    Disable();
  }

  public void Disable() {
    Color32 c = new Color32(1, 1, 1, 0);
    foreach (var f in fogs) {
      f.enabled = false;
      f.color = c;
    }
    fogEnabled = false;
  }

  public void EnableFog(float level) {
    StartCoroutine(EnableFogCoroutine(level));
  }

  IEnumerator EnableFogCoroutine(float level) {
    float a = fogs[0].color.a;
    yield return null;

    foreach (var f in fogs) f.enabled = true;
    if (a < level) {
      while (a < level) {
        a += Time.deltaTime;
        Color32 c = new Color32(255, 255, 255, (byte)(255 * a));
        foreach (var f in fogs) f.color = c;
        yield return null;
      }
    }
    else {
      while (a > level) {
        a -= Time.deltaTime;
        Color32 c = new Color32(255, 255, 255, (byte)(255 * a));
        foreach (var f in fogs) f.color = c;
        yield return null;
      }
    }
    fogEnabled = level != 0;
  }

  private void Update() {
    if (!fogEnabled) return;

    for (int i = 0; i < fogs.Length; i++) {
      delays[i] -= Time.deltaTime;
      if (delays[i] < 0) {
        startPos[i] = fogs[i].transform.localPosition;
        endPos[i] = Random.insideUnitSphere * Random.Range(.05f, .2f);
        endPos[i].y *= .5f;
        endPos[i] += origin[i];
        delays[i] = 3f + Random.Range(.1f, 2f);
        dists[i] = delays[i];
      }
      // We need the original pos + this variation

      float ease = (dists[i] - delays[i]) / dists[i];
      if (ease < .5f) {
        ease = 4 * ease * ease * ease;
      }
      else {
        ease = (-2 * ease + 2);
        ease = ease * ease * ease;
        ease = 1 - ease * .5f;
      }
      fogs[i].transform.localPosition = Vector3.Lerp(startPos[i], endPos[i], ease);
    }
  }

}