using UnityEngine;
using UnityEngine.UI;

public class BaseHP : MonoBehaviour
{
    [Header("HP�ݒ�")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("UI�ݒ�")]
    public Image hpBar;         // �ʏ��HP�o�[
    public Image afterImageBar; // �c���p��HP�o�[

    [Header("�c���Q�[�W�ݒ�")]
    public float afterImageSpeed = 0.5f; // �c���Q�[�W�����鑬��
    public float damageAmount = 10f;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPBar();
    }

    void Update()
    {
        // �c���o�[�����݂�HP�o�[�ɏ��X�ɒǂ����悤�ɂ���
        if (afterImageBar.fillAmount > hpBar.fillAmount)
        {
            afterImageBar.fillAmount -= afterImageSpeed * Time.deltaTime;
            if (afterImageBar.fillAmount < hpBar.fillAmount)
            {
                afterImageBar.fillAmount = hpBar.fillAmount;
            }
        }
        else
        {
            // ��{�I�Ɏc���͌��鑤�������������Ƃ�������
            // �񕜎��Ɏc����ǂ��z���ꍇ�͓����ɂ���
            afterImageBar.fillAmount = hpBar.fillAmount;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(damageAmount);
        }

    }

    // �_���[�W�����̗�
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        UpdateHPBar();
    }

    // �񕜏����̗�
    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateHPBar();
    }

    void UpdateHPBar()
    {
        float fill = currentHP / maxHP;
        hpBar.fillAmount = fill;
        // �c���o�[��Update()�ŏ��X�ɒǂ����̂ł����ł͕ς��Ȃ�
    }
}
