using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeButtonController : MonoBehaviour
{
    public bool IsUnlocked { get; private set; }

    [SerializeField] private SkillTreeButtonController nextTier;
    [SerializeField] private SkillTreeButtonController previousTier;
    [SerializeField] private Button button;
    [SerializeField] private string skill;
    [SerializeField] private int baseCost;
    [SerializeField] private TextMeshProUGUI skillDisplay;
    [SerializeField] private TextMeshProUGUI costDisplay;
    [SerializeField] private Image connector;
    [SerializeField] private Sprite normalBox;
    [SerializeField] private Sprite inactiveBox;
    [SerializeField] private Sprite boughtBox;
    [SerializeField] private Sprite normalConnector;
    [SerializeField] private Sprite boughtConnector;

    private int _cost;
    private int _level;

    public void BuySkill()
    {
        if (PlayerData.Experience < _cost) return;
        PlayerData.SpendExperience(_cost);
        IsUnlocked = true;
        if (nextTier != null) nextTier.SetState();
        button.interactable = false;
        SkillTreeController.Instance.UpdateDisplay();
        SkillTreeController.Skills[skill] += 1;
    }

    public void SetState()
    {
        if (IsUnlocked)
        {
            button.interactable = false;
            button.image.sprite = boughtBox;
            switch (_level)
            {
                case 3:
                    return;
                default:
                    connector.sprite = nextTier.IsUnlocked ? boughtConnector : normalConnector;
                    return;
            }
        }

        if (PlayerData.Experience < _cost)
        {

            button.interactable = false;
            button.image.sprite = inactiveBox;
            return;
        }

        if (previousTier != null && !previousTier.IsUnlocked)
        {
            return;
        }
        button.image.sprite = normalBox;
        button.interactable = true;
    }

    private void Awake()
    {
        skillDisplay.text = skill;
        _level = nextTier == null ? 3 : previousTier == null ? 1 : 2;
        _cost = baseCost * _level;
        costDisplay.text = _cost.ToString();
    }
}