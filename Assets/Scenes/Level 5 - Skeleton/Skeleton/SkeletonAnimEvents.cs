using UnityEngine;

public class SkeletonAnimEvents : MonoBehaviour {
  public Skeleton skeleton;

  public void AttackCompleted() {
    if (skeleton!=null) skeleton.AttackCompleted();
  }

  public void AttackStarted() {
    if (skeleton != null) skeleton.AttackStarted();
  }

  public void StartDefending() {
    if (skeleton != null) skeleton.StartDefending();
  }
}