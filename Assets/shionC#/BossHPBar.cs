using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHPBar : MonoBehaviour
{
    [Header("HP�o�[")]
    [SerializeField] private Image hpBar = null;
    [SerializeField] private Image afterImageBar = null;
    [SerializeField] private TextMeshProUGUI hpText = null;

    [Header("�A�t�^�[�C���[�W�Ǐ]���ԁi�b�j")]
    [SerializeField] private float afterImageLerpDuration = 0.5f;

    private float lerpTimer = 0f;
    private float targetFillAmount = 1f;
    private float startFillAmount = 1f;
    private bool isLerping = false;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        // CanvasGroup��������Βǉ�
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // UI�v�f���������Z�b�g����Ă��邩�`�F�b�N
        if (hpBar == null || afterImageBar == null)
        {
            Debug.LogError("HP�o�[��Image���Z�b�g����Ă��܂���I");
            enabled = false;
            return;
        }

        hpBar.fillAmount = 1f;
        afterImageBar.fillAmount = 1f;

        if (hpText != null)
        {
            hpText.text = "300 / 300";
            hpText.color = Color.white;
            hpText.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            Debug.LogWarning("hpText���A�^�b�`����Ă��܂���I");
        }
    }

    void Update()
    {
        if (isLerping)
        {
            lerpTimer += Time.deltaTime;
            float t = Mathf.Clamp01(lerpTimer / afterImageLerpDuration);
            afterImageBar.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, t);

            if (t >= 1f)
            {
                isLerping = false;
            }
        }
    }

    public void SetHP(float current, float max)
    {
        if (hpBar == null) return;

        float newFill = Mathf.Clamp01(current / max);
        hpBar.fillAmount = newFill;

        // �c���o�[�̒x���Ǐ]�p
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
