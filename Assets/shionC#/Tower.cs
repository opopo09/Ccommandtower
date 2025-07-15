using UnityEngine;
using TMPro; // 弾数表示に必要

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
    public TextMeshProUGUI ammoText; // TextMeshProのUIにアサイン

    [Header("コマンド入力")]
    public List<string> inputBuffer = new List<string>();
    public float inputBufferTime = 2f;
    private float inputTimer = 0f;

    public List<string> correctCommand = new List<string> { "A", "B", "X" }; // 回復コマンド
    public int recoveryAmount = 3;

    void Update()
    {
        GameObject target = FindTarget();
        if (target != null && Time.time - lastAttackTime >= attackCooldown && currentAmmo > 0)
        {
            ShootBullet(target);
            lastAttackTime = Time.time;
            currentAmmo--;
        }

        HandleCommandInput();
        UpdateAmmoUI();
    }

    void HandleCommandInput()
    {
        if (inputBuffer.Count > 0)
        {
            inputTimer -= Time.deltaTime;
            if (inputTimer <= 0f)
            {
                inputBuffer.Clear();
            }
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton0)) AddInput("A");
        if (Input.GetKeyDown(KeyCode.JoystickButton1)) AddInput("B");
        if (Input.GetKeyDown(KeyCode.JoystickButton2)) AddInput("X");
        if (Input.GetKeyDown(KeyCode.JoystickButton3)) AddInput("Y");
    }

    void AddInput(string input)
    {
        inputBuffer.Add(input);
        inputTimer = inputBufferTime;

        if (inputBuffer.Count > correctCommand.Count)
            inputBuffer.RemoveAt(0);

        if (inputBuffer.Count == correctCommand.Count)
        {
            bool match = true;
            for (int i = 0; i < correctCommand.Count; i++)
            {
                if (inputBuffer[i] != correctCommand[i])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                RecoverAmmo(recoveryAmount);
                inputBuffer.Clear();
            }
        }
    }

    void RecoverAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        Debug.Log($"弾を{amount}回復！ 現在弾数: {currentAmmo}");
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"Ammo: {currentAmmo}/{maxAmmo}";
        }
    }

    GameObject FindTarget()
    {
        GameObject best = null;
        float closestDist = Mathf.Infinity;

        foreach (string tag in superPriorityTags)
        {
            GameObject[] candidates = GameObject.FindGameObjectsWithTag(tag);
            foreach (var c in candidates)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist <= attackRange && dist < closestDist)
                {
                    closestDist = dist;
                    best = c;
                }
            }
            if (best != null) return best;
        }

        foreach (string tag in priorityTags)
        {
            GameObject[] candidates = GameObject.FindGameObjectsWithTag(tag);
            foreach (var c in candidates)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist <= attackRange && dist < closestDist)
                {
                    closestDist = dist;
                    best = c;
                }
            }
            if (best != null) return best;
        }

        foreach (string tag in targetTags)
        {
            GameObject[] candidates = GameObject.FindGameObjectsWithTag(tag);
            foreach (var c in candidates)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist <= attackRange && dist < closestDist)
                {
                    closestDist = dist;
                    best = c;
                }
            }
        }

        return best;
    }

    void ShootBullet(GameObject target)
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(target.transform, bulletDamage, bulletSpeed, targetTags);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
