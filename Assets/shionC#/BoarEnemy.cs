using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BoarEnemy : MonoBehaviour
{
    [Header("基本パラメータ")]
    public float baseWalkSpeed = 2f;
    public float baseChargeSpeed = 8f;
    public float baseChargeDistance = 4f;
    public float baseWaitTimeAfterCharge = 1f;
    public float baseDamage = 20f;
    public float baseOverrunDistance = 2f; // 突進後の通り過ぎ距離
    public float baseChargeAttackRadius = 1.5f; // 範囲攻撃の半径

    private float walkSpeed;
    private float chargeSpeed;
    private float chargeDistance;
    private float waitTimeAfterCharge;
    private float damage;
    private float overrunDistance;
    private float chargeAttackRadius;

    [Header("ターゲット設定（優先度：高→中→低）")]
    public string[] superPriorityTags;
    public string[] priorityTags;
    public string[] normalTags;

    private GameObject currentTarget;
    private SpriteRenderer spriteRenderer;
    private Vector3 chargeStartPos;
    private Vector3 chargeDirection;
    private Vector3 chargeEndPos;
    private bool hasDealtDamage = false;
    private float stopTimer = 0f;

    private enum State { Idle, Chasing, Charging, Stopped }
    private State currentState = State.Idle;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ResetStats();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                FindTarget();
                if (currentTarget != null) currentState = State.Chasing;
                break;

            case State.Chasing:
                if (currentTarget == null)
                {
                    currentState = State.Idle;
                    return;
                }

                float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (dist <= chargeDistance)
                {
                    StartCharge();
                }
                else
                {
                    MoveToTarget();
                }
                break;

            case State.Charging:
                ChargeForward();
                break;

            case State.Stopped:
                stopTimer -= Time.deltaTime;
                if (stopTimer <= 0f) currentState = State.Idle;
                break;
        }
    }

    void FindTarget()
    {
        currentTarget = FindClosestTarget(superPriorityTags)
                     ?? FindClosestTarget(priorityTags)
                     ?? FindClosestTarget(normalTags);
    }

    GameObject FindClosestTarget(string[] tags)
    {
        if (tags == null) return null;

        GameObject closest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (var tag in tags)
        {
            var objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (var obj in objs)
            {
                if (obj == null) continue;

                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    closest = obj;
                }
            }
        }

        return closest;
    }

    void MoveToTarget()
    {
        if (currentTarget == null) return;

        Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
        transform.position += dir * walkSpeed * Time.deltaTime;

        if (spriteRenderer != null)
            spriteRenderer.flipX = (dir.x < 0);
    }

    void StartCharge()
    {
        if (currentTarget == null) return;

        chargeStartPos = transform.position;
        chargeDirection = (currentTarget.transform.position - transform.position).normalized;

        chargeEndPos = transform.position + chargeDirection * (chargeDistance + overrunDistance);

        hasDealtDamage = false;
        currentState = State.Charging;

        if (spriteRenderer != null)
            spriteRenderer.flipX = (chargeDirection.x < 0);
    }

    void ChargeForward()
    {
        transform.position += chargeDirection * chargeSpeed * Time.deltaTime;

        if (!hasDealtDamage)
        {
            // 範囲攻撃判定：範囲内の対象を自作で探してダメージ
            ApplyAreaDamage();
        }

        if (Vector3.Distance(transform.position, chargeStartPos) >= (chargeDistance + overrunDistance))
        {
            currentState = State.Stopped;
            stopTimer = waitTimeAfterCharge;
        }
    }

    void ApplyAreaDamage()
    {
        bool hitAny = false;
        // superPriorityTags, priorityTags, normalTags すべてから範囲内の対象にダメージ

        foreach (var tags in new string[][] { superPriorityTags, priorityTags, normalTags })
        {
            if (tags == null) continue;

            foreach (var tag in tags)
            {
                var targets = GameObject.FindGameObjectsWithTag(tag);
                foreach (var target in targets)
                {
                    if (target == null) continue;

                    float dist = Vector3.Distance(transform.position, target.transform.position);
                    if (dist <= chargeAttackRadius)
                    {
                        Ally ally = target.GetComponent<Ally>();
                        if (ally != null)
                        {
                            ally.TakeDamage(damage);
                            hitAny = true;
                        }
                    }
                }
            }
        }

        if (hitAny) hasDealtDamage = true;
    }

    bool IsTargetTagDamageable(string tag)
    {
        if (superPriorityTags != null && System.Array.Exists(superPriorityTags, t => t == tag)) return true;
        if (priorityTags != null && System.Array.Exists(priorityTags, t => t == tag)) return true;
        if (normalTags != null && System.Array.Exists(normalTags, t => t == tag)) return true;
        return false;
    }

    public void Initialize(float hpMultiplier, float damageMultiplier, float speedMultiplier)
    {
        damage = baseDamage * damageMultiplier;
        chargeSpeed = baseChargeSpeed * speedMultiplier;
        walkSpeed = baseWalkSpeed * speedMultiplier;
        overrunDistance = baseOverrunDistance;
        chargeDistance = baseChargeDistance;
        waitTimeAfterCharge = baseWaitTimeAfterCharge / speedMultiplier;
        chargeAttackRadius = baseChargeAttackRadius;
    }

    void ResetStats()
    {
        damage = baseDamage;
        chargeSpeed = baseChargeSpeed;
        walkSpeed = baseWalkSpeed;
        chargeDistance = baseChargeDistance;
        waitTimeAfterCharge = baseWaitTimeAfterCharge;
        overrunDistance = baseOverrunDistance;
        chargeAttackRadius = baseChargeAttackRadius;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 endPoint = transform.position + transform.right * (baseChargeDistance + baseOverrunDistance);
        Gizmos.DrawLine(transform.position, endPoint);
        Gizmos.DrawWireSphere(transform.position, baseChargeAttackRadius);
    }
}
