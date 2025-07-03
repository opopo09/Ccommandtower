using UnityEngine;

[ExecuteAlways] // �ҏW���ł����f
public class FlipImage : MonoBehaviour
{
    [Tooltip("true = ���E���], false = �ʏ����")]
    public bool isFlipped = false;

    private SpriteRenderer spriteRenderer;
    private bool lastFlipState;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        // ��Ԃ��ς�����Ƃ��������f
        if (isFlipped != lastFlipState)
        {
            spriteRenderer.flipX = isFlipped;
            lastFlipState = isFlipped;
        }
    }
}
