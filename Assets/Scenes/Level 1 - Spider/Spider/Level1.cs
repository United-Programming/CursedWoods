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
    StartCoroutine(DestroyAndRespawnAsync(killedByPlayer));
  }
  IEnumerator DestroyAndRespawnAsync(bool killedByPlayer) {
    if (killedByPlayer) {
      yield return new WaitForSeconds(2.5f);
      toWin--;
    }
    if (toWin == 0) {
      // Destroy spider immediate and play win dance and music.
      if (spider != null) {
        float stumpTime = 1;
        Vector3 stumpScale = Vector3.one * .2f;
        while (stumpTime > 0) {
          stumpTime -= Time.deltaTime * 2;
          stumpScale.y = .2f * stumpTime;
          spider.transform.localScale = stumpScale;
          yield return null;
        }
        Destroy(spider);
      }
      spider = null;
      CenterOfWorld.PlayWinDance();
    }
    else {
      yield return new WaitForSeconds(Random.Range(2f, 5f));
      // Spawn Enemy
      if (spider != null) {
        float stumpTime = 1;
        Vector3 stumpScale = Vector3.one * .2f;
        while (stumpTime > 0) {
          stumpTime -= Time.deltaTime * 2;
          stumpScale.y = .2f * stumpTime;
          spider.transform.localScale = stumpScale;
          yield return null;
        }
        Destroy(spider);
      }
      Vector3 spawnPosition = Dist * Player.position - CenterOfWorld.transform.position + Random.insideUnitSphere * Random.Range(1f, 2f) + (Random.Range(0, 2) * 2 - 1) * Horiz * Player.right;
      spawnPosition.y = Forest.SampleHeight(spawnPosition);
      spider = Instantiate(SpiderPrefab, spawnPosition, Quaternion.LookRotation(spawnPosition - Player.position));
      if (spider.TryGetComponent(out Enemy1Spider script)) {
        script.controller = this;
        script.speed += (5 - toWin) * .25f;
      }
    }
  }

  internal void PlayerDeath() {
    CenterOfWorld.PlayerDeath();
  }

  private void Start() {
    Forest = GameObject.FindObjectOfType<Terrain>();
    CenterOfWorld = GameObject.FindObjectOfType<Controller>();
    Player = CenterOfWorld.transform.GetChild(1);

    StartCoroutine(DestroyAndRespawnAsync(false));
  }


  void SpawnEnemy() {
  }
}