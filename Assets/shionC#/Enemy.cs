using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;

    public float damage = 10f;  // 攻撃力を追加
    public float speed = 2f;    // 移動速度も追加

    public EnemyHPBar hpBar;  // InspectorでHPバーをセットする

    void Start()
    {
        currentHP = maxHP;
        if (hpBar != null)
        {
            hpBar.SetHP(currentHP, maxHP);
        }
    }

    // WaveManagerから呼び出してステータス強化
    public void Initialize(float hpMultiplier, float damageMultiplier, float speedMultiplier)
    {
        maxHP *= hpMultiplier;
        currentHP = maxHP;

        damage *= damageMultiplier;
        speed *= speedMultiplier;

        if (hpBar != null)
        {
            hpBar.SetHP(currentHP, maxHP);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        if (hpBar != null)
        {
            hpBar.SetHP(currentHP, maxHP);
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
