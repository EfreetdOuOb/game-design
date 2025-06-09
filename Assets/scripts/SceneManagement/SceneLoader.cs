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

        //淡出當前場景
        yield return StartCoroutine(ScreenFader.Instance.FadeSceneOut());
        //加載所有持久化資料
        yield return SceneManager.LoadSceneAsync(newSceneName);
        //獲取目標場景過度的位置

        //設置進入遊戲物件的位置

        //淡入新場景
        yield return StartCoroutine(ScreenFader.Instance.FadeSceneIn());
    }
}
