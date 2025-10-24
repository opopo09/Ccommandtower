using UnityEngine;
using System.Collections.Generic;

public class RepairManager : MonoBehaviour
{
    public static RepairManager instance;

    [Header("修理ユニット設定")]
    [Tooltip("シーンにいる全ての修理ユニットをここに設定します")]
    public List<RepairUnitAI> repairUnits;

    [Header("UI連携")]
    [Tooltip("修理が必要な時にアクティブにするUIオブジェクト")]
    public GameObject repairNeededSignUI;

    private List<Tower> destroyedTowers = new List<Tower>();
    private Queue<RepairUnitAI> idleRepairUnits = new Queue<RepairUnitAI>();
    private HashSet<Tower> assignedTowers = new HashSet<Tower>(); // 担当者が決まったタワーを管理

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        if (repairNeededSignUI != null) repairNeededSignUI.SetActive(false);
        foreach (var unit in repairUnits)
        {
            if (unit != null) idleRepairUnits.Enqueue(unit);
        }
    }

    void OnEnable()
    {
        Tower.OnTowerDestroyed += HandleTowerDestroyed;
        Tower.OnTowerRepaired += HandleTowerRepaired;
        RepairUnitAI.OnRepairComplete += HandleRepairComplete;
    }

    void OnDisable()
    {
        Tower.OnTowerDestroyed -= HandleTowerDestroyed;
        Tower.OnTowerRepaired -= HandleTowerRepaired;
        RepairUnitAI.OnRepairComplete -= HandleRepairComplete;
    }

    // ▼▼▼▼▼【ここからが新しいAIの核心部分です】▼▼▼▼▼
    void Update()
    {
        // 待機中の修理ユニットがいて、かつ、まだ担当者が決まっていない修理対象のタワーがあるか？
        if (idleRepairUnits.Count > 0 && destroyedTowers.Count > 0)
        {
            AssignRepairTask();
        }

        // UIの表示/非表示を毎フレーム更新する
        UpdateRepairSignUI();
    }
    // ▲▲▲▲▲【ここまで】▲▲▲▲▲

    private void HandleTowerDestroyed(Tower tower)
    {
        if (!destroyedTowers.Contains(tower) && !assignedTowers.Contains(tower))
        {
            destroyedTowers.Add(tower);
        }
    }

    private void HandleTowerRepaired(Tower tower)
    {
        if (destroyedTowers.Contains(tower)) destroyedTowers.Remove(tower);
        if (assignedTowers.Contains(tower)) assignedTowers.Remove(tower);
    }

    private void HandleRepairComplete(RepairUnitAI unit)
    {
        if (!idleRepairUnits.Contains(unit))
        {
            idleRepairUnits.Enqueue(unit);
        }
    }

    private void AssignRepairTask()
    {
        if (idleRepairUnits.Count == 0 || destroyedTowers.Count == 0) return;

        RepairUnitAI unit = idleRepairUnits.Dequeue();
        Tower towerToRepair = FindClosestTower(unit.transform.position, destroyedTowers);

        if (unit != null && towerToRepair != null)
        {
            unit.AssignRepairTarget(towerToRepair);
            destroyedTowers.Remove(towerToRepair);
            assignedTowers.Add(towerToRepair); // このタワーは担当者が決まったことを記録
        }
        else if (unit != null)
        {
            // 適切なタワーが見つからなければ、ユニットを待機キューに戻す
            idleRepairUnits.Enqueue(unit);
        }
    }

    private Tower FindClosestTower(Vector3 position, List<Tower> towers)
    {
        Tower closest = null;
        float minDistance = float.MaxValue;
        foreach (var tower in towers)
        {
            if (tower == null) continue;
            float dist = Vector3.Distance(position, tower.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = tower;
            }
        }
        return closest;
    }

    // ▼▼▼▼▼【UI更新ロジックを修正】▼▼▼▼▼
    private void UpdateRepairSignUI()
    {
        if (repairNeededSignUI != null)
        {
            // 修理待ちリストか、担当者決定済みリストのどちらかに1つでもタワーがあればUIを表示
            bool needsRepair = destroyedTowers.Count > 0 || assignedTowers.Count > 0;
            if (repairNeededSignUI.activeSelf != needsRepair)
            {
                repairNeededSignUI.SetActive(needsRepair);
            }
        }
    }
    // ▲▲▲▲▲【ここまで】▲▲▲▲▲
}