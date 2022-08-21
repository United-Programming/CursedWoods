using System.Collections;
using UnityEngine;

public class Level1 : Level {
  public override string ToString() {
    return "Level 1 - Spider";
  }
  public Transform Player;
  public Controller CenterOfWorld;
  public Terrain Forest;
  public GameObject SpiderPrefab;

  public float Horiz = 5;
  public float Dist = 1.5f;
  public int toWin = 5;

  GameObject spider;

  public override void Init(Terrain forest, Controller controller) {
    if (spider != null) Destroy(spider);
    Forest = forest;
    CenterOfWorld = controller;
    Player = CenterOfWorld.transform.GetChild(1);
    StartCoroutine(DestroyAndRespawnAsync(false));
  }


  public override void PlayerDeath() {
    CenterOfWorld.PlayerDeath();
  }

  public override void KillEnemy(GameObject enemy) {
    StartCoroutine(DestroyAndRespawnAsync(true));
  }

  public override void DestroyEnemy(GameObject enemy) {
    StartCoroutine(DestroyAndRespawnAsync(false));
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
      if (spider.TryGetComponent(out Spider script)) {
        script.level = this;
        script.speed += (5 - toWin) * .25f;
      }
    }
  }


}