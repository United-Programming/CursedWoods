using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Level2 : Level {
  public override string ToString() {
    return "Level 2 - Bees";
  }

  public override int GetToWin() { return ToWin; }
  public override string GetName() { return "Level 2 - Bees"; }

  public int ToWin = 3;
  public Transform Player;
  public Transform Center;
  public Controller Game;
  public Terrain Forest;
  public GameObject BeePrefab;
  public Vector3 LevelCenter;
  public override Vector3 GetLevelCenter() => LevelCenter;

  public int done = 0;
  readonly GameObject[] bees = new GameObject[8];

  // At init, spawn 8 bees around the player circle
  // They move randomly but will never goo too far away from spawn position
  // When player is close enough they will go to the player (alwyas random but with preferences in player direction)
  // When close enough they will attack and then retreat
  // We have to hit 3 of them


  public override void Init(Terrain forest, Controller controller, bool sameLevel) {
    foreach (var bee in bees)
      if (bee != null) Destroy(bee);
    Forest = forest;
    Game = controller;
    Center = controller.transform;
    Player = controller.transform.GetChild(1);
    if (!sameLevel) done = 0;
    SpawnBees();
  }

  void SpawnBees() {
    for (int i = 0; i < bees.Length; i++) {
      float angle = Random.Range(0, 2 * Mathf.PI);
      Vector3 spawnPosition = Center.position +
        new Vector3(Random.Range(-1f, 1f) + Mathf.Sin(angle) * Random.Range(32f, 34f),
                    Random.Range(1.35f, 5f),
                    Random.Range(-1f, 1f) + Mathf.Cos(angle) * Random.Range(32f, 34f));
      spawnPosition.y += Forest.SampleHeight(spawnPosition);
      bees[i] = Instantiate(BeePrefab, transform);
      bees[i].transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0, angle * Mathf.Rad2Deg + 180 + Random.Range(-15, 15), 0));
      if (bees[i].TryGetComponent(out Bee script)) {
        script.level = this;
        script.speed += i * .15f;
      }
    }
  }


  public override void PlayerDeath() {
    Game.PlayerDeath();
  }

  public override void KillEnemy(GameObject enemy) {
    StartCoroutine(DestroyAsync(enemy, true));
  }

  public override void DestroyEnemy(GameObject enemy) {
    // Level 2 will not have bees to be removed automatically
  }

  IEnumerator DestroyAsync(GameObject enemy, bool killedByPlayer) {
    if (killedByPlayer) {
      yield return new WaitForSeconds(.5f);
      if (ToWin > done) done++;
      Game.EnemyKilled(done);
      yield return new WaitForSeconds(2f);
    }
    int pos = -1;
    for (int i = 0; i < bees.Length; i++) {
      if (bees[i] == enemy) {
        pos = i;
        break;
      }
    }
    if (pos == -1 || enemy == null) yield break; // Should never happen

    if (ToWin == done && killedByPlayer) {
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
    else {
      yield return new WaitForSeconds(Random.Range(2f, 5f));
      float stumpTime = 1;
      Vector3 stumpScale = Vector3.one * .1f;
      while (stumpTime > 0) {
        stumpTime -= Time.deltaTime * 2;
        stumpScale.y = .1f * stumpTime;
        enemy.transform.localScale = stumpScale;
        yield return null;
      }
      Destroy(enemy);
    }
  }

  public override void RemoveAllEnemies() {
    for (int i = 0; i < bees.Length; i++) {
      if (bees[i] != null) Destroy(bees[i]);
      bees[i] = null;
    }
  }

  public override void ArrowhitAlert(Vector3 hitPoint) { // Not used here
  }
}