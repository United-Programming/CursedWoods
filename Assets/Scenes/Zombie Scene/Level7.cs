using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Level7 : Level {
  public override string ToString() => "Level 7 - Zombies";

  public override int GetToWin() => ToWin;
  public override string GetName() => ToString();

  public int ToWin = 3;
  public Transform Player;
  public Transform Center;
  public Controller Game;
  public Terrain Forest;
  public Zombie ZombiePrefab;
  public Vector3 LevelCenter;
  public override Vector3 GetLevelCenter() => LevelCenter;
  public int done = 0;
  readonly Zombie[] zombies = new Zombie[30];

  private void Start() {
    Init(Forest, Game, false);
  }

  public override void Init(Terrain forest, Controller controller, bool sameLevel) {
    foreach (Zombie zombie in zombies) {
      if (zombie != null) Destroy(zombie.gameObject);
    }

    Forest = forest;
    Game = controller;
    Center = controller.transform;
    Player = controller.Player;
    SpawnZombies();
  }

  private void SpawnZombies() {
    float startAngle = Random.Range(-.1f, .5f);
    for (int i = 0; i < zombies.Length; i++) {
      float angle = Mathf.PI * 2 * i / zombies.Length + Random.Range(-.025f, .025f) + startAngle;
      Vector3 spawnPosition = LevelCenter +
                              new Vector3(Mathf.Sin(angle) * Random.Range(26f, 30f), 0, Mathf.Cos(angle) * Random.Range(26f, 30f));
      spawnPosition.y += Forest.SampleHeight(spawnPosition);
      zombies[i] = Instantiate(ZombiePrefab, transform);
      zombies[i].transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0, angle * Mathf.Rad2Deg + 180 + Random.Range(-1f, 1f), 0));
      zombies[i].Init(this, 1.5f + (i + 1) * .15f, spawnPosition);
    }
  }

  public override void PlayerDeath() {
    Game.PlayerDeath();
  }

  public override void KillEnemy(GameObject enemy) {

  }

  public override void DestroyEnemy(GameObject enemy) {
  }

  public override void RemoveAllEnemies() {
  }

  public override void ArrowhitAlert(Vector3 hitPoint) {
  }

}
