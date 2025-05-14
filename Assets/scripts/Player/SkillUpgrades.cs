using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class SkillUpgrades : MonoBehaviour
{
    [SerializeField] private bool isUpgrades;

    [SerializeField] private GameObject icon;
    
    [SerializeField] private TextMeshProUGUI gradeText;

    [SerializeField] private int grade;

    private void Update()
    {
        icon.SetActive(isUpgrades);
    }

    public void Upgrades()
    {
        if(isUpgrades)
        {
            grade++;

            gradeText.text = grade.ToString();

            isUpgrades = false;
        }
    }
}
