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
        base.Update();
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
