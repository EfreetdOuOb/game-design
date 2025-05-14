using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class PlayerController : MonoBehaviour
{
    

    public float moveSpeed;   
    
    [Header("閃避參數")]
    [Tooltip("閃避距離")]
    [SerializeField] private float dashDistance = 3.0f; // 閃避距離
    [Tooltip("閃避持續時間")]
    [SerializeField] private float dashDuration = 0.2f; // 閃避持續時間
    [Tooltip("閃避冷卻時間")]
    [SerializeField] private float dashCooldown = 1.5f; // 閃避冷卻時間
    [Tooltip("閃避特效預製體")]
    [SerializeField] private GameObject dashEffectPrefab; // 閃避特效預製體
    public UnityEvent<float> OnDodgeUpdate;
    
    private bool canDash = true; // 是否可以閃避
    private bool isDashing = false; // 是否正在閃避中
    private float dashCooldownTimer = 0f; // 閃避冷卻計時器
    public Vector2 inputDirection;
    
    private Rigidbody2D rb2d;
    private Animator animator;
    private BaseState currentState;
    public InputActions inputActions;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        inputActions = new InputActions();
    }
    void Start() 
    {
         
        SetCurrentState(new Idle(this));
    }

    void Update()
    {
        currentState.Update();
        inputDirection = inputActions.GamePlay.Move.ReadValue<Vector2>();
        // 更新閃避冷卻
        if (!canDash)
        {
            OnDodgeUpdate?.Invoke(dashCooldownTimer);//更新冷卻條，傳入冷卻時間。
            dashCooldownTimer -= Time.deltaTime; 
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
                dashCooldownTimer = 0;
            }
        }
    }

    void FixedUpdate()
    {
        currentState.FixedUpdate(); 
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
        return Input.GetKeyDown(KeyCode.J);
    }
    
    //檢測是否按下閃避鍵
    public bool PressDashKey()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }



    //移動部分
    public void Move()
    {
        // 如果正在閃避中，則不允許常規移動
        if (isDashing) return;
        
        rb2d.linearVelocity = inputDirection * moveSpeed;

        
    }

    public void Stop()
    {
        // 如果正在閃避中，則不強制停止
        if (isDashing) return;
        
        rb2d.linearVelocity = Vector2.zero;
    }
    
    // 執行閃避
    public void Dash()
    {
        if (!canDash || isDashing) return;
        
        // 獲取當前移動方向
        Vector2 dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        // 如果沒有輸入方向，則使用角色面朝的方向
        if (dashDirection.magnitude < 0.1f)
        {
            dashDirection = transform.localScale.x < 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            dashDirection.Normalize();
        }
        
        // 開始閃避協程
        StartCoroutine(DashCoroutine(dashDirection));
    }
    
    // 閃避協程
    private IEnumerator DashCoroutine(Vector2 direction)
    {
        canDash = false;
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        
        // 播放閃避動畫
        PlayAnimation("dash");
        
        // 計算閃避距離和速度
        float dashSpeed = dashDistance / dashDuration;
        
        // 應用閃避速度
        rb2d.linearVelocity = direction * dashSpeed;
        
        // 生成閃避特效並設為玩家子物件
        if (dashEffectPrefab != null)
        {
            // 實例化特效作為子物件
            GameObject dashEffect = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity, transform);
            
            // 調整特效位置（如果需要）
            dashEffect.transform.localPosition = new Vector3(0, 0, 0);
            
            // 不再直接銷毀特效，讓粒子系統自然完成並消失
            ParticleSystem ps = dashEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // 獲取粒子系統的主模塊
                var main = ps.main;
                // 使用停止行為：EmitAndDestroy，這樣粒子會自然完成生命週期後銷毀
                main.stopAction = ParticleSystemStopAction.Destroy;
                
                // 在閃避結束時停止發射新粒子，但讓已發射的粒子完成生命週期
                StartCoroutine(StopEmittingAfterDash(ps));
            }
        }
        
        // 等待閃避持續時間結束
        yield return new WaitForSeconds(dashDuration);
        
        // 結束閃避
        isDashing = false;
        rb2d.linearVelocity = Vector2.zero;
        
        // 根據當前狀態決定回到哪個狀態
        if (PressArrowKey())
        {
            SetCurrentState(new Run(this));
        }
        else
        {
            SetCurrentState(new Idle(this));
        }
    }
    
    // 在閃避結束時停止發射新粒子
    private IEnumerator StopEmittingAfterDash(ParticleSystem ps)
    {
        if (ps == null) yield break;
        
        // 等待閃避結束
        yield return new WaitForSeconds(dashDuration);
        
        // 停止發射新粒子，但讓現有粒子完成生命週期
        ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }

    //調整方向
    public void Face()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        if (horizontalInput < 0) // 向左
        {
            transform.localScale = new Vector3(5, 5, 1); // 根據角色大小(5,5,0)設置負X軸翻轉
        }
        else if (horizontalInput > 0) // 向右
        {
            transform.localScale = new Vector3(-5, 5, 1); // 恢復正常大小
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


    public void OnEnable()
    {
        inputActions.Enable();
    }


    public void OnDisable()
    {
        inputActions.Disable(); 
    }


    //設置當前狀態
    public void SetCurrentState(BaseState state)
    {
        currentState = state;
    }

    public bool IsDead()
    {
        return currentState is Dead;
    }
    
    // 檢查是否可以閃避
    public bool CanDash()
    {
        return canDash;
    }
    
    // 檢查是否正在閃避中
    public bool IsDashing()
    {
        return isDashing;
    }
}
