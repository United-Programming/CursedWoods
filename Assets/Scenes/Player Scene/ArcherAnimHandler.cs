using UnityEngine;

public class ArcherAnimHandler : MonoBehaviour {
  public Controller controller;

  public void ArrowLoaded() {
    controller.ArrowLoaded();
  }

  public void ArrowShoot() {
    controller.ArrowShoot();
  }

}
