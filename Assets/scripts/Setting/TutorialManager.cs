using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("教學步驟總數")]
    public int totalSteps = 5; // 可在 Inspector 設定教學步驟數

    [Header("目前教學步驟 (從0開始)")]
    [SerializeField] private int currentStep = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 取得目前教學步驟
    public int GetCurrentStep()
    {
        return currentStep;
    }

    // 前進到下一步
    public void NextStep()
    {
        if (currentStep < totalSteps - 1)
        {
            currentStep++;
        }
    }

    // 重設教學進度
    public void ResetTutorial()
    {
        currentStep = 0;
    }

    // 設定到指定步驟
    public void SetStep(int step)
    {
        if (step >= 0 && step < totalSteps)
        {
            currentStep = step;
        }
    }
} 