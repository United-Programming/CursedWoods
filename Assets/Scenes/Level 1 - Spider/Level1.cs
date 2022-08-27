using System.Collections;
using UnityEngine;

public class Level1 : Level {
  public override string ToString() {
    return "Level 1 - Spider";
  }

  public override int GetToWin() { return ToWin; }
  public override string GetName() { return "Level 1 - Spiders"; }

  public int ToWin = 5;
  public Transform Player;
  public Controller controller;
  public Terrain Forest;
  public GameObject SpiderPrefab;

  public float Horiz = 5;
  public float Dist = 2f;
  public int done = 0;

  GameObject spider;

  public override void Init(Terrain forest, Controller controller, bool sameLevel) {
    if (spider != null) Destroy(spider);
    Forest = forest;
    this.controller = controller;
    Player = this.controller.transform.GetChild(1);
    StartCoroutine(DestroyAndRespawnAsync(false));
    if (!sameLevel) done = 0;
  }


  public override void PlayerDeath() {
    controller.PlayerDeath();
  }

  public override void KillEnemy(GameObject enemy) {
    StartCoroutine(DestroyAndRespawnAsync(true));
  }

  public override void DestroyEnemy(GameObject enemy) {
    StartCoroutine(DestroyAndRespawnAsync(false));
  }

  IEnumerator DestroyAndRespawnAsync(bool killedByPlayer) {
    if (killedByPlayer) {
      if (spider == null) yield break;
      yield return new WaitForSeconds(.5f);
      if (ToWin > done) done++;
      controller.EnemyKilled(done, ToWin);
      yield return new WaitForSeconds(2f);
    }
    if (ToWin == done && killedByPlayer) {
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
      controller.WinLevel();
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
      Vector3 spawnPosition = Player.position + Dist * (Player.position - controller.transform.position) + Random.insideUnitSphere * Random.Range(1f, 2f) + (Random.Range(0, 2) * 2 - 1) * Horiz * controller.transform.right;
      spawnPosition.y = Forest.SampleHeight(spawnPosition);
      spider = Instantiate(SpiderPrefab, spawnPosition, Quaternion.LookRotation(spawnPosition - Player.position));
      if (spider.TryGetComponent(out Spider script)) {
        script.level = this;
        script.speed += done * .25f;
      }
    }
  }

  public override void RemoveAllEnemies() {
    if (spider != null) Destroy(spider);
    spider = null;
  }
  
  public override void FadeIn() {
    throw new System.NotImplementedException();
  }

  public override void FadeOut() {
    throw new System.NotImplementedException();
  }
}