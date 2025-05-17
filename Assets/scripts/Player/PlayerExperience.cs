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
    private bool isLevelingUp = false; // 防止升級時的經驗條更新衝突

    public float maxExp => expToNextLevel; // 這樣可以直接使用 expToNextLevel 作為 maxExp

    private void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        UpdateExpUI();
    }

    public void UIUpdateExpSlider()
    {
        UIManager.Instance.UpdateExpSlider(currentExp, expToNextLevel);
    }

    public void UIUpdateExpText()
    {
        UIManager.Instance.UpdateLevelText(currentLevel);
    }

    // 獲得經驗值
    public void GainExperience(float amount)
    {
        // 增加經驗值
        currentExp += amount;
        
        // 如果未在升級過程中，則觸發經驗值更新事件
        if (!isLevelingUp)
        {
            OnExpUpdate?.Invoke(currentExp, expToNextLevel);
            
            // 檢查是否升級
            if (currentExp >= expToNextLevel)
            {
                StartCoroutine(ProcessLevelUps());
            }
            else
            {
                // 只有在不需要升級時才直接更新UI
                UpdateExpUI();
            }
        }
        else
        {
            // 如果正在升級中，經驗值會在升級過程結束後更新
            // 不做任何操作，讓升級協程處理
        }
    }

    // 使用協程處理可能的連續升級，以確保UI正確更新
    private IEnumerator ProcessLevelUps()
    {
        isLevelingUp = true;
        
        // 處理所有可能的連續升級
        while (currentExp >= expToNextLevel)
        {
            // 等待一小段時間以確保動畫效果順暢
            yield return new WaitForSeconds(0.05f);
            
            // 執行升級邏輯
            LevelUp();
            
            // 在每次升級後更新UI
            UpdateExpUI();
            
            // 觸發經驗值更新事件
            OnExpUpdate?.Invoke(currentExp, expToNextLevel);
            
            // 在升級之間短暫延遲，讓UI有時間更新
            yield return new WaitForSeconds(0.1f);
        }
        
        // 完成所有升級後的最終UI更新
        UpdateExpUI();
        isLevelingUp = false;
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