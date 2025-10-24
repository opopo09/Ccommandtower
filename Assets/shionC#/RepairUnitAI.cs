using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RepairUnitAI : MonoBehaviour
{
    [Header("性能設定")]
    public float moveSpeed = 4f;
    [Tooltip("修理を始めるための、タワーとの距離")]
    public float repairStartDistance = 1.5f;

    // 自身の任務完了を外部（RepairManager）に通知するためのイベント
    public static event Action<RepairUnitAI> OnRepairComplete;

    // AIの行動状態
    private enum State { Idle, MovingToTarget, Repairing, ReturningHome }
    private State currentState = State.Idle;

    private Tower repairTarget;
    private Queue<Vector3> path;
    private Vector3 initialPosition; // 待機場所

    // --- ▼ここから追加▼ ---
    private Animator animator; // Animatorコンポーネントを格納する変数
    // --- ▲ここまで追加▲ ---

    void Start()
    {
        initialPosition = transform.position; // 最初の位置を待機場所として記憶
        // --- ▼ここから追加▼ ---
        // 自分にアタッチされているAnimatorコンポーネントを取得
        animator = GetComponent<Animator>();
        // --- ▲ここまで追加▲ ---
    }

    void Update()
    {
        switch (currentState)
        {
            case State.MovingToTarget:
                FollowPathToTarget();
                break;
            case State.Repairing:
                // 修理中のロジックはコルーチンで処理されるため、Updateでは何もしない
                break;
            case State.ReturningHome:
                FollowPathToHome();
                break;
            case State.Idle:
                // 待機中のロジック（必要なら追加）
                break;
        }
    }

    /// <summary>
    /// RepairManagerから呼び出される、任務開始の命令
    /// </summary>
    public void AssignRepairTarget(Tower target)
    {
        if (currentState != State.Idle) return; // 待機中でなければ、新しい任務は受け付けない

        repairTarget = target;
        currentState = State.MovingToTarget;

        // AIの頭脳に、目標までの安全なルートを問い合わせる
        // 修理ユニットは敵を避けるので、avoidEnemiesはtrue, penaltyは50（例）
        path = EnemyAI.RequestPathFromAI(transform.position, target.transform.position, true, 50);
    }

    /// <summary>
    /// 目標のタワーへ向かうための経路追跡
    /// </summary>
    private void FollowPathToTarget()
    {
        if (repairTarget == null || !repairTarget.IsDestroyed) { GoHome(); return; }

        // 目的地までの距離をチェック
        if (Vector3.Distance(transform.position, repairTarget.transform.position) <= repairStartDistance)
        {
            // 十分に近づいたら、修理を開始
            StartCoroutine(RepairRoutine());
            return;
        }

        // 経路がなければ（または尽きたら）、再度問い合わせる
        if (path == null || path.Count == 0)
        {
            path = EnemyAI.RequestPathFromAI(transform.position, repairTarget.transform.position, true, 50);
            if (path == null || path.Count == 0)
            {
                // 道が見つからなければ、次のフレームで再試行
                return;
            }
        }

        // 経路を辿る
        Vector3 currentWaypoint = path.Peek();
        MoveTowards(currentWaypoint);
        if (Vector3.Distance(transform.position, currentWaypoint) < 0.2f)
        {
            path.Dequeue();
        }
    }

    /// <summary>
    /// 待機場所へ帰るための経路追跡
    /// </summary>
    private void FollowPathToHome()
    {
        if (path == null || path.Count == 0)
        {
            // 帰還完了
            currentState = State.Idle;
            OnRepairComplete?.Invoke(this); // 司令部に帰還報告
            return;
        }

        Vector3 currentWaypoint = path.Peek();
        MoveTowards(currentWaypoint);
        if (Vector3.Distance(transform.position, currentWaypoint) < 0.2f)
        {
            path.Dequeue();
        }
    }

    /// <summary>
    /// 修理を実行するコルーチン
    /// </summary>
    private IEnumerator RepairRoutine()
    {
        currentState = State.Repairing;
        path = null;

        // --- ▼ここから変更▼ ---
        // 修理アニメーションを開始
        if (animator != null) animator.SetBool("IsRepairing", true);

        if (repairTarget != null) repairTarget.StartRepair();

        yield return new WaitForSeconds(repairTarget != null ? repairTarget.repairTime : 2.0f);

        if (repairTarget != null && repairTarget.IsDestroyed)
        {
            repairTarget.CompleteRepair();
        }

        // 修理アニメーションを終了し、待機/移動アニメーションに戻す
        if (animator != null) animator.SetBool("IsRepairing", false);
        // --- ▲ここまで変更▲ ---

        GoHome();
    }

    /// <summary>
    /// 待機場所へ帰る命令
    /// </summary>
    private void GoHome()
    {
        currentState = State.ReturningHome;
        repairTarget = null;
        path = EnemyAI.RequestPathFromAI(transform.position, initialPosition, true, 50);
    }

    /// <summary>
    /// 指定された目標地点へ移動する
    /// </summary>
    private void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = direction.x < 0;
    }
}