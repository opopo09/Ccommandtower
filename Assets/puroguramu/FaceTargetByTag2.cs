using UnityEngine;

public class FaceTargetByTag2 : MonoBehaviour
{
    public string targetTag = "Enemy"; // 敵のタグ

    void Update()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject nearest = GetNearestTarget(targets);

        if (nearest != null)
        {
            Vector3 dir = nearest.transform.position - transform.position;

            if (dir.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1 : 1); // 左向きならマイナス
                transform.localScale = scale;
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
