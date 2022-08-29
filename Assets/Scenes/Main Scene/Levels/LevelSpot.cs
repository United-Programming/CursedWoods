using UnityEngine;
using UnityEditor;

public class LevelSpot : MonoBehaviour {
  public Terrain Ground;
}

[CustomEditor(typeof(LevelSpot))]
[CanEditMultipleObjects]
class LevelSpotEditor : Editor {
  public override void OnInspectorGUI() {
    DrawDefaultInspector();
    if (GUILayout.Button("Set to ground")) {
      LevelSpot t = target as LevelSpot;
      Vector3 pos = t.transform.position;
      pos.y = t.Ground.SampleHeight(pos) + .2f;
      t.transform.position = pos;
    }
    if (GUILayout.Button("Set to ground + .5")) {
      LevelSpot t = target as LevelSpot;
      Vector3 pos = t.transform.position;
      pos.y = t.Ground.SampleHeight(pos) + .7f;
      t.transform.position = pos;
    }
    if (GUILayout.Button("Set to ground + .75")) {
      LevelSpot t = target as LevelSpot;
      Vector3 pos = t.transform.position;
      pos.y = t.Ground.SampleHeight(pos) + .95f;
      t.transform.position = pos;
    }
  }
}
