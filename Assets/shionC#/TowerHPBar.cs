using UnityEngine;
using UnityEngine.UI;

public class TowerHPBar : MonoBehaviour
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
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        if (hpBar != null) hpBar.fillAmount = 1f;
        if (afterImageBar != null) afterImageBar.fillAmount = 1f;
    }

    void Update()
    {
        if (afterImageBar != null && hpBar != null && afterImageBar.fillAmount > hpBar.fillAmount)
        {
            afterImageBar.fillAmount -= afterImageSpeed * Time.deltaTime;
            if (afterImageBar.fillAmount < hpBar.fillAmount) afterImageBar.fillAmount = hpBar.fillAmount;
        }

        if (canvasGroup.alpha > 0f)
        {
            visibleTimer -= Time.deltaTime;
            if (visibleTimer <= 0f) Hide();
        }
    }

    public void SetHP(float currentHP, float maxHP)
    {
        if (hpBar == null) return;
        float hpRatio = Mathf.Clamp01(currentHP / maxHP);
        hpBar.fillAmount = hpRatio;
        Show();
    }

    public void Show()
    {
        canvasGroup.alpha = 1f;
        visibleTimer = visibleDuration;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }
}