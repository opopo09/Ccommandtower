using UnityEngine;

[ExecuteAlways] // •ÒW’†‚Å‚à”½‰f
public class FlipImage : MonoBehaviour
{
    [Tooltip("true = ¶‰E”½“], false = ’ÊíŒü‚«")]
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

        // ó‘Ô‚ª•Ï‚í‚Á‚½‚Æ‚«‚¾‚¯”½‰f
        if (isFlipped != lastFlipState)
        {
            spriteRenderer.flipX = isFlipped;
            lastFlipState = isFlipped;
        }
    }
}
