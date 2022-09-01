using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "GhostPostProcessingMaterialInstance", menuName = "Game/GhostPostProcessingMaterialInstance")]
public class GhostPostProcessingMaterialInstance : ScriptableObject {
  //---Your Materials---
  public static Material GhostPostProcessMaterial;

  public static Material GetMaterial {
    get {
      if (GhostPostProcessMaterial != null) return GhostPostProcessMaterial;
      GhostPostProcessMaterial = Resources.Load("CusedWoods_GhostPostProcessMaterial") as Material;
      return GhostPostProcessMaterial;
    }
  }
}