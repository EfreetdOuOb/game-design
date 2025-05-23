using UnityEngine;

public abstract class AttackManager : MonoBehaviour
{
    [Header("攻擊基礎設定")]
    public float attackRange = 1f;     // 攻擊範圍
    [SerializeField] protected int baseAttackDamage = 10;  // 基礎攻擊傷害
    [SerializeField] public Transform attackPoint;      // 攻擊判定點改為public，讓Monster可以直接訪問
    
    protected bool isAttacking = false;
    protected bool hasDamaged = false;
    protected Transform currentTarget;
    
    // 獲取基礎攻擊傷害值的公開方法
    public int GetBaseAttackDamage()
    {
        return baseAttackDamage;
    }

    // 開始攻擊
    public virtual void StartAttacking(Transform target)
    {
        
        isAttacking = true;
        hasDamaged = false;
        currentTarget = target;
    }
    
    // 停止攻擊
    public virtual void StopAttacking()
    {
        isAttacking = false;
        hasDamaged = false;
        currentTarget = null;
    }
    
    // 獲取當前攻擊傷害值
    public virtual int GetAttackDamage()
    {
        return baseAttackDamage;
    }

    // 獲取攻擊判定位置
    public Vector2 GetAttackPosition()
    {
        return attackPoint != null ? attackPoint.position : transform.position;
    }
    
    // 攻擊判定 - 由動畫事件調用
    public abstract void AttackTrigger();
    
    // 在Unity編輯器中繪製攻擊範圍
    protected virtual void OnDrawGizmosSelected()
    {
        Vector2 attackPos = Application.isPlaying ? GetAttackPosition() : (attackPoint != null ? attackPoint.position : transform.position);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }
}
