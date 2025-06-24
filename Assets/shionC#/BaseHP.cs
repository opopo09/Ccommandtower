using UnityEngine;
using UnityEngine.UI;
using TMPro;  // ← 追加！

public class BaseHP : MonoBehaviour
{
    [Header("HP設定")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("UI設定")]
    public Image hpBar;
    public Image afterImageBar;
    public TextMeshProUGUI hpText;  // ← 追加！

    [Header("残像ゲージ設定")]
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

