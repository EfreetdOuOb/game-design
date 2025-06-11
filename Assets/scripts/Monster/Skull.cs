using UnityEngine;

public class Skull : Monster
{
    private bool hasRevived = false; // 是否已經復活過

    protected override void Awake()
    {
        base.Awake();
        // 設置骷髏的基本屬性
         
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        // 檢查遊戲是否暫停
        var gm = FindAnyObjectByType<GameManager>();
        if (gm != null && gm.isPaused)
            return;
        base.Update();
        if (currentState is SkullChase)
        {
            Vector2 direction = MoveTowardsPlayer();
             
            if (direction != Vector2.zero)
            {
                Face(direction);
                transform.Translate(direction * moveSpeed * Time.deltaTime);//裡面使用deltaTime而不是fixedDeltaTime，fixedDeltaTime會根據幀數改變怪物速度
            }
        }
    }

    protected override MonsterState GetIdleState()
    {
        return new SkullIdle(this);
    }

    protected override MonsterState GetHurtState()
    {
        return new SkullHurt(this);
    }

    protected override MonsterState GetDeadState()
    {
        return new SkullSecondDead(this);
    }
    protected override int CalculateExpValue()
    {
        return 65; 
    }

    public void CallReviveFromState()
    {
        OnRevive();
    }

    public override bool CanRevive()
    {
        return !hasRevived;
    }

    public override void OnRevive()
    {
        hasRevived = true;
        currentHealth = startingHealth * 0.5f;
        SetCurrentState(new SkullRevive(this));
    }

    
}
