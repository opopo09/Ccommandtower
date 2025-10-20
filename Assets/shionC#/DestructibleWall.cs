using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    [Header("ステータス")]
    public float maxHealth = 100f;

    [Header("スプライト設定")]
    [Tooltip("正常時のスプライト")]
    public Sprite normalSprite;
    [Tooltip("破壊された時のスプライト")]
    public Sprite brokenSprite;

    [Header("画面外インジケーター用")]
    [Tooltip("破壊された時に、インジケーターに表示されるアイコン")]
    public Sprite brokenIcon; // OffScreenIndicatorManager側で使います

    public bool IsBroken { get; private set; }

    private float currentHealth;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        IsBroken = false;
        if (spriteRenderer != null) spriteRenderer.sprite = normalSprite;
    }

    void OnEnable()
    {
        // 自分がシーンに出現したことを司令塔に報告
        if (RepairUnitManager.instance != null)
        {
            RepairUnitManager.instance.AddWall(this);
        }
    }

    void OnDisable()
    {
        // 自分がシーンから消えることを司令塔に報告
        if (RepairUnitManager.instance != null)
        {
            RepairUnitManager.instance.RemoveWall(this);
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsBroken) return; // すでに壊れていたら何もしない

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Break();
        }
    }

    public void Repair(float repairAmount)
    {
        if (!IsBroken) return;

        currentHealth += repairAmount;
        if (currentHealth >= maxHealth)
        {
            Restore();
        }
    }

    private void Break()
    {
        currentHealth = 0;
        IsBroken = true;
        if (spriteRenderer != null) spriteRenderer.sprite = brokenSprite;
        // 必要であれば、Colliderを無効化して通行可能にする
        // Collider2D col = GetComponent<Collider2D>();
        // if(col != null) col.enabled = false;
    }

    private void Restore()
    {
        currentHealth = maxHealth;
        IsBroken = false;
        if (spriteRenderer != null) spriteRenderer.sprite = normalSprite;
        // Colliderを再度有効化する場合
        // Collider2D col = GetComponent<Collider2D>();
        // if(col != null) col.enabled = true;
    }
}