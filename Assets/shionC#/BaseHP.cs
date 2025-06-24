using UnityEngine;
using UnityEngine.UI;
using TMPro;  // �� �ǉ��I

public class BaseHP : MonoBehaviour
{
    [Header("HP�ݒ�")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("UI�ݒ�")]
    public Image hpBar;
    public Image afterImageBar;
    public TextMeshProUGUI hpText;  // �� �ǉ��I

    [Header("�c���Q�[�W�ݒ�")]
    public float afterImageSpeed = 0.5f;
    public float damageAmount = 10f;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPBar();
    }

    void Update()
    {
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
            afterImageBar.fillAmount = hpBar.fillAmount;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        UpdateHPBar();
    }

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

        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";
        }
    }
}

