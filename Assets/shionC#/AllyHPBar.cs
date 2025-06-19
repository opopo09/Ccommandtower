using UnityEngine;
using UnityEngine.UI;

public class AllyHPBar : MonoBehaviour
{
    public Image hpBar;           // 通常HPバー (Fill)
    public Image afterImageBar;   // 残像HPバー (AfterImage)

    public float afterImageSpeed = 1.0f;  // 残像ゲージが減る速さ

    void Start()
    {
        hpBar.fillAmount = 1f;
        afterImageBar.fillAmount = 1f;
    }

    void Update()
    {
        // 残像バーが現在のHPバーより大きい場合、ゆっくり減らす
        if (afterImageBar.fillAmount > hpBar.fillAmount)
        {
            afterImageBar.fillAmount -= afterImageSpeed * Time.deltaTime;

            if (afterImageBar.fillAmount < hpBar.fillAmount)
                afterImageBar.fillAmount = hpBar.fillAmount;
        }
        else
        {
            // 回復時などは通常バーと同じにする
            afterImageBar.fillAmount = hpBar.fillAmount;
        }
    }

    // HPの割合を更新する関数（外部から呼ぶ）
    public void SetHP(float currentHP, float maxHP)
    {
        float hpRatio = Mathf.Clamp01(currentHP / maxHP);
        hpBar.fillAmount = hpRatio;
        // 残像バーはUpdateで追いつくのでここは変えない
    }
}
