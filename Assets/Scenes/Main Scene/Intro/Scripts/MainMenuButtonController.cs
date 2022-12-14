using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuButtonController : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Toggle toggle;
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject selector;
    [SerializeField] private Button defaultButton;

    public void OnSelect(BaseEventData eventData)
    {
        if (MainMenuController.CurrentlySelected != null && MainMenuController.CurrentlySelected != this)
        {
            MainMenuController.CurrentlySelected.ShowSelector(false);
        }

        selector.SetActive(true);
        ShowSelector(true);
        MainMenuController.CurrentlySelected = this;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (defaultButton == null) return;

        if (defaultButton != button) ShowSelector(false);
        StartCoroutine(CheckSelection());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null) button.Select();
        if (toggle != null) toggle.Select();
        if (slider != null) slider.Select();
    }

    public void ShowSelector(bool boolean)
    {
        selector.SetActive(boolean);
    }

    private IEnumerator CheckSelection()
    {
    
        yield return null;
        if (EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject != MainMenuController.LastButton)
        {
            yield break;
        }

        if (defaultButton!=null) defaultButton.Select();
    }

    private void OnEnable()
    {
        StartCoroutine(CheckSelection());
    }
}