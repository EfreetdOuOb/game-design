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


    //�˴��O�_���U��V��
    public bool PressArrowKey()
    {
        return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1 || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1;
    }
    //�˴��O�_���U������
    public bool PressAttackKey()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }



    


    //���Ⲿ��
    public void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal") * moveSpeed;
        float vertical = Input.GetAxisRaw("Vertical") * moveSpeed;

        rb2d.velocity = new Vector2(horizontal, vertical);
    }

    public void Stop()
    {
        float horizontal = 0 * moveSpeed;
        float vertical = 0 * moveSpeed;

        rb2d.velocity = new Vector2(horizontal, vertical);
    }

    //������V
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
    //����ʵe
    public void PlayAnimation(string clip)
    {
        animator.Play(clip);
    }

    //�P�_�ʵe�O�_���񧹲�(�u����@�����ʵe)(�T�w�g�k)
    public bool IsAnimationDone(string _aniName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return (stateInfo.IsName(_aniName) && stateInfo.normalizedTime >= 1.0);
    }


    //������e���A
    public void SetCurrentState(BaseState state)
    {
        currentState = state;
    }
}
