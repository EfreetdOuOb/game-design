using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator _animator; 

    private void Awake()
    {
        _animator = GetComponent<Animator>(); 
    }

    [ContextMenu(itemName:"Open")]
    public void Open()
    {
        _animator.SetTrigger("Open");
    }

    // 這個方法會被 Animation Event 呼叫
    public void OnOpenAnimationEnd()
    {
        Destroy(gameObject);
    }
}
