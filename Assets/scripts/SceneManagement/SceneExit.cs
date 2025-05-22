using UnityEngine;

public class SceneExit : MonoBehaviour
{
     [Tooltip("要前往的場景名稱")]
     public string newSceneName;

     private void OnTriggerEnter2D(Collider2D other)
     {
        if(other.CompareTag("Player"))
        {
             TransitionInternal();
        }
     }

     public void TransitionInternal()
     {
        SceneLoader.Instance.TransitionToScene(newSceneName);
     }
}
