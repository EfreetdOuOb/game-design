using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFlowController : MonoBehaviour
{
    public static LevelFlowController Instance { get; private set; }

    public enum LevelState { Waiting, Playing, Completed, Failed, Transition }
    public LevelState CurrentState { get; private set; } = LevelState.Waiting;

    [Header("流程控制")]
    public float transitionDelay = 2f; // 過場延遲

    private GameManager gameManager;
    private UIManager uiManager;
    private EnemySpawner enemySpawner;
    private PlayerController playerController;

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
            return;
        }
    }

    private void Start()
    {
        InitReferences();
        StartLevel();
    }

    private void InitReferences()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        uiManager = FindAnyObjectByType<UIManager>();
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        playerController = FindAnyObjectByType<PlayerController>();
    }

    private void Update()
    {
        if (CurrentState == LevelState.Playing)
        {
            // 檢查玩家是否死亡
            if (playerController == null || playerController.GetComponent<Health>().currentHealth <= 0)
            {
                OnLevelFailed();
            }
            // 檢查敵人是否全滅
            else if (enemySpawner != null && enemySpawner._allWavesCompleted && GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0)
            {
                OnLevelCompleted();
            }
        }
    }

    public void StartLevel()
    {
        InitReferences();
        CurrentState = LevelState.Playing;
        // 可加載關卡開始動畫、音效等
        Debug.Log("關卡開始");
    }

    public void OnLevelCompleted()
    {
        if (CurrentState != LevelState.Playing) return;
        CurrentState = LevelState.Completed;
        Debug.Log("關卡完成");
        if (uiManager != null)
            uiManager.ShowRoomCompleteMenu();
        StartCoroutine(TransitionToNextLevel());
    }

    public void OnLevelFailed()
    {
        if (CurrentState != LevelState.Playing) return;
        CurrentState = LevelState.Failed;
        Debug.Log("關卡失敗");
        if (uiManager != null)
            uiManager.ShowGameOverMenu();
        // 可加載失敗動畫、音效等
    }

    private IEnumerator TransitionToNextLevel()
    {
        CurrentState = LevelState.Transition;
        yield return new WaitForSecondsRealtime(transitionDelay);
        // 這裡可以根據需求載入下一關或回主選單
        // 例如：
        // SceneManager.LoadScene("NextLevelSceneName");
        Debug.Log("過場結束，可切換到下一關");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
} 