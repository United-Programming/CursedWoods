using UnityEngine;

public abstract class Level : MonoBehaviour {

  public abstract void Init(Terrain forest, Controller controller);

  public abstract void PlayerDeath();
  public abstract void KillEnemy(GameObject enemy);
  public abstract void DestroyEnemy(GameObject enemy);
}