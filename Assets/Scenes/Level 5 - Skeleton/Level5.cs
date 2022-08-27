using System.Collections;
using UnityEngine;

public class Level5 : Level {
  public override string ToString() {
    return "Level 5 - Skeletons";
  }

  public override int GetToWin() { return ToWin; }
  public override string GetName() { return "Level 5 - Skeletons"; }

  public int ToWin = 3;
  public Transform Player;
  public Controller controller;
  public Terrain Forest;
  public GameObject SkeletonPrefab;

  public int done = 0;
  readonly GameObject[] skeletons = new GameObject[8];


  public override void Init(Terrain forest, Controller controller, bool sameLevel) {
    foreach (var skeleton in skeletons)
      if (skeleton != null) Destroy(skeleton);
    Forest = forest;
    this.controller = controller;
    Player = this.controller.transform.GetChild(1);
    if (!sameLevel) done = 0;
    SpawnSkeletons();
  }

  void SpawnSkeletons() {
    for (int i = 0; i < skeletons.Length; i++) {
      float angle = Mathf.PI * 2 * i / skeletons.Length + Random.Range(-.025f, .025f);
      Vector3 spawnPosition = controller.transform.position +
        new Vector3(Mathf.Sin(angle) * Random.Range(28f, 35f), 0, Mathf.Cos(angle) * Random.Range(28f, 35f));
      spawnPosition.y += Forest.SampleHeight(spawnPosition);
      skeletons[i] = Instantiate(SkeletonPrefab, transform);
      skeletons[i].transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0, angle * Mathf.Rad2Deg + 180 + Random.Range(-1f, 1f), 0));
      if (skeletons[i].TryGetComponent(out Skeleton script)) {
        script.Init(this, 1.5f + (i+1) * .15f, spawnPosition);
      }
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
      yield return new WaitForSeconds(2f);
    }
    int pos = -1;
    for (int i = 0; i < skeletons.Length; i++) {
      if (skeletons[i] == enemy) {
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
        stumpTime -= Time.deltaTime * 2;
        stumpScale.y = stumpTime;
        enemy.transform.localScale = stumpScale;
        yield return null;
      }
      Destroy(enemy);
      controller.WinLevel();
    }
    else {
      Debug.Log(enemy.transform.localScale);
      yield return new WaitForSeconds(Random.Range(1f, 3f));
      float stumpTime = 1;
      Vector3 stumpScale = Vector3.one;
      while (stumpTime > 0) {
        stumpTime -= Time.deltaTime * 2;
        stumpScale.y = stumpTime;
        enemy.transform.localScale = stumpScale;
        yield return null;
      }
      Destroy(enemy);
    }
  }

  public override void RemoveAllEnemies() {
    for (int i = 0; i < skeletons.Length; i++) {
      if (skeletons[i] != null) Destroy(skeletons[i]);
      skeletons[i] = null;
    }
  }

}