using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private bool isTitleSequenceFinished;
    [SerializeField] private bool isOnMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private Button defaultMainMenuButton;
    private static readonly int ShowMenu = Animator.StringToHash("showMenu");

    public static MainMenuButtonController CurrentlySelected { get; set; }

    public void StartNewGame()
    {
        Debug.Log("Start a new game!");
        // starts a new game
    }

    public void ShowOptions()
    {
        Debug.Log("ShowOptions");
        CurrentlySelected.ShowSelector(false);
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit game!");
        Application.Quit();
    }

    public void ReturnToMainMenu()
    {
        CurrentlySelected.ShowSelector(false);
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void Dummy()
    {
        Debug.Log("Dummy Result!");
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || isOnMenu)
        {
            return;
        }

        if (isTitleSequenceFinished)
        {
            isOnMenu = true;
            mainMenu.SetActive(true);
            defaultMainMenuButton.Select();
            animator.SetTrigger(ShowMenu);
        }

        animator.Play("IntroAnimation", 0, 1f);
    }
}