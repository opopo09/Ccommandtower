using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RepairUnitAI : MonoBehaviour
{
    [Header("性能")]
    public float moveSpeed = 3f;
    public float repairRange = 1.5f;
    public float repairAmountPerSecond = 20f; // 1秒あたりの修復量

    private DestructibleWall currentTarget;
    private bool isRepairing = false;

    // --- ▼ここから追加▼ ---
    private Animator animator; // Animatorコンポーネントを格納する変数

    void Awake()
    {
        // 自分にアタッチされているAnimatorコンポーネントを取得
        animator = GetComponent<Animator>();
    }
    // --- ▲ここまで追加▲ ---

    void Update()
    {
        FindTargetAndAct();
    }

    void FindTargetAndAct()
    {
        if (isRepairing)
        {
            // 修理中の場合
            if (currentTarget == null || !currentTarget.IsBroken)
            {
                // --- ▼ここから変更▼ ---
                isRepairing = false; // ターゲットが無効になったら修理中断
                animator.SetBool("IsRepairing", false); // Animatorに修理が終了したことを伝える
                currentTarget = null; // ターゲットをクリア
                return;
                // --- ▲ここまで変更▲ ---
            }

            // ターゲットに修復量を適用
            currentTarget.Repair(repairAmountPerSecond * Time.deltaTime);
        }
        else
        {
            // 新しいターゲットを探す
            if (currentTarget == null) // ターゲットがいない場合のみ新しいターゲットを探す
            {
                currentTarget = FindClosestBrokenWall();
            }

            if (currentTarget != null)
            {
                // ターゲットまでの距離を計算
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

                if (distance <= repairRange)
                {
                    // 修理範囲内なら、修理を開始
                    // --- ▼ここから変更▼ ---
                    isRepairing = true;
                    // Animatorにトリガーを送信して、修理アニメーションに切り替える
                    animator.SetTrigger("StartRepair");
                    // Animatorに修理中であることを伝えるbool値も設定
                    animator.SetBool("IsRepairing", true);
                    // --- ▲ここまで変更▲ ---
                }
                else
                {
                    // 範囲外なら、ターゲットに近づく
                    MoveTowards(currentTarget.transform.position);
                }
            }
            // ターゲットがいなければ、何もしない（待機）
        }
    }

    private DestructibleWall FindClosestBrokenWall()
    {
        // RepairUnitManager(後述)から、壊れた壁のリストを取得
        if (RepairUnitManager.instance == null) return null;

        List<DestructibleWall> brokenWalls = RepairUnitManager.instance.GetBrokenWalls();

        DestructibleWall closest = null;
        float minDistance = float.MaxValue;

        foreach (var wall in brokenWalls)
        {
            if (wall == null || !wall.IsBroken) continue; // 無効な壁はスキップ

            float distance = Vector3.Distance(transform.position, wall.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = wall;
            }
        }
        return closest;
    }

    void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}