using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RepairUnitManager : MonoBehaviour
{
    public static RepairUnitManager instance;

    private List<DestructibleWall> allWalls = new List<DestructibleWall>();

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    public void AddWall(DestructibleWall wall) { if (!allWalls.Contains(wall)) allWalls.Add(wall); }
    public void RemoveWall(DestructibleWall wall) { if (allWalls.Contains(wall)) allWalls.Remove(wall); }

    public List<DestructibleWall> GetBrokenWalls()
    {
        // LINQを使って、リストの中から現在 IsBroken が true のものだけを抽出して返す
        return allWalls.Where(wall => wall != null && wall.IsBroken).ToList();
    }
}