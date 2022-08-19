using System.Collections;
using UnityEngine;

public class Level1 : MonoBehaviour {
  public Transform Player;
  public Controller CenterOfWorld;
  public GameObject SpiderPrefab;
  public Terrain Forest;

  public float Horiz = 5;
  public float Dist = 1.5f;
  public int toWin = 5;

  GameObject spider;

  internal void DestroyEnemyAndRespawn(GameObject enemy, bool killedByPlayer) {
    toWin--;
    if (toWin == 0) {
      // Destroy spider immediate and play win dance and music.
      if (spider!=null) Destroy(spider);
      spider = null;
      CenterOfWorld.PlayWinDance();
    }
    else
      StartCoroutine(DestroyAndRespawnAsync());
  }
  IEnumerator DestroyAndRespawnAsync() {
    yield return new WaitForSeconds(Random.Range(2f, 5f));
    SpawnEnemy();
  }

  internal void PlayerDeath() {
    CenterOfWorld.PlayerDeath();
  }

  private void Start() {
    Forest = GameObject.FindObjectOfType<Terrain>();
    CenterOfWorld = GameObject.FindObjectOfType<Controller>();
    Player = CenterOfWorld.transform.GetChild(1);

    SpawnEnemy();
  }


  void SpawnEnemy() {
    if (spider != null) Destroy(spider);
    Vector3 spawnPosition = Dist * Player.position - CenterOfWorld.transform.position + Random.insideUnitSphere * Random.Range(1f, 2f) + (Random.Range(0, 2) * 2 - 1) * Horiz * Player.right;
    spawnPosition.y = Forest.SampleHeight(spawnPosition);
    spider = Instantiate(SpiderPrefab, spawnPosition, Quaternion.LookRotation(spawnPosition - Player.position));
    spider.GetComponent<Enemy1Spider>().controller = this;
  }
}