using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillTreeController : MonoBehaviour
{
    public static readonly Dictionary<string, int> Skills = new()
    {
        {"Armor", PlayerData.Armor},
        {"Attack Speed", PlayerData.AttackSpeed},
        {"Move Speed", PlayerData.MoveSpeed},
        {"Accuracy", PlayerData.Accuracy},
    };
    public static SkillTreeController Instance { get; private set; }



    [SerializeField] private List<SkillTreeButtonController> buttons;
    [SerializeField] private TextMeshProUGUI experienceCounter;


    public void UpdateDisplay()
    {
        experienceCounter.text = PlayerData.Experience.ToString();
        foreach (SkillTreeButtonController button in buttons)
        {
            button.SetState();
        }
    }

    public void AddExperience()
    {
        PlayerData.GainExperience(5);
        UpdateDisplay();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        UpdateDisplay();
    }
}