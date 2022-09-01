using UnityEngine.Rendering.Universal;

[System.Serializable]
public class GhostPostProcessRenderer : ScriptableRendererFeature {
  GhostPostProcessPass pass;

  public override void Create() {
    pass = new GhostPostProcessPass();
  }

  public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
    renderer.EnqueuePass(pass);
  }
}
