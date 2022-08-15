using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject optionsMenu;

    public static MainMenuButtonController CurrentlySelected { get; set; }

    public void StartNewGame()
    {
        Debug.Log("Start a new game!");
        // starts a new game
    }

    public void ShowOptions()
    {
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
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void Dummy()
    {
        Debug.Log("Dummy Result!");
    }
}