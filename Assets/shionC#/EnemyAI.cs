using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAI : MonoBehaviour
{
    [Header("自己防衛 & 攻撃設定")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [Header("ターゲット設定")]
    public string[] highPriorityTags;
    public string[] midPriorityTags;
    public string[] lowPriorityTags;

    [Header("ゴール設定")]
    public string goalBaseTag = "base";

    [Header("ナビゲーション設定 (最重要)")]
    public Vector2 gridWorldSize = new Vector2(50, 50);
    public float nodeRadius = 0.3f; // 貫通対策のため、推奨値を0.3に変更

    [Header("初期動作: 散開")]
    [Tooltip("有効な場合、出現時にこの半径内のランダムな地点へ一度移動します。0で無効。")]
    public float initialScatterRadius = 2.0f;

    private enum State { Egress, Scattering, Idle, FollowingPath, Attacking }
    private State currentState;

    private Transform goalBaseTransform;
    private float lastAttackTime = -999f;
    private Enemy enemy;
    private SpriteRenderer spriteRenderer;
    private Queue<Vector3> path;
    private float pathRequestCooldown = 0.5f; // 経路探索の頻度を少し上げる
    private float lastPathRequestTime;
    private Vector3 initialDestination; // 脱出または散開のための初期目標

    private static Node[,] grid;
    private static int gridSizeX, gridSizeY;
    private static float nodeDiameter;
    private static bool isGridCreated = false;

    private class Node
    {
        public bool isWalkable; public Vector3 worldPosition; public int gridX, gridY;
        public int gCost, hCost; public Node parent;
        public int fCost { get { return gCost + hCost; } }
        public Node(bool walkable, Vector3 worldPos, int _gridX, int _gridY)
        {
            isWalkable = walkable; worldPosition = worldPos; gridX = _gridX; gridY = _gridY;
        }
    }

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!isGridCreated) { CreateGrid(); isGridCreated = true; }
    }

    void Start()
    {
        GameObject baseObject = GameObject.FindGameObjectWithTag(goalBaseTag);
        if (baseObject != null) { goalBaseTransform = baseObject.transform; }
        else { Debug.LogError(gameObject.name + " がゴールを見つけられませんでした。"); this.enabled = false; return; }

        if (isGridCreated && !IsWalkable(transform.position))
        {
            currentState = State.Egress;
            initialDestination = GetClosestWalkablePosition(transform.position);
        }
        else if (initialScatterRadius > 0)
        {
            currentState = State.Scattering;
            initialDestination = transform.position + (Vector3)(Random.insideUnitCircle * initialScatterRadius);
        }
        else
        {
            currentState = State.Idle;
        }
        RequestPath((currentState == State.Idle) ? goalBaseTransform.position : initialDestination);
    }

    void Update()
    {
        if (enemy == null || goalBaseTransform == null || !isGridCreated) return;

        if (Vector3.Distance(transform.position, goalBaseTransform.position) < 1.0f) { GoToGoalBaseReached(); return; }

        Transform enemyToAttack = FindClosestTargetByDistance(highPriorityTags) ?? FindClosestTargetByDistance(midPriorityTags) ?? FindClosestTargetByDistance(lowPriorityTags);
        if (enemyToAttack != null && Vector3.Distance(transform.position, enemyToAttack.position) <= attackRange) { AttackTarget(enemyToAttack); return; }

        // パスが空になった時の処理
        if (path == null || path.Count == 0)
        {
            if (currentState == State.Egress || currentState == State.Scattering)
            {
                currentState = State.Idle; // 初期行動が完了したらIdleへ
            }

            if (Time.time > lastPathRequestTime + pathRequestCooldown)
            {
                lastPathRequestTime = Time.time;
                RequestPath(goalBaseTransform.position);
            }
            return; // 経路がない場合はここで終了
        }

        // 経路を辿る
        currentState = State.FollowingPath;
        FollowPath();
    }

    void RequestPath(Vector3 destination)
    {
        FindPath(destination);
    }

    void FollowPath()
    {
        if (path == null || path.Count == 0) return;
        MoveTowards(path.Peek());
        if (Vector3.Distance(transform.position, path.Peek()) < 0.2f) { path.Dequeue(); }
    }

    void MoveTowards(Vector3 target) { Vector3 direction = (target - transform.position).normalized; transform.position += direction * enemy.speed * Time.deltaTime; if (spriteRenderer != null && direction.sqrMagnitude > 0.01f) { spriteRenderer.flipX = direction.x < 0; } }

    // --- A* Pathfinding Logic (Integrated & Static Access) ---
    void CreateGrid() { nodeDiameter = nodeRadius * 2; gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter); gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter); grid = new Node[gridSizeX, gridSizeY]; Vector3 worldBottomLeft = (Vector3.zero - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2); ObstacleData[] allObstacles = FindObjectsByType<ObstacleData>(FindObjectsSortMode.None); for (int x = 0; x < gridSizeX; x++) { for (int y = 0; y < gridSizeY; y++) { Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius); bool walkable = true; foreach (var obstacle in allObstacles) { if (Vector3.Distance(worldPoint, obstacle.transform.position) < obstacle.avoidanceRadius + nodeRadius) { walkable = false; break; } } grid[x, y] = new Node(walkable, worldPoint, x, y); } } }
    void FindPath(Vector3 targetPosition) { Node startNode = NodeFromWorldPoint(transform.position); Node targetNode = NodeFromWorldPoint(targetPosition); if (startNode == null || targetNode == null) { path = null; return; } if (!startNode.isWalkable) { startNode = FindClosestWalkableNode(startNode); } if (!targetNode.isWalkable) { targetNode = FindClosestWalkableNode(targetNode); } if (targetNode == null || startNode == null) { path = null; return; } List<Node> openSet = new List<Node>(); HashSet<Node> closedSet = new HashSet<Node>(); openSet.Add(startNode); startNode.gCost = 0; while (openSet.Count > 0) { Node currentNode = openSet[0]; for (int i = 1; i < openSet.Count; i++) { if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) { currentNode = openSet[i]; } } openSet.Remove(currentNode); closedSet.Add(currentNode); if (currentNode == targetNode) { RetracePath(startNode, targetNode); return; } foreach (Node neighbour in GetNeighbours(currentNode)) { if (!neighbour.isWalkable || closedSet.Contains(neighbour)) continue; int newCost = currentNode.gCost + GetDistance(currentNode, neighbour); if (newCost < neighbour.gCost || !openSet.Contains(neighbour)) { neighbour.gCost = newCost; neighbour.hCost = GetDistance(neighbour, targetNode); neighbour.parent = currentNode; if (!openSet.Contains(neighbour)) openSet.Add(neighbour); } } } path = null; }

    #region A* Helper Methods
    void RetracePath(Node start, Node end) { List<Vector3> waypoints = new List<Vector3>(); Node current = end; while (current != start) { waypoints.Add(current.worldPosition); current = current.parent; } if (waypoints.Count > 0) waypoints.Reverse(); path = new Queue<Vector3>(waypoints); }
    int GetDistance(Node a, Node b) { int dstX = Mathf.Abs(a.gridX - b.gridX); int dstY = Mathf.Abs(a.gridY - b.gridY); if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY); return 14 * dstX + 10 * (dstY - dstX); }
    Node NodeFromWorldPoint(Vector3 worldPos) { float percentX = Mathf.Clamp01((worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x); float percentY = Mathf.Clamp01((worldPos.y + gridWorldSize.y / 2) / gridWorldSize.y); int x = Mathf.RoundToInt((gridSizeX - 1) * percentX); int y = Mathf.RoundToInt((gridSizeY - 1) * percentY); if (grid != null && x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY) return grid[x, y]; return null; }
    List<Node> GetNeighbours(Node node) { List<Node> neighbours = new List<Node>(); for (int x = -1; x <= 1; x++) { for (int y = -1; y <= 1; y++) { if (x == 0 && y == 0) continue; int checkX = node.gridX + x; int checkY = node.gridY + y; if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) neighbours.Add(grid[checkX, checkY]); } } return neighbours; }
    bool IsWalkable(Vector3 worldPos) { Node node = NodeFromWorldPoint(worldPos); return node != null && node.isWalkable; }
    Vector3 GetClosestWalkablePosition(Vector3 worldPos) { Node node = NodeFromWorldPoint(worldPos); if (node != null && node.isWalkable) return node.worldPosition; Node closest = FindClosestWalkableNode(node); return closest != null ? closest.worldPosition : worldPos; }
    Node FindClosestWalkableNode(Node node) { if (node == null) return null; Queue<Node> queue = new Queue<Node>(); queue.Enqueue(node); HashSet<Node> searched = new HashSet<Node>(); searched.Add(node); while (queue.Count > 0) { Node current = queue.Dequeue(); if (current.isWalkable) return current; foreach (var neighbour in GetNeighbours(current)) { if (!searched.Contains(neighbour)) { searched.Add(neighbour); queue.Enqueue(neighbour); } } } return null; }
    #endregion

    #region Unchanged Helper Methods
    void AttackTarget(Transform target) { if (target == null) return; Vector3 dir = (target.position - transform.position).normalized; if (spriteRenderer != null) spriteRenderer.flipX = dir.x < 0; if (Time.time > lastAttackTime + attackCooldown) { lastAttackTime = Time.time; Ally ally = target.GetComponent<Ally>(); BaseHP baseHP = target.GetComponent<BaseHP>(); if (ally != null) ally.TakeDamage(enemy.damage); else if (baseHP != null) baseHP.TakeDamage(enemy.damage); } }
    void GoToGoalBaseReached() { if (goalBaseTransform == null) return; BaseHP baseHP = goalBaseTransform.GetComponent<BaseHP>(); if (baseHP != null) { baseHP.TakeDamage(enemy.currentHP); } Destroy(gameObject); }
    Transform FindClosestTargetByDistance(string[] tags) { Transform c = null; float m = float.MaxValue; if (tags == null) return null; foreach (string t in tags) { GameObject[] o = GameObject.FindGameObjectsWithTag(t); foreach (GameObject i in o) { if (i == this.gameObject || !i.activeInHierarchy) continue; float d = Vector3.Distance(transform.position, i.transform.position); if (d < m) { m = d; c = i.transform; } } } return c; }
    void OnDrawGizmos() { if (grid != null && Application.isPlaying) { Gizmos.color = Color.yellow; Gizmos.DrawWireCube(Vector3.zero, new Vector3(gridWorldSize.x, gridWorldSize.y, 1)); foreach (Node n in grid) { Gizmos.color = (n.isWalkable) ? new Color(1, 1, 1, 0.05f) : new Color(1, 0, 0, 0.2f); Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f)); } } if (path != null && path.Count > 0) { Gizmos.color = Color.cyan; Vector3 prev = transform.position; foreach (var p in path) { Gizmos.DrawLine(prev, p); Gizmos.DrawCube(p, Vector3.one * .2f); prev = p; } } if (currentState == State.Egress || currentState == State.Scattering) { Gizmos.color = Color.magenta; Gizmos.DrawLine(transform.position, initialDestination); Gizmos.DrawWireSphere(initialDestination, 0.5f); } }
    #endregion
}