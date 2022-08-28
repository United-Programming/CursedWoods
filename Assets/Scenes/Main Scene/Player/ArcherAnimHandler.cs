using UnityEngine;

public class ArcherAnimHandler : MonoBehaviour {
  public Controller controller;
  public AudioSource audioL;
  public AudioSource audioR;
  public AudioClip[] footStepsGrass;

  public void ArrowLoaded() {
    controller.ArrowLoaded();
  }

  public void ArrowShoot() {
    controller.ArrowShoot();
  }

  public void StepL() {
    audioL.clip = footStepsGrass[Random.Range(0, footStepsGrass.Length)];
    audioL.Play();
  }
  public void StepR() {
    audioR.clip = footStepsGrass[Random.Range(0, footStepsGrass.Length)];
    audioR.Play();
  }

}
