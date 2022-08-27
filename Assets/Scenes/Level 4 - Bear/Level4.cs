using System.Collections;
using UnityEngine;

public class Level4 : Level {
  public override string ToString() {
    return "Level 4 - Bear";
  }

  public override int GetToWin() { return ToWin; }
  public override string GetName() { return "Level 4 - Bear"; }

  public int ToWin = 3;
  public Transform Player;
  public Controller controller;
  public Terrain Forest;
  public GameObject BearPrefab;

  public int done = 0;
  GameObject bear = null;


  public override void Init(Terrain forest, Controller controller, bool sameLevel) {
    if (bear != null) Destroy(bear);
    Forest = forest;
    this.controller = controller;
    Player = this.controller.transform.GetChild(1);
    if (!sameLevel) done = 0;
    SpawnFrogs();
  }

  void SpawnFrogs() {
    bear = Instantiate(BearPrefab, transform);
    if (bear.TryGetComponent(out Bear script)) {
      script.Init(this, 2.5f, Vector3.zero);
    }
  }


  public override void PlayerDeath() {
    controller.PlayerDeath(true);
  }

  public override void KillEnemy(GameObject enemy) {
    StartCoroutine(DestroyAsync(enemy, true));
  }

  public override void DestroyEnemy(GameObject enemy) {
    // Level 4 will not have bears to be removed automatically
  }

  IEnumerator DestroyAsync(GameObject enemy, bool killedByPlayer) {
    if (killedByPlayer) {
      yield return new WaitForSeconds(.5f);
      if (ToWin > done) done++;
      controller.EnemyKilled(done, ToWin);
    }

    if (bear == null || enemy == null) yield break; // This will happen when the bear is not yet fully killed

    if (ToWin == done && killedByPlayer) {
      yield return new WaitForSeconds(2.5f);
      // Destroy bee immediate and play win dance and music.
      float stumpTime = 1;
      Vector3 stumpScale = Vector3.one * .1f;
      while (stumpTime > 0) {
        stumpTime -= Time.deltaTime * 2;
        stumpScale.y = .1f * stumpTime;
        enemy.transform.localScale = stumpScale;
        yield return null;
      }
      Destroy(enemy);
      controller.WinLevel();
    }
  }

  public override void RemoveAllEnemies() {
    if (bear != null) Destroy(bear);
    bear = null;
  }

}