using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerExperience : MonoBehaviour
{
    [Header("等級系統")]
    public int currentLevel = 1;
    public float currentExp = 0f;
    public float expToNextLevel = 300f;
    public float expMultiplier = 1.2f; // 每次升級後下一級所需經驗的倍數

    [Header("UI組件")]
    public Slider expSlider;
    public Text levelText;

    [Header("事件")]
    public UnityEvent<float, float> OnExpUpdate; // 添加經驗值更新事件，參數為當前經驗值和下一級所需經驗值
    public UnityEvent<int> OnLevelUp; // 修改事件參數為等級

    private UIManager uiManager;

    private void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        UpdateExpUI();
    }

    // 獲得經驗值
    public void GainExperience(float amount)
    {
        currentExp += amount;
        
        // 觸發經驗值更新事件
        OnExpUpdate?.Invoke(currentExp, expToNextLevel);
        
        // 檢查是否升級
        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
        
        UpdateExpUI();
    }

    // 升級
    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        currentLevel++;
        expToNextLevel = Mathf.Floor(expToNextLevel * expMultiplier);
        
        // 觸發升級事件，傳入當前等級
        OnLevelUp?.Invoke(currentLevel);
        
        Debug.Log("升級到 " + currentLevel + " 級！");
    }

    // 更新經驗值UI
    private void UpdateExpUI()
    {
        if (expSlider != null)
        {
            expSlider.maxValue = expToNextLevel;
            expSlider.value = currentExp;
        }
        
        if (levelText != null)
        {
            levelText.text = "Lv." + currentLevel;
        }
    }
} 