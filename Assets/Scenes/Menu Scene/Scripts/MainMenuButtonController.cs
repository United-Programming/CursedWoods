using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuButtonController : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selector;
    [SerializeField] private Button defaultButton;

    public void OnSelect(BaseEventData eventData)
    {
        if (MainMenuController.CurrentlySelected != this)
        {
            MainMenuController.CurrentlySelected.ShowSelector(false);
        }

        selector.SetActive(true);
        ShowSelector(true);
        MainMenuController.CurrentlySelected = this;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        defaultButton.Select();
        StartCoroutine(CheckSelection());
        ShowSelector(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        button.Select();
    }

    private void ShowSelector(bool boolean)
    {
        selector.SetActive(boolean);
    }

    private IEnumerator CheckSelection()
    {
        yield return null;
        if (eventSystem.currentSelectedGameObject != null)
        {
            yield break;
        }

        defaultButton.Select();
    }

    private void Awake()
    {
        if (button == defaultButton)
        {
            MainMenuController.CurrentlySelected = this;
        }
    }
}