using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;

    public float damage = 10f;  // �U���͂�ǉ�
    public float speed = 2f;    // �ړ����x���ǉ�

    public EnemyHPBar hpBar;  // Inspector��HP�o�[���Z�b�g����

    void Start()
    {
        currentHP = maxHP;
        if (hpBar != null)
        {
            hpBar.SetHP(currentHP, maxHP);
        }
    }

    // WaveManager����Ăяo���ăX�e�[�^�X����
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
