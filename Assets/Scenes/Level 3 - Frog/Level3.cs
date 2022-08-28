using System.Collections;
using UnityEngine;

public class Level3 : Level {
  public override string ToString() {
    return "Level 3 - Frogs";
  }

  public override int GetToWin() { return ToWin; }
  public override string GetName() { return "Level 3 - Frogs"; }

  public int ToWin = 3;
  public Transform Player;
  public Transform Center;
  public Controller Game;
  public Terrain Forest;
  public GameObject FrogPrefab;
  public GameObject BloodPrefab;

  public int done = 0;
  readonly GameObject[] frogs = new GameObject[16];
  GameObject blood = null;


  public override void Init(Terrain forest, Controller controller, bool sameLevel) {
    foreach (var frog in frogs)
      if (frog != null) Destroy(frog);
    if (blood != null) Destroy(blood);
    Forest = forest;
    Game = controller;
    Center = controller.transform;
    Player = controller.transform.GetChild(1);
    if (!sameLevel) done = 0;
    SpawnFrogs();
  }

  void SpawnFrogs() {
    for (int i = 0; i < frogs.Length; i++) {
      float angle = Mathf.PI * 2 * i / frogs.Length + Random.Range(-.025f, .025f);
      Vector3 spawnPosition = Center.position +
        new Vector3(Mathf.Sin(angle) * Random.Range(59.5f, 60.5f), 0, Mathf.Cos(angle) * Random.Range(59.5f, 60.5f));
      spawnPosition.y += Forest.SampleHeight(spawnPosition);
      frogs[i] = Instantiate(FrogPrefab, transform);
      frogs[i].transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0, angle * Mathf.Rad2Deg + 180 + Random.Range(-1f, 1f), 0));
      if (frogs[i].TryGetComponent(out Frog script)) {
        script.Init(this, i * .15f, spawnPosition);
      }
    }
  }


  public override void PlayerDeath() {
    Game.PlayerDeath(true);
    StartCoroutine(AddBlood());
  }
  IEnumerator AddBlood() {
    if (blood != null) Destroy(blood);
    blood = Instantiate(BloodPrefab, transform);
    blood.transform.SetPositionAndRotation(Player.position, Quaternion.Euler(0, Player.localRotation.eulerAngles.y, 0));
    float size = 0;
    while (size < 1) {
      yield return null;
      if (blood == null) yield break;
      size += 5 * Time.deltaTime;
      blood.transform.localScale = new Vector3(6 * size + 1, 5 * size + 1, 4 * size + 1);
    }
    yield return new WaitForSeconds(2.95f);
    while (size > 0) {
      yield return null;
      if (blood == null) yield break;
      size -= 5 * Time.deltaTime;
      blood.transform.localScale = new Vector3(6, 5 * size + 1, 4);
    }
    if (blood == null) yield break;
    Destroy(blood);
    blood = null;
  }

  public override void KillEnemy(GameObject enemy) {
    StartCoroutine(DestroyAsync(enemy, true));
  }

  public override void DestroyEnemy(GameObject enemy) {
    // Level 3 will not have frogs to be removed automatically
  }

  IEnumerator DestroyAsync(GameObject enemy, bool killedByPlayer) {
    if (killedByPlayer) {
      yield return new WaitForSeconds(.5f);
      if (ToWin > done) done++;
      Game.EnemyKilled(done);
      yield return new WaitForSeconds(2f);
    }
    int pos = -1;
    for (int i = 0; i < frogs.Length; i++) {
      if (frogs[i] == enemy) {
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
    for (int i = 0; i < frogs.Length; i++) {
      if (frogs[i] != null) Destroy(frogs[i]);
      frogs[i] = null;
    }
    if (blood != null) Destroy(blood);
    blood = null;
  }

  public override void ArrowhitAlert(Vector3 hitPoint) { // Not used here
  }
}