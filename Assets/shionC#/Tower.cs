using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class Tower : MonoBehaviour
{
    [Header("攻撃設定")]
    public float attackRange = 5f;
    public float attackCooldown = 1f;

    [Header("弾の設定")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float bulletDamage = 10f;
    public int maxAmmo = 10;
    public int currentAmmo = 10;

    [Header("ターゲット設定（タグによる優先度）")]
    public string[] superPriorityTags;
    public string[] priorityTags;
    public string[] targetTags;

    [Header("UI設定")]
    public TextMeshProUGUI ammoText;

    [Header("コマンド入力")]
    public List<CommandButton> correctCommand = new List<CommandButton>();
    public float inputBufferTime = 2f;
    public int recoveryAmount = 3;

    [Header("効果音")]
    public AudioClip inputSuccessSE;
    public AudioClip inputFailureSE;

    [Header("バイブレーション")]
    public float vibrationDuration = 0.2f;

    private float lastAttackTime = -999f;
    private GameObject currentTarget;
    private AudioSource audioSource;
    private float inputTimer = 0f;
    private int currentCommandIndex = 0;
    private bool vibrationTriggered = false;

    [Header("体力設定")]
    public float maxHP = 500f;
    public TowerHPBar hpBar;
    [Header("破壊状態の設定")]
    public Sprite destroyedSprite;
    public float repairTime = 5.0f;
    public bool IsDestroyed { get; private set; } = false;
    private float currentHP;
    private Sprite originalSprite;
    private string originalTag;
    public static event System.Action<Tower> OnTowerDestroyed;
    public static event System.Action<Tower> OnTowerRepaired;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalSprite = sr.sprite;
        originalTag = gameObject.tag;
    }

    void Start()
    {
        currentHP = maxHP;
        if (hpBar != null) { hpBar.SetHP(currentHP, maxHP); hpBar.Hide(); }
        IsDestroyed = false;
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void Update()
    {
        if (IsDestroyed) return;
        HandleAttack();
        HandleCommandInput();
    }

    public void TakeDamage(float damage) { if (IsDestroyed) return; currentHP -= damage; if (currentHP < 0) currentHP = 0; if (hpBar != null) { hpBar.SetHP(currentHP, maxHP); } if (currentHP <= 0) { Die(); } }
    private void Die() { if (IsDestroyed) return; IsDestroyed = true; Debug.Log("タワーが破壊されました！"); SpriteRenderer sr = GetComponent<SpriteRenderer>(); if (sr != null && destroyedSprite != null) { sr.sprite = destroyedSprite; } gameObject.tag = "Untagged"; currentTarget = null; OnTowerDestroyed?.Invoke(this); }
    public void StartRepair() { Debug.Log(gameObject.name + " の修理が開始されました。"); }
    public void CompleteRepair() { Debug.Log(gameObject.name + " の修理が完了しました！"); currentHP = maxHP; if (hpBar != null) { hpBar.SetHP(currentHP, maxHP); } SpriteRenderer sr = GetComponent<SpriteRenderer>(); if (sr != null && originalSprite != null) { sr.sprite = originalSprite; } gameObject.tag = originalTag; IsDestroyed = false; OnTowerRepaired?.Invoke(this); }

    void HandleAttack()
    {
        if (currentAmmo <= 0 || Time.time < lastAttackTime + attackCooldown) return;

        currentTarget = FindTarget();

        if (currentTarget != null)
        {
            ShootBullet(currentTarget);
            lastAttackTime = Time.time;
            currentAmmo--;
            UpdateAmmoUI();
        }
    }

    // ▼▼▼▼▼【CRITICAL FIX - THIS IS THE NEW, TAG-BASED BRAIN】▼▼▼▼▼
    GameObject FindTarget()
    {
        System.Func<string[], GameObject> findClosestInTags = (tags) =>
        {
            if (tags == null || tags.Length == 0) return null;

            GameObject closestInGroup = null;
            float minDistance = float.MaxValue;

            foreach (string tag in tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;
                try
                {
                    GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
                    foreach (var target in targets)
                    {
                        float dist = Vector3.Distance(transform.position, target.transform.position);
                        if (dist <= attackRange && dist < minDistance)
                        {
                            minDistance = dist;
                            closestInGroup = target;
                        }
                    }
                }
                catch (UnityException) { /* タグが存在しない場合のエラーを無視 */ }
            }
            return closestInGroup;
        };

        GameObject target = findClosestInTags(superPriorityTags);
        if (target != null) return target;

        target = findClosestInTags(priorityTags);
        if (target != null) return target;

        target = findClosestInTags(targetTags);
        if (target != null) return target;

        return null;
    }
    // ▲▲▲▲▲【END OF CRITICAL FIX】▲▲▲▲▲


    #region Command Input and Other Methods
    void HandleCommandInput() { var gamepad = Gamepad.current; if (gamepad == null) return; CommandButton? input = GetPressedButton(gamepad); if (input.HasValue) { if (currentCommandIndex == 0) { inputTimer = inputBufferTime; } if (currentCommandIndex < correctCommand.Count && input.Value == correctCommand[currentCommandIndex]) { currentCommandIndex++; if (currentCommandIndex >= correctCommand.Count) { RecoverAmmo(recoveryAmount); PlaySE(inputSuccessSE); ResetCommandInput(); } } else { TriggerFailureVibration(); PlaySE(inputFailureSE); ResetCommandInput(); } } if (currentCommandIndex > 0) { inputTimer -= Time.deltaTime; if (inputTimer <= 0f) { ResetCommandInput(); } } }
    CommandButton? GetPressedButton(Gamepad gamepad) { if (gamepad.buttonSouth.wasPressedThisFrame) return CommandButton.A; if (gamepad.buttonEast.wasPressedThisFrame) return CommandButton.B; if (gamepad.buttonWest.wasPressedThisFrame) return CommandButton.X; if (gamepad.buttonNorth.wasPressedThisFrame) return CommandButton.Y; if (gamepad.dpad.up.wasPressedThisFrame) return CommandButton.DPadUp; if (gamepad.dpad.down.wasPressedThisFrame) return CommandButton.DPadDown; if (gamepad.dpad.left.wasPressedThisFrame) return CommandButton.DPadLeft; if (gamepad.dpad.right.wasPressedThisFrame) return CommandButton.DPadRight; return null; }
    void ResetCommandInput() { currentCommandIndex = 0; inputTimer = 0f; vibrationTriggered = false; }
    void RecoverAmmo(int amount) { currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo); UpdateAmmoUI(); }
    void UpdateAmmoUI() { if (ammoText != null) ammoText.text = $"Ammo: {currentAmmo}/{maxAmmo}"; }
    void ShootBullet(GameObject target) { if (bulletPrefab == null || firePoint == null || target == null) return; GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity); Bullet bulletScript = bullet.GetComponent<Bullet>(); if (bulletScript != null) bulletScript.Initialize(target.transform, bulletDamage, bulletSpeed, new string[] { target.tag }); }
    void PlaySE(AudioClip clip) { if (audioSource != null && clip != null) audioSource.PlayOneShot(clip); }
    void TriggerFailureVibration() { if (!vibrationTriggered) { GamepadVibrationManager.PlayVibration(vibrationDuration, 0.6f, 0.6f, this); vibrationTriggered = true; } }
    void OnDrawGizmosSelected() { Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange); }
    #endregion
}