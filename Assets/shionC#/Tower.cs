using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    [Header("攻撃設定")]
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    private float lastAttackTime = -999f;

    [Header("弾の設定")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float bulletDamage = 10f;
    public int maxAmmo = 10;
    public int currentAmmo = 10;

    [Header("ターゲット設定")]
    public string[] superPriorityTags;
    public string[] priorityTags;
    public string[] targetTags;

    [Header("UI設定")]
    public TextMeshProUGUI ammoText;

    [Header("コマンド入力")]
    public List<CommandButton> correctCommand = new List<CommandButton> { CommandButton.A, CommandButton.B, CommandButton.X };
    public float inputBufferTime = 2f;
    public int recoveryAmount = 3;

    [Header("効果音")]
    public AudioClip inputSuccessSE;
    public AudioClip inputFailureSE;
    private AudioSource audioSource;

    [Header("バイブレーション")]
    public float vibrationDuration = 0.2f;

    private float inputTimer = 0f;
    private int currentCommandIndex = 0;
    private bool vibrationTriggered = false;

    private GameObject currentTarget;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void Update()
    {
        currentTarget = FindTarget();

        if (currentTarget != null && currentAmmo > 0 && Time.time >= lastAttackTime + attackCooldown)
        {
            ShootBullet(currentTarget);
            lastAttackTime = Time.time;
            currentAmmo--;
            UpdateAmmoUI();
        }

        HandleCommandInput();
    }

    void HandleCommandInput()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return;

        CommandButton? input = GetPressedButton(gamepad);
        if (input.HasValue)
        {
            if (currentCommandIndex == 0)
            {
                inputTimer = inputBufferTime;
            }

            if (currentCommandIndex < correctCommand.Count && input.Value == correctCommand[currentCommandIndex])
            {
                currentCommandIndex++;
                if (currentCommandIndex >= correctCommand.Count)
                {
                    RecoverAmmo(recoveryAmount);
                    PlaySE(inputSuccessSE);
                    ResetCommandInput();
                }
            }
            else
            {
                TriggerFailureVibration();
                PlaySE(inputFailureSE);
                ResetCommandInput();
            }
        }

        if (currentCommandIndex > 0)
        {
            inputTimer -= Time.deltaTime;
            if (inputTimer <= 0f)
            {
                ResetCommandInput();
            }
        }
    }

    CommandButton? GetPressedButton(Gamepad gamepad)
    {
        if (gamepad.buttonSouth.wasPressedThisFrame) return CommandButton.A;
        if (gamepad.buttonEast.wasPressedThisFrame) return CommandButton.B;
        if (gamepad.buttonWest.wasPressedThisFrame) return CommandButton.X;
        if (gamepad.buttonNorth.wasPressedThisFrame) return CommandButton.Y;
        if (gamepad.dpad.up.wasPressedThisFrame) return CommandButton.DPadUp;
        if (gamepad.dpad.down.wasPressedThisFrame) return CommandButton.DPadDown;
        if (gamepad.dpad.left.wasPressedThisFrame) return CommandButton.DPadLeft;
        if (gamepad.dpad.right.wasPressedThisFrame) return CommandButton.DPadRight;
        return null;
    }

    void ResetCommandInput()
    {
        currentCommandIndex = 0;
        inputTimer = 0f;
        vibrationTriggered = false;
    }

    void RecoverAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"Ammo: {currentAmmo}/{maxAmmo}";
    }

    GameObject FindTarget()
    {
        System.Func<string[], GameObject> findClosestInTags = (tags) =>
        {
            GameObject closestInGroup = null;
            float minD = float.MaxValue;
            if (tags == null) return null;
            foreach (string tag in tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;
                try
                {
                    GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
                    foreach (var target in targets)
                    {
                        if (target == null) continue;
                        float dist = Vector3.Distance(transform.position, target.transform.position);
                        if (dist <= attackRange && dist < minD)
                        {
                            minD = dist;
                            closestInGroup = target;
                        }
                    }
                }
                catch (UnityException) { }
            }
            return closestInGroup;
        };

        GameObject closestTarget = findClosestInTags(superPriorityTags);
        if (closestTarget != null) return closestTarget;

        closestTarget = findClosestInTags(priorityTags);
        if (closestTarget != null) return closestTarget;

        closestTarget = findClosestInTags(targetTags);
        return closestTarget;
    }


    void ShootBullet(GameObject target)
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.Initialize(target.transform, bulletDamage, bulletSpeed, targetTags);
    }

    void PlaySE(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }



    void TriggerFailureVibration()
    {
        if (!vibrationTriggered)
        {
            GamepadVibrationManager.PlayVibration(vibrationDuration, 0.6f, 0.6f, this);
            vibrationTriggered = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}