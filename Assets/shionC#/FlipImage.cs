using UnityEngine;

[ExecuteAlways] // 編集中でも反映
public class FlipImage : MonoBehaviour
{
    [Tooltip("true = 左右反転, false = 通常向き")]
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

        // 状態が変わったときだけ反映
        if (isFlipped != lastFlipState)
        {
            spriteRenderer.flipX = isFlipped;
            lastFlipState = isFlipped;
        }
    }
}
