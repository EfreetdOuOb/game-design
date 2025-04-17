using UnityEngine;

public class Box : MonoBehaviour
{

    private Animator animator;
    private BoxState currentState;
    private bool playerInRange = false; // 玩家是否在範圍內


    private void Awake()
    { 
        animator = GetComponent<Animator>();
    }
    void Start()
    {

        SetCurrentState(new Unopened(this));
    }

    void Update()
    {
        currentState.Update();
        
        // 檢測玩家是否在範圍內且按下F鍵
        if (playerInRange && PressInteractKey()) // 使用 PressInteractKey() 方法
        {
            currentState.OnInteract();
        }
    }

    void FixedUpdate()
    {
        currentState.FixedUpdate(); 

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 檢測是否是玩家進入範圍
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
        currentState.OnTriggerEnter2D(collision);
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        // 檢測是否是玩家離開範圍
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
        currentState.OnTriggerExit2D(collision);
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        currentState.OnTriggerStay2D(collision);
    }
    
    //檢測是否按下F鍵
    public bool PressInteractKey()
    {
        return Input.GetKeyDown(KeyCode.F);
    }
    
    //檢測玩家是否在範圍內
    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
    
    public void PlayAnimation(string clip)
    {
        animator.Play(clip);
    }

    //判斷動畫是否播放完畢(只用於一次性動畫) 
    public bool IsAnimationDone(string _aniName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return (stateInfo.IsName(_aniName) && stateInfo.normalizedTime >= 1.0);
    }


    //設置當前狀態
    public void SetCurrentState(BoxState state)
    {
        currentState = state;
    }

}
