using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonLevel : Level
{
    public override string ToString() => "Level X - Dragons";

    public override int GetToWin() => ToWin;
    public override string GetName() => ToString();

    public int ToWin = 3;
    public Transform Player;
    public Transform Center;
    public Controller Game;
    public Terrain Forest;
    public Dragon dragonPrefab;
    public Vector3 LevelCenter;
    public override Vector3 GetLevelCenter() => LevelCenter;
    public int done = 0;
    readonly Dragon[] dragons = new Dragon[1];

    private void Start() {
        // Init(Forest, Game, false);
    }
    
    public override void Init(Terrain forest, Controller controller, bool sameLevel) {
        foreach (Dragon dragon in dragons) {
            if (dragon != null) Destroy(dragon.gameObject);
        }

        Forest = forest;
        Game = controller;
        Center = controller.transform;
        Player = controller.Player;
        SpawnDragons();
    }
    
    private void SpawnDragons() {
        float startAngle = Random.Range(-.1f, .5f);
        for (int i = 0; i < dragons.Length; i++) {
            float angle = Mathf.PI * 2 * i / dragons.Length + Random.Range(-.025f, .025f) + startAngle;
            Vector3 spawnPosition = LevelCenter +
                                    new Vector3(Mathf.Sin(angle) * Random.Range(26f, 30f), 0, Mathf.Cos(angle) * Random.Range(26f, 30f));
            spawnPosition.y += Forest.SampleHeight(spawnPosition);
            dragons[i] = Instantiate(dragonPrefab, transform);
            dragons[i].transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0, angle * Mathf.Rad2Deg + 180 + Random.Range(-1f, 1f), 0));
            dragons[i].Init(this, 1.5f + (i + 1) * .15f, spawnPosition);
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
