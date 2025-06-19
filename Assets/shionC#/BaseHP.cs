using UnityEngine;
using UnityEngine.UI;

public class BaseHP : MonoBehaviour
{
    [Header("HP設定")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("UI設定")]
    public Image hpBar;         // 通常のHPバー
    public Image afterImageBar; // 残像用のHPバー

    [Header("残像ゲージ設定")]
    public float afterImageSpeed = 0.5f; // 残像ゲージが減る速さ
    public float damageAmount = 10f;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPBar();
    }

    void Update()
    {
        // 残像バーが現在のHPバーに徐々に追いつくようにする
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
            // 基本的に残像は減る側だけ動かすことが多いが
            // 回復時に残像を追い越す場合は同じにする
            afterImageBar.fillAmount = hpBar.fillAmount;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(damageAmount);
        }

    }

    // ダメージ処理の例
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        UpdateHPBar();
    }

    // 回復処理の例
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
        // 残像バーはUpdate()で徐々に追いつくのでここでは変えない
    }
}
