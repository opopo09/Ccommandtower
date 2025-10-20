using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAI : MonoBehaviour
{
    [Header("é©å»ñhâq & çUåÇê›íË")]
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
    private float pathRequestCooldown = 1f;
    private float lastPathRequestTime;
    private Vector3 currentDestination;

    private static Node[,] grid;
    private static int gridSizeX, gridSizeY;
    private static float nodeDiameter;
    private static bool isGridCreated = false;
    private static Vector2 staticGridWorldSize;

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

    #region Public Static API for other scripts
    public static bool IsPositionWalkable(Vector3 worldPos)
    {
        if (!isGridCreated) return false;
        Node node = NodeFromWorldPoint_Static(worldPos);
        return node != null && node.isWalkable;
    }

    public static Queue<Vector3> RequestPathFromAI(Vector3 startPos, Vector3 endPos)
    {
        if (!isGridCreated) return null;
        Node startNode = NodeFromWorldPoint_Static(startPos);
        Node targetNode = NodeFromWorldPoint_Static(endPos);
        if (startNode == null || targetNode == null) return null;
        if (!startNode.isWalkable) { startNode = FindClosestWalkableNode_Static(startNode); }
        if (!targetNode.isWalkable) { targetNode = FindClosestWalkableNode_Static(targetNode); }
        if (targetNode == null || startNode == null) return null;
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        startNode.gCost = 0;
        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++) { if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) { currentNode = openSet[i]; } }
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            if (currentNode == targetNode) { return RetracePath_Static(startNode, targetNode); }
            foreach (Node neighbour in GetNeighbours_Static(currentNode))
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour)) continue;
                int newCost = currentNode.gCost + GetDistance_Static(currentNode, neighbour);
                if (newCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCost;
                    neighbour.hCost = GetDistance_Static(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                }
            }
        }
        return null;
    }
    #endregion

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!isGridCreated)
        {
            staticGridWorldSize = gridWorldSize;
            CreateGrid();
            isGridCreated = true;
        }
    }

    void OnEnable() { if (OffScreenIndicatorManager.instance != null) OffScreenIndicatorManager.instance.AddEnemy(this); }
    void OnDisable() { if (OffScreenIndicatorManager.instance != null) OffScreenIndicatorManager.instance.RemoveEnemy(this); }

    void Start()
    {
        GameObject baseObject = GameObject.FindGameObjectWithTag(goalBaseTag);
        if (baseObject != null) { goalBaseTransform = baseObject.transform; }
        else { Debug.LogError(gameObject.name + " Ç™ÉSÅ[ÉãÇå©Ç¬ÇØÇÁÇÍÇ‹ÇπÇÒÇ≈ÇµÇΩÅB"); this.enabled = false; return; }

        if (isGridCreated && !IsWalkable(transform.position))
        {
            currentState = State.Egress;
            SetNewDestinationAndRequestPath();
        }
        else if (initialScatterRadius > 0)
        {
            currentState = State.Scattering;
            SetNewDestinationAndRequestPath();
        }
        else
        {
            currentState = State.Idle;
        }
    }

    void Update()
    {
        if (enemy == null || goalBaseTransform == null || !isGridCreated) return;

        if (Vector3.Distance(transform.position, goalBaseTransform.position) < 1.0f) { GoToGoalBaseReached(); return; }

        Transform enemyToAttack = FindClosestTargetByDistance(highPriorityTags) ?? FindClosestTargetByDistance(midPriorityTags) ?? FindClosestTargetByDistance(lowPriorityTags);

        if (enemyToAttack != null && Vector3.Distance(transform.position, enemyToAttack.position) <= attackRange)
        {
            IsAttacking = true;
            currentState = State.Attacking;
            AttackTarget(enemyToAttack);
            return;
        }

        IsAttacking = false;

        if (path == null || path.Count == 0)
        {
            if (Time.time > lastPathRequestTime + pathRequestCooldown)
            {
                lastPathRequestTime = Time.time;
                SetNewDestinationAndRequestPath();
            }
        }
        else
        {
            if (currentState != State.Attacking) currentState = State.FollowingPath;
            FollowPath();
        }
    }

    void SetNewDestinationAndRequestPath()
    {
        if (currentState == State.Egress || currentState == State.Scattering) { currentState = State.Idle; }
        if (currentState == State.Idle) { currentState = State.ApproachingGoal; }
        else if (currentState == State.ApproachingGoal) { currentState = State.GoingToGoal; }

        switch (currentState)
        {
            case State.Egress: currentDestination = GetClosestWalkablePosition(transform.position); break;
            case State.Scattering: currentDestination = transform.position + (Vector3)(Random.insideUnitCircle * initialScatterRadius); break;
            case State.ApproachingGoal:
                currentDestination = goalBaseTransform.position;
                if (goalApproachRadius > 0)
                {
                    Vector3 randomOffset = (Vector3)Random.insideUnitCircle.normalized * goalApproachRadius;
                    Vector3 approachPoint = goalBaseTransform.position + randomOffset;
                    if (IsWalkable(approachPoint)) { currentDestination = approachPoint; }
                }
                break;
            case State.GoingToGoal: currentDestination = goalBaseTransform.position; break;
            default: currentDestination = goalBaseTransform.position; break;
        }
        RequestPath(currentDestination);
    }

    void RequestPath(Vector3 destination) { path = RequestPathFromAI(transform.position, destination); }
    void FollowPath() { if (path == null || path.Count == 0) { return; } MoveTowards(path.Peek()); if (Vector3.Distance(transform.position, path.Peek()) < 0.2f) { path.Dequeue(); } }
    void MoveTowards(Vector3 target) { Vector3 direction = (target - transform.position).normalized; transform.position += direction * enemy.speed * Time.deltaTime; if (spriteRenderer != null && direction.sqrMagnitude > 0.01f) { spriteRenderer.flipX = direction.x < 0; } }

    void CreateGrid() { nodeDiameter = nodeRadius * 2; gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter); gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter); grid = new Node[gridSizeX, gridSizeY]; Vector3 worldBottomLeft = (Vector3.zero - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2); ObstacleData[] allObstacles = FindObjectsByType<ObstacleData>(FindObjectsSortMode.None); for (int x = 0; x < gridSizeX; x++) { for (int y = 0; y < gridSizeY; y++) { Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius); bool walkable = true; foreach (var obstacle in allObstacles) { if (Vector3.Distance(worldPoint, obstacle.transform.position) < obstacle.avoidanceRadius + nodeRadius) { walkable = false; break; } } grid[x, y] = new Node(walkable, worldPoint, x, y); } } }

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
    void AttackTarget(Transform target) { if (target == null) return; Vector3 dir = (target.position - transform.position).normalized; if (spriteRenderer != null) spriteRenderer.flipX = dir.x < 0; if (Time.time > lastAttackTime + attackCooldown) { lastAttackTime = Time.time; Ally ally = target.GetComponent<Ally>(); BaseHP baseHP = target.GetComponent<BaseHP>(); if (ally != null) ally.TakeDamage(enemy.damage); else if (baseHP != null) baseHP.TakeDamage(enemy.damage); } }
    void GoToGoalBaseReached() { if (goalBaseTransform == null) return; BaseHP baseHP = goalBaseTransform.GetComponent<BaseHP>(); if (baseHP != null) { baseHP.TakeDamage(enemy.currentHP); } Destroy(gameObject); }
    Transform FindClosestTargetByDistance(string[] tags) { Transform c = null; float m = float.MaxValue; if (tags == null) return null; foreach (string t in tags) { GameObject[] o = GameObject.FindGameObjectsWithTag(t); foreach (GameObject i in o) { if (i == this.gameObject || !i.activeInHierarchy) continue; float d = Vector3.Distance(transform.position, i.transform.position); if (d < m) { m = d; c = i.transform; } } } return c; }
    void OnDrawGizmos() { if (grid != null && Application.isPlaying) { Gizmos.color = Color.yellow; Gizmos.DrawWireCube(Vector3.zero, new Vector3(gridWorldSize.x, gridWorldSize.y, 1)); } if (path != null && path.Count > 0) { Gizmos.color = Color.cyan; Vector3 prev = transform.position; foreach (var p in path) { Gizmos.DrawLine(prev, p); prev = p; } } }
    #endregion
}