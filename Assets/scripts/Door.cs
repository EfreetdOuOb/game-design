using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator _animator;
    private Collider2D doorLegCollider;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        doorLegCollider = transform.Find("doorLeg").GetComponent<Collider2D>();
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
