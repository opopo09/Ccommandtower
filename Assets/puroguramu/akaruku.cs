using UnityEngine;

public class akaruku : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        // 明るくする
        spriteRenderer.color = new Color(1.5f, 1.5f, 1.5f, 1f);
    }
}
