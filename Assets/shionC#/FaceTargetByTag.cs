using UnityEngine;

public class FaceTargetByTag : MonoBehaviour
{
    public string targetTag = "Enemy"; // ������������̃^�O�i��FEnemy�j
    public SpriteRenderer spriteRenderer; // �E�������f�t�H���g�̃X�v���C�g

    void Update()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject nearest = GetNearestTarget(targets);

        if (nearest != null)
        {
            Vector3 dir = nearest.transform.position - transform.position;

            // x���������āA�E���� or �������ɂ���
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

