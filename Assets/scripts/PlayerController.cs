using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Transform hpPoint;
    [SerializeField]
    private RectTransform uiRoot;
    [SerializeField]
    private RectTransform hpUITransform;

    public float moveSpeed;   
    private Rigidbody2D rb2d;
    private Animator animator;
    private BaseState currentState;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void Start() 
    {
         
        SetCurrentState(new Idle(this));
    }

    void Update()
    {
        currentState.Update();
    }

    void FixedUpdate()
    {
        currentState.FixedUpdate();
        var pos = Camera.main.WorldToScreenPoint(hpPoint.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(uiRoot, pos, null, out var p);
        hpUITransform.anchoredPosition = p;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    { 
        currentState.OnTriggerEnter2D(collision);
    }
    private void OnTriggerStay2D(Collider2D collision) 
    {
        currentState.OnTriggerStay2D(collision);
    }


    //檢測是否按下方向鍵
    public bool PressArrowKey()
    {
        return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1 || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1;
    }
    //檢測是否按下攻擊鍵
    public bool PressAttackKey()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }



    


    //移動部分
    public void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal") * moveSpeed;
        float vertical = Input.GetAxisRaw("Vertical") * moveSpeed;

        rb2d.linearVelocity = new Vector2(horizontal, vertical);
    }

    public void Stop()
    {
        float horizontal = 0 * moveSpeed;
        float vertical = 0 * moveSpeed;

        rb2d.linearVelocity = new Vector2(horizontal, vertical);
    }

    //調整方向
    public void Face()
    {
        bool flipped = GetComponentInChildren<SpriteRenderer>().flipX;
        if (Input.GetAxis("Horizontal") < 0 && flipped)
        {
            GetComponentInChildren<SpriteRenderer>().flipX = false;
        }
        if (Input.GetAxis("Horizontal") > 0 && !flipped)
        {
            GetComponentInChildren<SpriteRenderer>().flipX = true;
        } 
    }
    //播放動畫
    public void PlayAnimation(string clip)
    {
        animator.Play(clip);
    }

    //判斷動畫是否播放完畢(只用於一次性動畫)(確認攻擊)
    public bool IsAnimationDone(string _aniName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return (stateInfo.IsName(_aniName) && stateInfo.normalizedTime >= 1.0);
    }


    //設置當前狀態
    public void SetCurrentState(BaseState state)
    {
        currentState = state;
    }
}
