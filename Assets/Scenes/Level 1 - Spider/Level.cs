using UnityEngine;

public abstract class Level : MonoBehaviour {
  public abstract int GetToWin();
  public abstract string GetName();

  public abstract void Init(Terrain forest, Controller controller, bool sameLevel);

  public abstract void PlayerDeath();
  public abstract void KillEnemy(GameObject enemy);
  public abstract void DestroyEnemy(GameObject enemy);

  public abstract void RemoveAllEnemies();
}