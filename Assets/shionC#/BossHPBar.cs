using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class BossHPBar : MonoBehaviour
{
    [Header("HPバー本体")]
    [SerializeField] private Image hpBar = null;
    [SerializeField] private Image afterImageBar = null;
    [SerializeField] private TextMeshProUGUI hpText = null;

    [Header("アフターイメージ追従時間（秒）")]
    [SerializeField] private float afterImageLerpDuration = 0.5f;

    private float lerpTimer = 0f;
    private float targetFillAmount = 1f;
    private float startFillAmount = 1f;
    private bool isLerping = false;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;
    }

    void Start()
    {
        if (hpBar == null || afterImageBar == null)
        {
            Debug.LogError("HPバーか残像バーが設定されていません！");
            enabled = false;
            return;
        }

        // 初期値を仮で最大表示（SetHPで上書きされる前提）
        SetHP(1f, 1f);
    }

    void Update()
    {
        if (!isLerping) return;

        lerpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(lerpTimer / afterImageLerpDuration);
        afterImageBar.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, t);

        if (t >= 1f)
        {
            isLerping = false;
        }
    }

    /// <summary>
    /// HPバーの表示を更新する（数値とゲージ）
    /// </summary>
    public void SetHP(float current, float max)
    {
        if (hpBar == null || afterImageBar == null) return;

        float safeMax = Mathf.Max(0.01f, max); // 0除算防止
        float newFill = Mathf.Clamp01(current / safeMax);

        hpBar.fillAmount = newFill;

        startFillAmount = afterImageBar.fillAmount;
        targetFillAmount = newFill;
        lerpTimer = 0f;
        isLerping = true;

        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }
}
