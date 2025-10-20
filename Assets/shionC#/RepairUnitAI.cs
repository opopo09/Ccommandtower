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
                isRepairing = false; // ターゲットが無効になったら修理中断
                return;
            }

            // ターゲットに修復量を適用
            currentTarget.Repair(repairAmountPerSecond * Time.deltaTime);
        }
        else
        {
            // 新しいターゲットを探す
            currentTarget = FindClosestBrokenWall();

            if (currentTarget != null)
            {
                // ターゲットまでの距離を計算
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

                if (distance <= repairRange)
                {
                    // 修理範囲内なら、修理を開始
                    isRepairing = true;
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

        // 必要に応じてスプライトの向きを変える
        // SpriteRenderer sr = GetComponent<SpriteRenderer>();
        // if(sr != null) sr.flipX = direction.x < 0;
    }
}