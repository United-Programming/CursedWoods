using UnityEngine;

public class Level1 : MonoBehaviour {
  public Transform Player;
  public Controller CenterOfWorld;
  public GameObject SpiderPrefab;
  public Terrain Forest;

  public float Horiz = 5;
  public float Dist = 1.5f;

  GameObject spider;

  internal void DestroyEnemyAndRespawn(GameObject enemy) {
    Debug.Log("Destroying: " + enemy.name);
  }

  internal void PlayerDeath() {
    CenterOfWorld.PlayerDeath();
  }

  private void Start() {
    Forest = GameObject.FindObjectOfType<Terrain>();
    CenterOfWorld = GameObject.FindObjectOfType<Controller>();
    Player = CenterOfWorld.transform.GetChild(1);
    return;

    Vector3 spawnPosition = Dist * Player.position - CenterOfWorld.transform.position + Random.insideUnitSphere * Random.Range(1f, 2f) + (Random.Range(0, 2) * 2 - 1) * Horiz * Player.right;
    spawnPosition.y = Forest.SampleHeight(spawnPosition);

    spider = Instantiate(SpiderPrefab, spawnPosition, Quaternion.LookRotation(spawnPosition - Player.position));
  }

  private void Update() {
    if (Input.GetKeyDown(KeyCode.Space)) {
      Destroy(spider);
      Vector3 spawnPosition = Dist * Player.position - CenterOfWorld.transform.position + Random.insideUnitSphere * Random.Range(1f, 2f) + (Random.Range(0, 2) * 2 - 1) * Horiz * Player.right;
      spawnPosition.y = Forest.SampleHeight(spawnPosition);
      spider = Instantiate(SpiderPrefab, spawnPosition, Quaternion.LookRotation(spawnPosition - Player.position));
    }
  }
}