using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    
    [Header("TIME")]
    [SerializeField] private float gameTime = 0f; // 改為正計時
    public Text timerText;
    public Text scoreText;
    public GameObject gameOverMenu; 
    public GameObject gamePauseMenu;

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
}
