using UnityEngine;
using System.Collections.Generic; // ここを修正しました！

public class PatrollingEnemyAI : MonoBehaviour
{
    [Header("移動設定")]
    public float patrolSpeed = 1.5f; // 巡回時の速度
    public float chaseSpeed = 3.0f;  // 追跡時の速度

    [Header("索敵・攻撃設定")]
    public float detectionRange = 5f; // 索敵範囲
    public float attackRange = 1.5f;  // 攻撃範囲
    public float attackCooldown = 1f; // 攻撃クールダウン

    [Header("ターゲットタグ")]
    public string targetTag = "Ally"; // 追いかけるオブジェクトのタグ (例: "Ally", "Player")
    public string baseTag = "Base";   // 最終的な拠点オブジェクトのタグ (オプション)

    [Header("巡回設定")]
    public List<Transform> patrolPoints; // 巡回するウェイポイントのリスト
    public float waypointThreshold = 0.5f; // ウェイポイントに到達したとみなす距離

    [Header("範囲基準点")]
    public Transform rangeOrigin; // 索敵・攻撃範囲の基準点となるTransform (子オブジェクトを設定推奨)

    // AIの状態管理に関する追加
    public enum AIState
    {
        Patrolling,  // 巡回中
        Chasing,     // ターゲットを追跡中
        Attacking,   // 攻撃中
        Fleeing      // 逃走中
    }
    private AIState currentState = AIState.Patrolling; // 初期状態は巡回

    [Header("逃走設定")]
    public float fleeDistance = 8f; // この距離まで逃げたら巡回に戻る (ターゲットから離れる距離)
    public float reEngageCooldown = 3f; // 逃走後、再度ターゲットを追いかけるまでのクールダウン
    private float lastAttackFleeTime = -999f; // 最後に攻撃して逃走を開始した時間

    private Transform currentTarget;
    private float lastAttackTime = -999f;
    private Enemy enemy; // HP, 攻撃力, 速度などを管理するコンポーネント (既存のEnemy.csを想定)

    private Vector3 currentPatrolTarget; // 現在向かっている巡回地点
    private int currentPatrolIndex = 0;  // 現在のウェイポイントのインデックス

    void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Enemy コンポーネントがアタッチされていません。このAIは正常に動作しません。", this);
            enabled = false; // Enemyコンポーネントがない場合、このスクリプトを無効にする
            return;
        }

        // rangeOriginが設定されていない場合、自身のTransformをデフォルトとして使う
        if (rangeOrigin == null)
        {
            rangeOrigin = this.transform;
            Debug.LogWarning("Range Origin が設定されていません。敵のTransform.positionが基準点として使用されます。", this);
        }

        // 巡回ポイントが設定されていれば、最初のポイントを設定
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            SetNextPatrolTarget();
        }
    }

    void Update()
    {
        if (enemy == null) return;

        // 状態に基づいて行動を決定
        switch (currentState)
        {
            case AIState.Patrolling:
                // 巡回中に索敵
                FindTargetInDetectionRange();
                if (currentTarget != null)
                {
                    // ターゲットを見つけたら追跡状態へ
                    currentState = AIState.Chasing;
                    Debug.Log("ステート変更: Patrolling -> Chasing");
                }
                Patrol();
                break;

            case AIState.Chasing:
                // 追跡中にターゲットが範囲外に出たか、攻撃範囲に入ったかチェック
                FindTargetInDetectionRange(); // ターゲットの距離チェックも兼ねる

                if (currentTarget == null)
                {
                    // ターゲットを見失ったら巡回状態へ
                    currentState = AIState.Patrolling;
                    Debug.Log("ステート変更: Chasing -> Patrolling (ターゲット見失い)");
                }
                else if (Vector3.Distance(rangeOrigin.position, currentTarget.position) <= attackRange)
                {
                    // 攻撃範囲に入ったら攻撃状態へ
                    currentState = AIState.Attacking;
                    Debug.Log("ステート変更: Chasing -> Attacking");
                }
                else
                {
                    ChaseTarget(); // 追跡専用メソッドを呼び出す
                }
                break;

            case AIState.Attacking:
                // 攻撃中
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    PerformAttack(); // 攻撃実行
                    lastAttackTime = Time.time;

                    // 攻撃後、逃走状態へ移行
                    currentState = AIState.Fleeing;
                    lastAttackFleeTime = Time.time; // 逃走開始時間を記録
                    Debug.Log("ステート変更: Attacking -> Fleeing (攻撃後)");
                }
                else if (currentTarget == null || Vector3.Distance(rangeOrigin.position, currentTarget.position) > attackRange)
                {
                    // ターゲットを見失った、または攻撃範囲外に出た場合
                    currentState = AIState.Patrolling; // もしくはChasingに戻すか選択
                    Debug.Log("ステート変更: Attacking -> Patrolling (攻撃中断)");
                }
                // 攻撃クールダウン中は何もせず待機
                break;

            case AIState.Fleeing:
                // 逃走中
                FleeFromTarget();

                // 逃走クールダウンが明けていれば索敵を再開
                if (Time.time - lastAttackFleeTime >= reEngageCooldown)
                {
                    // ターゲットが索敵範囲内に戻ってきていればChasing、そうでなければPatrolling
                    FindTargetInDetectionRange(); // ここでcurrentTargetが更新される可能性がある
                    if (currentTarget != null)
                    {
                        currentState = AIState.Chasing;
                        Debug.Log("ステート変更: Fleeing -> Chasing (再交戦)");
                    }
                    else
                    {
                        currentState = AIState.Patrolling;
                        Debug.Log("ステート変更: Fleeing -> Patrolling (逃走完了、ターゲットなし)");
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 索敵範囲内にターゲットが存在するかをチェックし、currentTargetを設定します。
    /// </summary>
    void FindTargetInDetectionRange()
    {
        // 現在ターゲットがいる場合、それが索敵範囲内にいるかを確認
        if (currentTarget != null)
        {
            // ターゲットがDestroyされた場合もnullになるのでチェック
            if (currentTarget == null)
            {
                Debug.Log("追跡中のターゲットが消失しました。");
                currentTarget = null;
                return;
            }

            // ターゲットが索敵範囲外に出た場合のみ、currentTargetをnullにする
            // Fleeingステートからの再交戦判定では、索敵範囲に入り直す必要があるため、
            // ここではChasingステートからの遷移でのみ範囲外チェックを行う
            if (currentState == AIState.Chasing && Vector3.Distance(rangeOrigin.position, currentTarget.position) > detectionRange)
            {
                Debug.Log($"ターゲット({currentTarget.name})が索敵範囲外に出たため、追跡を終了します。");
                currentTarget = null;
                return;
            }
            // ターゲットが存在し、かつ範囲内であれば、新しいターゲットを探す必要はない
            return;
        }

        // 新しいターゲットを探す (指定されたタグのオブジェクトを優先)
        Collider[] hitColliders = Physics.OverlapSphere(rangeOrigin.position, detectionRange);
        Transform newTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            // 指定されたタグを持つオブジェクトを探す (例: "Ally")
            if (hitCollider.CompareTag(targetTag))
            {
                float dist = Vector3.Distance(rangeOrigin.position, hitCollider.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    newTarget = hitCollider.transform;
                }
            }
        }

        if (newTarget != null)
        {
            currentTarget = newTarget;
            // ここではステート変更せず、Update()の呼び出し元で変更する
            Debug.Log($"新しいターゲット({currentTarget.name})を索敵しました！");
        }
        else
        {
            // 指定されたタグのオブジェクトがいなければ、拠点を探す (オプション)
            GameObject baseObj = GameObject.FindGameObjectWithTag(baseTag);
            if (baseObj != null)
            {
                // 拠点も索敵範囲内にいる場合のみターゲットにする
                if (Vector3.Distance(rangeOrigin.position, baseObj.transform.position) <= detectionRange)
                {
                    currentTarget = baseObj.transform;
                    // ここではステート変更せず、Update()の呼び出し元で変更する
                    Debug.Log($"拠点({currentTarget.name})を索敵しました！");
                }
            }
        }
    }

    /// <summary>
    /// ターゲットを追跡します。（攻撃は行いません）
    /// </summary>
    void ChaseTarget()
    {
        if (currentTarget == null) return;

        Vector3 dir = (currentTarget.position - transform.position).normalized;
        transform.position += dir * chaseSpeed * Time.deltaTime;
        AdjustSpriteDirection(dir.x);
    }

    /// <summary>
    /// 設定されたウェイポイント間を巡回します。
    /// </summary>
    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            // 巡回ポイントが設定されていない場合は何もしない
            return;
        }

        // ウェイポイントに近づいたら次のウェイポイントを設定
        if (Vector3.Distance(transform.position, currentPatrolTarget) < waypointThreshold)
        {
            SetNextPatrolTarget();
        }

        // 移動
        Vector3 dir = (currentPatrolTarget - transform.position).normalized;
        transform.position += dir * patrolSpeed * Time.deltaTime;

        // スプライトの向きを調整
        AdjustSpriteDirection(dir.x);
    }

    /// <summary>
    /// ターゲットから逃走します。
    /// </summary>
    void FleeFromTarget()
    {
        if (currentTarget == null)
        {
            // ターゲットがいなければ、逃走クールダウン後に巡回へ戻る
            // Fleeingステートのまま、reEngageCooldownが明けるのを待つ
            return;
        }

        // ターゲットから遠ざかる方向へ移動
        Vector3 fleeDirection = (transform.position - currentTarget.position).normalized;
        transform.position += fleeDirection * chaseSpeed * Time.deltaTime; // 追跡と同じ速度で逃げる

        AdjustSpriteDirection(fleeDirection.x);
    }


    /// <summary>
    /// 次の巡回目標地点を設定します。
    /// </summary>
    void SetNextPatrolTarget()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        currentPatrolTarget = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count; // 次のインデックスへ（ループ）
    }

    /// <summary>
    /// スプライトの向きを調整します。（右向きスプライトを前提）
    /// </summary>
    /// <param name="directionX">移動方向のX成分</param>
    void AdjustSpriteDirection(float directionX)
    {
        if (directionX != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(directionX);
            transform.localScale = scale;
        }
    }

    /// <summary>
    /// ターゲットに対して攻撃を実行します。
    /// </summary>
    void PerformAttack()
    {
        if (currentTarget == null) return; // ターゲットが消失していたら何もしない

        // ターゲットがAllyコンポーネントを持っているか確認
        Ally ally = currentTarget.GetComponent<Ally>();
        if (ally != null)
        {
            ally.TakeDamage(enemy.damage);
            Debug.Log($"{currentTarget.name} に {enemy.damage} ダメージを与えました。");
            return;
        }

        // ターゲットがBaseHPコンポーネントを持っているか確認
        BaseHP baseHP = currentTarget.GetComponent<BaseHP>();
        if (baseHP != null)
        {
            baseHP.TakeDamage(enemy.damage);
            Debug.Log($"{currentTarget.name} に {enemy.damage} ダメージを与えました。");
            return;
        }

        Debug.LogWarning($"ターゲット {currentTarget.name} に攻撃できるコンポーネント (AllyまたはBaseHP) がありませんでした。");
    }

    /// <summary>
    /// UnityエディターのSceneビューで、視覚的なデバッグ情報を表示します。
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // rangeOrigin が設定されていない場合、自身の位置を基準とする
        Vector3 originPos = (rangeOrigin != null) ? rangeOrigin.position : transform.position;

        // 索敵範囲の描画 (黄色)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(originPos, detectionRange);

        // 攻撃範囲の描画 (赤)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(originPos, attackRange);

        // 巡回ルートの描画 (青)
        if (patrolPoints != null && patrolPoints.Count > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.2f); // ウェイポイント自体を描画
                    if (i < patrolPoints.Count - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position); // ウェイポイント間の線を描画
                    }
                }
            }
            // 最後のポイントと最初のポイントを結ぶ（ループ巡回の場合）
            if (patrolPoints.Count > 1 && patrolPoints[patrolPoints.Count - 1] != null && patrolPoints[0] != null)
            {
                Gizmos.DrawLine(patrolPoints[patrolPoints.Count - 1].position, patrolPoints[0].position);
            }
        }

        // 現在の巡回目標地点の描画 (シアン)
        if (patrolPoints != null && patrolPoints.Count > 0 && currentPatrolTarget != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentPatrolTarget, waypointThreshold);
        }

        // ターゲットへの線の描画 (マゼンタ)
        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}