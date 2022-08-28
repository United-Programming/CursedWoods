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
  public Transform Center;
  public Controller Game;
  public Terrain Forest;
  public Bear BearPrefab;

  public int done = 0;
  Bear bear = null;


  public override void Init(Terrain forest, Controller controller, bool sameLevel) {
    if (bear != null) Destroy(bear.gameObject);
    Forest = forest;
    Game = controller;
    Center = controller.transform;
    Player = controller.transform.GetChild(1);
    if (!sameLevel) done = 0;
    SpawnBear();
  }

  void SpawnBear() {
    bear = Instantiate(BearPrefab, transform);
    bear.Init(this, 2.5f, Vector3.zero);
  }


  public override void PlayerDeath() {
    Game.PlayerDeath(true);
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
      Game.EnemyKilled(done);
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
      Game.WinLevel();
    }
  }

  public override void RemoveAllEnemies() {
    if (bear != null) Destroy(bear.gameObject);
    bear = null;
  }


  public override void ArrowhitAlert(Vector3 hitPoint) { // Alert the bear if the arrow was close enough
    if (bear.status != Bear.BearStatus.Waiting && bear.status != Bear.BearStatus.Walking) return; // Not needed
    if (Vector3.Distance(bear.transform.position, hitPoint) > 5) return; // Too far away
    bear.StartBuffing();
  }

}