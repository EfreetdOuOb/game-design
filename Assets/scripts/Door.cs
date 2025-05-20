using UnityEngine;
using DG.Tweening;

public class Door : MonoBehaviour
{
    public enum DoorType
    {
        Vertical,    // 直的門
        Horizontal   // 橫的門
    }

    private bool _isOpen = false;
    [SerializeField] private BoxCollider2D _doorCollider; // 門的碰撞箱
    [SerializeField] private DoorType _doorType = DoorType.Horizontal; // 門的類型
    
    // 直門的動畫參數
    [Header("直門動畫參數")]
    [SerializeField] private float _openDuration = 0.5f; // 開門動畫時間
    [SerializeField] private float _closeDuration = 0.5f; // 關門動畫時間
    [SerializeField] private float _openY = 1.95f; // 開門時的 Y 座標
    [SerializeField] private float _closeY = 3.92f; // 關門時的 Y 座標

    // 橫門的動畫參數
    [Header("橫門動畫參數")]
    [SerializeField] private Animator _animator; // 動畫控制器

    private void Awake()
    {
        if (_doorCollider == null)
        {
            _doorCollider = GetComponent<BoxCollider2D>();
        }
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    [ContextMenu(itemName:"Open")]
    public void Open()
    {
        if (!_isOpen)
        {
            _isOpen = true;
            if (_doorType == DoorType.Horizontal)
            {
                // 橫門使用動畫狀態機
                if (_animator != null)
                {
                    _animator.Play("goingDown");
                }
                if (_doorCollider != null)
                {
                    _doorCollider.enabled = false; // 開門時關閉碰撞箱
                }
            }
            else
            {
                // 直門使用 DOTween
                transform.DOMoveY(_openY, _openDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        if (_doorCollider != null)
                        {
                            _doorCollider.enabled = false; // 開門時關閉碰撞箱
                        }
                    });
            }
        }
    }

    [ContextMenu(itemName:"Close")]
    public void Close()
    {
        if (_isOpen)
        {
            _isOpen = false;
            if (_doorType == DoorType.Horizontal)
            {
                // 橫門使用動畫狀態機
                if (_animator != null)
                {
                    _animator.Play("goingUp");
                }
                if (_doorCollider != null)
                {
                    _doorCollider.enabled = true; // 關門時開啟碰撞箱
                }
            }
            else
            {
                // 直門使用 DOTween
                if (_doorCollider != null)
                {
                    _doorCollider.enabled = true; // 關門時開啟碰撞箱
                }
                transform.DOMoveY(_closeY, _closeDuration)
                    .SetEase(Ease.InQuad);
            }
        }
    }

    // 動畫事件：當開門動畫結束時呼叫
    public void OnOpenAnimationEnd()
    {
        if (_doorType == DoorType.Horizontal)
        {
            _animator.Play("idle");
        }
    }

    // 動畫事件：當關門動畫結束時呼叫
    public void OnCloseAnimationEnd()
    {
        if (_doorType == DoorType.Horizontal)
        {
            _animator.Play("idle");
        }
    }
}
