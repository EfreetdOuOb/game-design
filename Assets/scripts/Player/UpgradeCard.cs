using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UpgradeCard : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button cardButton;
    
    public UnityEvent OnCardSelected;
    public UpgradeSkill Skill { get; private set; }
    
    public void Initialize(UpgradeSkill skill)
    {
        this.Skill = skill;
        
        if (iconImage != null)
        {
            iconImage.sprite = skill.icon;
        }
        
        if (titleText != null)
        {
            titleText.text = skill.skillName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = skill.description;
        }
        
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(() => OnCardSelected?.Invoke());
        }
    }
} 