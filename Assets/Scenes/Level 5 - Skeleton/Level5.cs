using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Level5 : Level {
  public override string ToString() {
    return "Level 5 - Skeletons";
  }

  public override int GetToWin() { return ToWin; }
  public override string GetName() { return "Level 5 - Skeletons"; }

  public int ToWin = 3;
  public Transform Player;
  public Transform Center;
  public Controller Game;
  public Terrain Forest;
  public Skeleton SkeletonPrefab;
  public Vector3 LevelCenter;
  public override Vector3 GetLevelCenter() => LevelCenter;

  public int done = 0;
  readonly Skeleton[] skeletons = new Skeleton[5];


  public override void Init(Terrain forest, Controller controller, bool sameLevel) {
    foreach (var skeleton in skeletons)
      if (skeleton != null) Destroy(skeleton.gameObject);
    Forest = forest;
    Game = controller;
    Center = controller.transform;
    Player = controller.transform.GetChild(1);
    if (!sameLevel) done = 0;
    SpawnSkeletons();
  }

  void SpawnSkeletons() {
    float startAngle = Random.Range(-.1f, .5f);
    for (int i = 0; i < skeletons.Length; i++) {
      float angle = Mathf.PI * 2 * i / skeletons.Length + Random.Range(-.025f, .025f) + startAngle;
      Vector3 spawnPosition = Center.position +
        new Vector3(Mathf.Sin(angle) * Random.Range(28f, 35f), 0, Mathf.Cos(angle) * Random.Range(28f, 35f));
      spawnPosition.y += Forest.SampleHeight(spawnPosition);
      skeletons[i] = Instantiate(SkeletonPrefab, transform);
      skeletons[i].transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0, angle * Mathf.Rad2Deg + 180 + Random.Range(-1f, 1f), 0));
      skeletons[i].Init(this, 1.5f + (i+1) * .15f, spawnPosition);
    }
  }


  public override void PlayerDeath() {
    Game.PlayerDeath(true);
  }

  public override void KillEnemy(GameObject enemy) {
    StartCoroutine(DestroyAsync(enemy, true));
  }

  public override void DestroyEnemy(GameObject enemy) {
    // No automatic destroy
  }

  IEnumerator DestroyAsync(GameObject enemy, bool killedByPlayer) {
    if (killedByPlayer) {
      yield return new WaitForSeconds(.5f);
      if (ToWin > done) done++;
      Game.EnemyKilled(done);
      yield return new WaitForSeconds(2f);
    }
    int pos = -1;
    for (int i = 0; i < skeletons.Length; i++) {
      if (skeletons[i].gameObject == enemy) {
        pos = i;
        break;
      }
    }
    if (pos == -1 || enemy == null) yield break; // Should never happen

    if (ToWin == done && killedByPlayer) {
      Debug.Log(enemy.transform.localScale);
      // Destroy bee immediate and play win dance and music.
      float stumpTime = 1;
      Vector3 stumpScale = Vector3.one;
      while (stumpTime > 0) {
        stumpTime -= Time.deltaTime * 3;
        stumpScale.y = stumpTime;
        enemy.transform.localScale = stumpScale;
        yield return null;
      }
      Destroy(enemy);
      Game.WinLevel();
    }
    else {
      yield return new WaitForSeconds(Random.Range(1f, 3f));
      float stumpTime = 1;
      Vector3 stumpScale = Vector3.one;
      while (stumpTime > 0) {
        stumpTime -= Time.deltaTime * 3;
        stumpScale.y = stumpTime;
        enemy.transform.localScale = stumpScale;
        yield return null;
      }
      Destroy(enemy);
    }
  }

  public override void RemoveAllEnemies() {
    for (int i = 0; i < skeletons.Length; i++) {
      if (skeletons[i] != null) Destroy(skeletons[i].gameObject);
      skeletons[i] = null;
    }
  }

  public override void ArrowhitAlert(Vector3 hitPoint) { // Alert the skeletons if the arrow was close enough
    foreach (var skel in skeletons) {
      if (skel.status != Skeleton.SkeletonStatus.Walking && skel.status != Skeleton.SkeletonStatus.Waiting) continue; // Not needed
      if (Vector3.Distance(skel.transform.position, hitPoint) > 8) continue; // Too far away
      skel.StartChasing();
    }
  }
}