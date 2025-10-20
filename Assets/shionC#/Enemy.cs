using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("基本ステータス")]
    public float maxHP = 100f;
    public float currentHP;
    public float damage = 10f;
    public float speed = 2f;

    [Header("関連コンポーネント")]
    public EnemyHPBar hpBar;

    void Start()
    {
        currentHP = maxHP;
        if (hpBar != null)
        {
            hpBar.SetHP(currentHP, maxHP);
        }
    }

    public void Initialize(float hpMultiplier, float damageMultiplier, float speedMultiplier)
    {
        maxHP *= hpMultiplier;
        currentHP = maxHP;

        damage *= damageMultiplier;
        speed *= speedMultiplier;

        // 経験値もウェーブに応じて少し増やすと面白いかもしれません（オプション）
        // experienceReward = (int)(experienceReward * hpMultiplier);

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