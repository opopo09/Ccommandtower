using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour
{
    public Image hpBar;           // 通常HPバー (Fill)
    public Image afterImageBar;   // 残像HPバー (AfterImage)

    public float afterImageSpeed = 1.0f;  // 残像ゲージが減る速さ

    [Header("表示制御")]
    public float visibleDuration = 1.5f;    // 表示しておく時間（秒）

    private float visibleTimer = 0f;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 開始時に非表示
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        visibleTimer = 0f;
    }

    void Start()
    {
        hpBar.fillAmount = 1f;
        afterImageBar.fillAmount = 1f;
    }

    void Update()
    {
        // 残像バーの追従処理
        if (afterImageBar.fillAmount > hpBar.fillAmount)
        {
            afterImageBar.fillAmount -= afterImageSpeed * Time.deltaTime;
            if (afterImageBar.fillAmount < hpBar.fillAmount)
                afterImageBar.fillAmount = hpBar.fillAmount;
        }
        else
        {
            afterImageBar.fillAmount = hpBar.fillAmount;
        }

        // 表示タイマーを減らして非表示切り替え
        if (canvasGroup.alpha > 0f)
        {
            visibleTimer -= Time.deltaTime;
            if (visibleTimer <= 0f)
            {
                Hide();
            }
        }
    }

    // HP更新＆表示開始
    public void SetHP(float currentHP, float maxHP)
    {
        float hpRatio = Mathf.Clamp01(currentHP / maxHP);
        hpBar.fillAmount = hpRatio;
        Show();
    }

    // 表示をオンにする
    public void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        visibleTimer = visibleDuration;
    }

    // 非表示にする
    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        visibleTimer = 0f;
    }
}
