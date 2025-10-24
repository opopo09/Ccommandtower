using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAI : MonoBehaviour
{
    [Header("çıìG & çUåÇê›íË")]
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [Header("É^Å[ÉQÉbÉgê›íË")]
    public string[] highPriorityTags;
    public string[] midPriorityTags;
    public string[] lowPriorityTags;

    [Header("ÉSÅ[Éãê›íË")]
    public string goalBaseTag = "base";

    [Header("ÉiÉrÉQÅ[ÉVÉáÉìê›íË (ç≈èdóv)")]
    public Vector2 gridWorldSize = new Vector2(50, 50);
    public float nodeRadius = 0.3f;
    public int enemyAvoidancePenalty = 50;

    [Header("êÌó™çsìÆê›íË")]
    public float initialScatterRadius = 3.0f;
    public float goalApproachRadius = 5.0f;

    public bool IsAttacking { get; private set; }

    private enum State { Egress, Scattering, ApproachingGoal, GoingToGoal, FollowingPath, Attacking, Idle }
    private State currentState;

    private Transform goalBaseTransform;
    private float lastAttackTime = -999f;
    private Enemy enemy;
    private SpriteRenderer spriteRenderer;
    private Queue<Vector3> path;
    private Vector3 currentDestination;
    private Transform currentTarget;

    private static Node[,] grid;
    private static int gridSizeX, gridSizeY;
    private static float nodeDiameter;
    private static bool isGridCreated = false;
    private static Vector2 staticGridWorldSize;
    private static List<Transform> allEnemyTransforms = new List<Transform>();

    private class Node
    {
        public bool isWalkable; public Vector3 worldPosition; public int gridX, gridY;
        public int gCost, hCost; public Node parent;
        public int movementPenalty;
        public int fCost { get { return gCost + hCost + movementPenalty; } }
        public Node(bool walkable, Vector3 worldPos, int _gridX, int _gridY)
        {
            isWalkable = walkable; worldPosition = worldPos; gridX = _gridX; gridY = _gridY; movementPenalty = 0;
        }
    }

    #region Public Static API for other scripts
    public static bool IsPositionWalkable(Vector3 worldPos) { if (!isGridCreated) return false; Node node = NodeFromWorldPoint_Static(worldPos); return node != null && node.isWalkable; }

    public static Queue<Vector3> RequestPathFromAI(Vector3 startPos, Vector3 endPos, bool avoidEnemies, int penalty)
    {
        if (!isGridCreated) return null;
        return FindPath_Static(startPos, endPos, avoidEnemies, penalty);
    }
    #endregion

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!isGridCreated) { staticGridWorldSize = gridWorldSize; CreateGrid(); isGridCreated = true; }
    }

    void OnEnable() { if (OffScreenIndicatorManager.instance != null) OffScreenIndicatorManager.instance.AddEnemy(this); if (!allEnemyTransforms.Contains(this.transform)) allEnemyTransforms.Add(this.transform); }
    void OnDisable() { if (OffScreenIndicatorManager.instance != null) OffScreenIndicatorManager.instance.RemoveEnemy(this); if (allEnemyTransforms.Contains(this.transform)) allEnemyTransforms.Remove(this.transform); }

    void Start()
    {
        GameObject baseObject = GameObject.FindGameObjectWithTag(goalBaseTag);
        if (baseObject != null) { goalBaseTransform = baseObject.transform; }
        else { Debug.LogError(gameObject.name + " Ç™ÉSÅ[ÉãÇå©Ç¬ÇØÇÁÇÍÇ‹ÇπÇÒÇ≈ÇµÇΩÅB"); this.enabled = false; return; }

        if (isGridCreated && !IsWalkable(transform.position))
        {
            currentState = State.Egress;
        }
        else if (initialScatterRadius > 0)
        {
            currentState = State.Scattering;
        }
        else
        {
            currentState = State.ApproachingGoal;
        }
        PlanNextMove();
    }

    void Update()
    {
        if (enemy == null || goalBaseTransform == null || !isGridCreated) return;
        if (Vector3.Distance(transform.position, goalBaseTransform.position) < 1.0f) { GoToGoalBaseReached(); return; }

        Transform enemyToAttack = FindClosestTargetByDistance(highPriorityTags) ?? FindClosestTargetByDistance(midPriorityTags) ?? FindClosestTargetByDistance(lowPriorityTags);
        Transform newTarget = (enemyToAttack != null && Vector3.Distance(transform.position, enemyToAttack.position) <= detectionRange) ? enemyToAttack : goalBaseTransform;

        if (newTarget != goalBaseTransform && Vector3.Distance(transform.position, newTarget.position) <= attackRange)
        {
            IsAttacking = true;
            AttackTarget(newTarget);
            path?.Clear();
            return;
        }
        IsAttacking = false;

        if (currentTarget != newTarget || (path == null || path.Count == 0))
        {
            currentTarget = newTarget;
            PlanNextMove();
        }

        FollowPath();
    }

    void PlanNextMove()
    {
        if (path != null && path.Count > 0) return;

        if (currentState == State.Egress) { currentState = State.Scattering; }
        else if (currentState == State.Scattering) { currentState = State.ApproachingGoal; }
        else if (currentState == State.ApproachingGoal) { currentState = State.GoingToGoal; }

        Vector3 destination;
        switch (currentState)
        {
            case State.Egress:
                destination = GetClosestWalkablePosition(transform.position);
                break;
            case State.Scattering:
                destination = transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * initialScatterRadius);
                break;
            case State.ApproachingGoal:
                destination = (currentTarget != null) ? currentTarget.position : goalBaseTransform.position;
                if (currentTarget == goalBaseTransform && goalApproachRadius > 0)
                {
                    Vector3 randomOffset = (Vector3)UnityEngine.Random.insideUnitCircle.normalized * goalApproachRadius;
                    Vector3 approachPoint = goalBaseTransform.position + randomOffset;
                    destination = GetClosestWalkablePosition(approachPoint);
                }
                break;
            case State.GoingToGoal:
            default:
                destination = (currentTarget != null) ? currentTarget.position : goalBaseTransform.position;
                break;
        }
        currentDestination = destination;
        RequestPath(currentDestination);
    }

    // Å•Å•Å•Å•Å•ÅyCRITICAL FIXÅzÅ•Å•Å•Å•Å•
    void RequestPath(Vector3 destination)
    {
        // EnemyAI does not need to avoid other enemies, so 'avoidEnemies' is false.
        path = RequestPathFromAI(transform.position, destination, false, 0);
    }
    // Å£Å£Å£Å£Å£ÅyCRITICAL FIXÅzÅ£Å£Å£Å£Å£

    void FollowPath() { if (path != null && path.Count > 0) { MoveTowards(path.Peek()); if (Vector3.Distance(transform.position, path.Peek()) < 0.2f) { path.Dequeue(); } } else { PlanNextMove(); } }
    void MoveTowards(Vector3 target) { Vector3 direction = (target - transform.position).normalized; transform.position += direction * enemy.speed * Time.deltaTime; if (spriteRenderer != null && direction.sqrMagnitude > 0.01f) { spriteRenderer.flipX = direction.x < 0; } }

    void CreateGrid() { nodeDiameter = nodeRadius * 2; gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter); gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter); grid = new Node[gridSizeX, gridSizeY]; Vector3 worldBottomLeft = (Vector3.zero - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2); ObstacleData[] allObstacles = FindObjectsByType<ObstacleData>(FindObjectsSortMode.None); for (int x = 0; x < gridSizeX; x++) { for (int y = 0; y < gridSizeY; y++) { Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius); bool walkable = true; foreach (var obstacle in allObstacles) { if (Vector3.Distance(worldPoint, obstacle.transform.position) < obstacle.avoidanceRadius + nodeRadius) { walkable = false; break; } } grid[x, y] = new Node(walkable, worldPoint, x, y); } } }

    #region A* Pathfinding Logic
    private static void UpdateGridPenalties(int penalty) { if (!isGridCreated) return; foreach (Node n in grid) { n.movementPenalty = 0; } foreach (Transform enemyTransform in allEnemyTransforms) { if (enemyTransform == null) continue; Node enemyNode = NodeFromWorldPoint_Static(enemyTransform.position); if (enemyNode != null) { enemyNode.movementPenalty = penalty; } } }
    private static Queue<Vector3> FindPath_Static(Vector3 startPos, Vector3 endPos, bool avoidEnemies, int penalty) { if (avoidEnemies) { UpdateGridPenalties(penalty); } else { UpdateGridPenalties(0); } Node startNode = NodeFromWorldPoint_Static(startPos); Node targetNode = NodeFromWorldPoint_Static(endPos); if (startNode == null || targetNode == null) { return null; } if (!startNode.isWalkable) { startNode = FindClosestWalkableNode_Static(startNode); } if (!targetNode.isWalkable) { targetNode = FindClosestWalkableNode_Static(targetNode); } if (targetNode == null || startNode == null) { return null; } List<Node> openSet = new List<Node>(); HashSet<Node> closedSet = new HashSet<Node>(); openSet.Add(startNode); startNode.gCost = 0; while (openSet.Count > 0) { Node currentNode = openSet[0]; for (int i = 1; i < openSet.Count; i++) { if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) { currentNode = openSet[i]; } } openSet.Remove(currentNode); closedSet.Add(currentNode); if (currentNode == targetNode) { return RetracePath_Static(startNode, targetNode); } foreach (Node neighbour in GetNeighbours_Static(currentNode)) { if (!neighbour.isWalkable || closedSet.Contains(neighbour)) continue; int newCost = currentNode.gCost + GetDistance_Static(currentNode, neighbour) + neighbour.movementPenalty; if (newCost < neighbour.gCost || !openSet.Contains(neighbour)) { neighbour.gCost = newCost; neighbour.hCost = GetDistance_Static(neighbour, targetNode); neighbour.parent = currentNode; if (!openSet.Contains(neighbour)) openSet.Add(neighbour); } } } return null; }
    #endregion

    #region A* Helper Methods
    static Queue<Vector3> RetracePath_Static(Node start, Node end) { List<Vector3> waypoints = new List<Vector3>(); Node current = end; while (current != start) { waypoints.Add(current.worldPosition); current = current.parent; } if (waypoints.Count > 0) waypoints.Reverse(); return new Queue<Vector3>(waypoints); }
    static int GetDistance_Static(Node a, Node b) { int dstX = Mathf.Abs(a.gridX - b.gridX); int dstY = Mathf.Abs(a.gridY - b.gridY); if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY); return 14 * dstX + 10 * (dstY - dstX); }
    static Node NodeFromWorldPoint_Static(Vector3 worldPos) { float percentX = Mathf.Clamp01((worldPos.x + staticGridWorldSize.x / 2) / staticGridWorldSize.x); float percentY = Mathf.Clamp01((worldPos.y + staticGridWorldSize.y / 2) / staticGridWorldSize.y); int x = Mathf.RoundToInt((gridSizeX - 1) * percentX); int y = Mathf.RoundToInt((gridSizeY - 1) * percentY); if (grid != null && x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY) return grid[x, y]; return null; }
    static List<Node> GetNeighbours_Static(Node node) { List<Node> neighbours = new List<Node>(); for (int x = -1; x <= 1; x++) { for (int y = -1; y <= 1; y++) { if (x == 0 && y == 0) continue; int checkX = node.gridX + x; int checkY = node.gridY + y; if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) neighbours.Add(grid[checkX, checkY]); } } return neighbours; }
    bool IsWalkable(Vector3 worldPos) { Node node = NodeFromWorldPoint_Static(worldPos); return node != null && node.isWalkable; }
    Vector3 GetClosestWalkablePosition(Vector3 worldPos) { Node node = NodeFromWorldPoint_Static(worldPos); if (node != null && node.isWalkable) return node.worldPosition; Node closest = FindClosestWalkableNode_Static(node); return closest != null ? closest.worldPosition : worldPos; }
    static Node FindClosestWalkableNode_Static(Node node) { if (node == null) return null; Queue<Node> queue = new Queue<Node>(); queue.Enqueue(node); HashSet<Node> searched = new HashSet<Node>(); searched.Add(node); while (queue.Count > 0) { Node current = queue.Dequeue(); if (current.isWalkable) return current; foreach (var neighbour in GetNeighbours_Static(current)) { if (!searched.Contains(neighbour)) { searched.Add(neighbour); queue.Enqueue(neighbour); } } } return null; }
    #endregion

    #region Unchanged Helper Methods
    void AttackTarget(Transform target) { if (target == null) return; Vector3 dir = (target.position - transform.position).normalized; if (spriteRenderer != null) spriteRenderer.flipX = dir.x < 0; if (Time.time > lastAttackTime + attackCooldown) { lastAttackTime = Time.time; Ally ally = target.GetComponent<Ally>(); BaseHP baseHP = target.GetComponent<BaseHP>(); Tower tower = target.GetComponent<Tower>(); if (tower != null) { tower.TakeDamage(enemy.damage); return; } if (ally != null) { ally.TakeDamage(enemy.damage); return; } if (baseHP != null) { baseHP.TakeDamage(enemy.damage); } } }
    void GoToGoalBaseReached() { if (goalBaseTransform == null) return; BaseHP baseHP = goalBaseTransform.GetComponent<BaseHP>(); if (baseHP != null) { baseHP.TakeDamage(enemy.currentHP); } Destroy(gameObject); }
    Transform FindClosestTargetByDistance(string[] tags) { Transform c = null; float m = float.MaxValue; if (tags == null) return null; foreach (string t in tags) { if (string.IsNullOrEmpty(t)) continue; GameObject[] o = GameObject.FindGameObjectsWithTag(t); foreach (GameObject i in o) { if (i == this.gameObject || !i.activeInHierarchy) continue; Tower tower = i.GetComponent<Tower>(); if (tower != null && tower.IsDestroyed) continue; float d = Vector3.Distance(transform.position, i.transform.position); if (d < m) { m = d; c = i.transform; } } } return c; }
    void OnDrawGizmos() { if (grid != null && Application.isPlaying) { Gizmos.color = Color.yellow; Gizmos.DrawWireCube(Vector3.zero, new Vector3(gridWorldSize.x, gridWorldSize.y, 1)); } if (path != null && path.Count > 0) { Gizmos.color = Color.cyan; Vector3 prev = transform.position; foreach (var p in path) { Gizmos.DrawLine(prev, p); prev = p; } } if (currentState == State.Egress || currentState == State.Scattering) { Gizmos.color = Color.magenta; Gizmos.DrawLine(transform.position, currentDestination); Gizmos.DrawWireSphere(currentDestination, 0.5f); } }
    #endregion
}