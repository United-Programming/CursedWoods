using UnityEngine;
using UnityEngine.Audio;
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

  [SerializeField] private Slider volumeMusic;
  [SerializeField] private Slider volumeSound;
  [SerializeField] private AudioSource testSound;

  private void Start() {
    float music = PlayerPrefs.GetFloat("VolumeMusic", .7f);
    MasterMixer.SetFloat("VolumeMusic", music * 70 - 60);
    volumeMusic.SetValueWithoutNotify(music);
    float sounds = PlayerPrefs.GetFloat("VolumeSounds", .7f);
    volumeSound.SetValueWithoutNotify(sounds);
    MasterMixer.SetFloat("VolumeSounds", sounds);
  }

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

  public AudioMixer MasterMixer;

  public void AlterVolume(bool music) {
    if (music) {
      MasterMixer.SetFloat("VolumeMusic", volumeMusic.value * 70 - 60);
      PlayerPrefs.SetFloat("VolumeMusic", volumeMusic.value);
    }
    else {
      if (!testSound.isPlaying) testSound.Play();
      MasterMixer.SetFloat("VolumeSounds", volumeSound.value * 70 - 60);
      PlayerPrefs.SetFloat("VolumeSounds", volumeSound.value);
    }
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