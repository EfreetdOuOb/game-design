using UnityEngine;

public class Ghost : Monster
{
    [Header("鬼魂特定屬性")]
    [Tooltip("自爆造成的傷害值")]
    public float attackDamage = 50f; // 攻擊傷害，使用舊有的傷害值
    [Tooltip("自爆後消失的延遲時間")]
    public float explodeDelay = 0.5f; // 自爆後延遲銷毀的時間 (現在主要由動畫和 attackDamageFrame 控制)
    [Tooltip("自爆的範圍")]
    public float explosionRadius = 10f; // 自爆範圍

    protected override void Awake()
    {
        base.Awake();
        // 確保 attackManager 已被賦值 (通常在 Monster.Awake() 或 Unity Editor 中完成)
        if (attackManager == null)
        {
            attackManager = GetComponent<MonsterAttackManager>();
        }
    }

    protected override void Start()
    {
        base.Start();
        // 設置初始狀態為閒置
        SetCurrentState(GetIdleState());
    }
    protected override void Update()
    {
        base.Update();
    }
    protected override int CalculateExpValue()
    {
        return 60; 
    }

    // 覆寫 GetDeadState() 讓它在自爆後進入死亡狀態
    protected override MonsterState GetDeadState()
    {
        return new GhostDead(this);
    }

    protected override MonsterState GetIdleState()
    {
        return new GhostIdle(this);
    }

    protected override MonsterState GetHurtState()
    {
        return new GhostHurt(this);
    }

    // 鬼魂的攻擊就是自爆
    public override void Attack()
    {
        if (canAttack && attackManager != null && target != null)
        {
            // 將 MonsterAttackManager 的攻擊類型設定為 Explosion
            if (attackManager is MonsterAttackManager monsterAttackManagerInstance)
            {
                monsterAttackManagerInstance.attackType = MonsterAttackManager.AttackType.Explosion;
            }

            // 調用 Monster 的 Attack 方法，它會進一步調用 attackManager.StartAttacking(target)
            // MonsterAttackManager 將會播放 "dead" 動畫，並在動畫進度達到 attackDamageFrame 時處理傷害，然後銷毀鬼魂。
            base.Attack();
        }
    }
}
