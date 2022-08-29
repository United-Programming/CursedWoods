using UnityEngine;

public class DisableInGame : MonoBehaviour {
  private void Awake() {
    gameObject.SetActive(false);
  }
}