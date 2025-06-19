using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;

    public EnemyHPBar hpBar;  // InspectorでHPバーをセットする

    void Start()
    {
        currentHP = maxHP;
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


