using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    private EnemySpawner enemySpawner;
    [Header("UI組件")]
    public GameObject gameOverMenu; 
    public GameObject gamePauseMenu;
    public GameObject roomCompleteMenu;

    
    
    [Header("TIME")]
    [SerializeField] private float gameTime = 0f; // 改為正計時
    public Text timerText;
    public Text scoreText;
 
    

    private int score;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        score = 0;
        UpdateScoreText();
        gameOverMenu.SetActive(false);
        gamePauseMenu.SetActive(false);
    }

    void Update()
    {
        Timer();
        if(enemySpawner._allWavesCompleted==true)
        {
            ShowRoomCompleteMenu();
        }
    }

    

    private void Timer()
    {
        // 正常計時
        gameTime += Time.deltaTime;
        
        // 格式化為分:秒
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void ShowGameOverMenu()  
    {
        gameOverMenu.SetActive(true);  
    }

    public void ShowGamePauseMenu()
    {
        gamePauseMenu.SetActive(true);
    }
    
    public void ShowRoomCompleteMenu()
    {
        roomCompleteMenu.SetActive(true);
        
        // 啟動協程等待半秒後關閉
        StartCoroutine(HideRoomCompleteMenuAfterDelay(0.5f));
    }
    
    // 延遲關閉房間完成 UI 的協程
    private IEnumerator HideRoomCompleteMenuAfterDelay(float delay)
    {
        // 使用 WaitForSecondsRealtime 確保即使遊戲暫停也會計時
        yield return new WaitForSecondsRealtime(delay);
        roomCompleteMenu.SetActive(false);
    }
}
