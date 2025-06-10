using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance {get; private set;}

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            
        }else if(Instance != null)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName));
    }
    
    public IEnumerator TransitionCoroutine(string newSceneName)
    {
        // 保存所有持久化資料
        GameManager.Instance.SaveData();

        // 確保淡出效果一定會執行
        yield return StartCoroutine(ScreenFader.Instance.FadeSceneOut());

        //異步加載新場景
        yield return SceneManager.LoadSceneAsync(newSceneName);

         

        //加載所有持久化資料
        GameManager.Instance.LoadData();

        //獲取目標場景過度的位置
        SceneEntrance entrance = FindAnyObjectByType<SceneEntrance>();

        //設置進入遊戲物件的位置
        SetEnteringPosition(entrance);

        //淡入新場景
        yield return StartCoroutine(ScreenFader.Instance.FadeSceneIn());
        
    }

    private void SetEnteringPosition(SceneEntrance entrance)
    {
        if(entrance == null)
            return;
            
        Transform entanceTransform = entrance.transform; 
        PlayerController.Instance.transform.position = entanceTransform.position;
    }
}
