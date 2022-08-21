using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {
  public static MainMenuButtonController CurrentlySelected { get; set; }

  public static GameObject LastButton { get; private set; }

  private static readonly int ShowMenu = Animator.StringToHash("showMenu");

  [SerializeField] private EventSystem eventSystem;
  [SerializeField] private Animator animator;
  [SerializeField] private bool isTitleSequenceFinished;
  [SerializeField] private bool isOnMenu;
  [SerializeField] private GameObject mainMenu;
  [SerializeField] private GameObject optionsMenu;
  [SerializeField] private GameObject creditsMenu;
  [SerializeField] private Button defaultMainMenuButton;
  [SerializeField] private GameObject optionsButton;
  [SerializeField] private GameObject creditsButton;
  [SerializeField] private GameObject returnButton;

  public void StartNewGame() {
    Debug.Log("Start a new game!");
    // starts a new game
  }

  public void ShowOptions() {
    CurrentlySelected.ShowSelector(false);
    mainMenu.SetActive(false);
    optionsMenu.SetActive(true);
    LastButton = optionsButton;
  }

  public void ShowCredits() {
    CurrentlySelected.ShowSelector(false);
    mainMenu.SetActive(false);
    creditsMenu.SetActive(true);
    LastButton = optionsButton;
  }

  public void QuitGame() {
    Application.Quit();
  }

  public void ReturnToMainMenu() {
    CurrentlySelected.ShowSelector(false);
    optionsMenu.SetActive(false);
    creditsMenu.SetActive(false);
    mainMenu.SetActive(true);
    LastButton = returnButton;
  }

  public void Dummy() {
    Debug.Log("Dummy Result!");
  }

  public void ShowMainMenu() {
    mainMenu.SetActive(true);
    animator.Play("ShowMainMenu", 0, 0f);
  }

  private void Update() {
    if (!Input.GetMouseButtonDown(0) || isOnMenu) {
      return;
    }

    if (isTitleSequenceFinished) {
      isOnMenu = true;
      mainMenu.SetActive(true);
      defaultMainMenuButton.Select();
      animator.SetTrigger(ShowMenu);
    }

    animator.Play("IntroAnimation", 0, 1f);
  }


  public Toggle FullScreenToggle;
  public void ToggleFullScreen() {
    Screen.fullScreen = FullScreenToggle.isOn;
  }
}