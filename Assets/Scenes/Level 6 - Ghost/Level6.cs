using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Level6 : Level {
  public override string ToString() {
    return "Level 6 - Ghost";
  }

  public override int GetToWin() { return ToWin; }
  public override string GetName() { return "Level 6 - Ghost"; }

  public int ToWin = 4;
  public Transform Player;
  public Transform Center;
  public Controller Game;
  public Terrain Forest;
  public Ghost GhostPrefab;
  public Orb OrbPrefab;
  public Vector3 LevelCenter;
  public Volume RenderingVolume;
  public override Vector3 GetLevelCenter() => LevelCenter;

  public int done = 0;
  Ghost ghost = null;
  Orb orb = null;
  GhostPostProcessEffect ghostEffect;



  public override void Init(Terrain forest, Controller controller, bool sameLevel) {
    if (ghost != null) Destroy(ghost.gameObject);
    if (orb != null) Destroy(orb.gameObject);
    Forest = forest;
    Game = controller;
    Center = controller.transform;
    Player = controller.transform.GetChild(1);
    if (!sameLevel) done = 0;
    SpawnGhost();

    if (RenderingVolume.sharedProfile.TryGet(out ghostEffect)) {
      ghostEffect.intensity.value = 0;
    }
  }

  void SpawnGhost() {
    ghost = Instantiate(GhostPrefab, transform);
    ghost.Init(this);

    orb = Instantiate(OrbPrefab, transform);
    orb.Init(this, ghost);
  }


  public override void PlayerDeath() {
    if (ghostEffect != null) {
      float targetValue = ghostEffect.intensity.value;
      if (targetValue == 0) targetValue = .3f;
      else if (targetValue == .3f) targetValue = .5f;
      else if (targetValue == .5f) targetValue = .7f;
      else {
        Game.PlayerDeath(true);
        targetValue = 0;
      }
      StartCoroutine(FadeEffectValue(targetValue));
    }
    else
      Game.PlayerDeath(true);
  }
  IEnumerator FadeEffectValue(float targetValue) {
    float time = 0;
    yield return null;
    while (time < 1) {
      time += Time.deltaTime;
      float val = Mathf.Lerp(ghostEffect.intensity.value, targetValue, time);
      ghostEffect.intensity.value = val;
      yield return null;
    }
    ghostEffect.intensity.value = targetValue;
  }

  public override void KillEnemy(GameObject enemy) {
    StartCoroutine(DestroyAsync(enemy, true));
  }

  public override void DestroyEnemy(GameObject enemy) {
    // No automatic destroy
  }

  public bool Completed => done == ToWin;

  IEnumerator DestroyAsync(GameObject enemy, bool killedByPlayer) {
    if (ToWin > done) done++;
    yield return new WaitForSeconds(.5f);
    Game.EnemyKilled(done);
    if (ToWin == done) {
      Destroy(enemy);
      Game.WinLevel();
    }
  }

  public override void RemoveAllEnemies() {
    if (ghost != null) Destroy(ghost.gameObject);
  }

  public override void ArrowhitAlert(Vector3 hitPoint) {
    if (Vector3.Distance(hitPoint, orb.transform.position) < 10) {
      // have the ghost to come close and start hitting
      ghost.ReachOrb(orb.transform.position);
    }
  }

}