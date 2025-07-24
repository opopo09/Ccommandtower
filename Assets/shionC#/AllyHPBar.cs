using UnityEngine;
using UnityEngine.UI;

public class AllyHPBar : MonoBehaviour
{
    public Image hpBar;
    public Image afterImageBar;

    public float afterImageSpeed = 1.0f;
    public float visibleDuration = 1.5f;

    private float visibleTimer = 0f;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 開始時に確実に非表示にする
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
        // 残像バー追従
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

        // 非表示タイマー管理
        if (canvasGroup.alpha > 0f)
        {
            visibleTimer -= Time.deltaTime;
            if (visibleTimer <= 0f)
            {
                Hide();
            }
        }
    }

    public void SetHP(float currentHP, float maxHP)
    {
        float hpRatio = Mathf.Clamp01(currentHP / maxHP);
        hpBar.fillAmount = hpRatio;
        Show();
    }

    public void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        visibleTimer = visibleDuration;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        visibleTimer = 0f;
    }
}
