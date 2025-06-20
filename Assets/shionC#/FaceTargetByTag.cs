using UnityEngine;

public class FaceTargetByTag : MonoBehaviour
{
    public string targetTag = "Enemy"; // 向きたい相手のタグ（例：Enemy）
    public SpriteRenderer spriteRenderer; // 右向きがデフォルトのスプライト

    void Update()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject nearest = GetNearestTarget(targets);

        if (nearest != null)
        {
            Vector3 dir = nearest.transform.position - transform.position;

            // x方向を見て、右向き or 左向きにする
            if (dir.x != 0)
            {
                spriteRenderer.flipX = dir.x < 0;
            }
        }
    }

    GameObject GetNearestTarget(GameObject[] objects)
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject obj in objects)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = obj;
            }
        }

        return closest;
    }
}

