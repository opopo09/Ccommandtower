using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("基本ステータス")]
    public float maxHP = 100f;
    public float currentHP;
    public float damage = 10f;
    public float speed = 2f;

    [Header("撃破報酬")]
    [SerializeField] private float gaugeReward = 1.0f;
    [SerializeField] private int experienceReward = 10; // ← 追加：この敵を倒した時の経験値

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
        // ゲージを回復させる処理
        if (GaugeManager.Instance != null)
        {
            GaugeManager.Instance.AddGauge(gaugeReward);
        }

        // ↓↓↓↓ ここに処理を追加しました ↓↓↓↓
        // 経験値を加算する処理
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.AddExperience(experienceReward);
        }
        // ↑↑↑↑ ここまでが追加した処理です ↑↑↑↑

        Destroy(gameObject);
    }
}