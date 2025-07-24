using UnityEngine;
using System.Collections.Generic;

public class ObjectAvoiderManager : MonoBehaviour
{
    [Header("避けたいオブジェクトのタグ")]
    public string[] targetTags = { "Ally", "Enemy", "Item", "Dragon" };

    [Header("避ける半径（この距離未満なら押し戻す）")]
    public float avoidRadius = 1.0f;

    [Header("押し戻しの強さ（1～10推奨）")]
    public float pushForce = 3.0f;

    [Header("スケールを考慮して判定する")]
    public bool useScaleSize = true;

    [Header("Rigidbody を持つオブジェクトにも対応")]
    public bool useRigidbodyIfExists = true;

    private List<Transform> objectsToAvoid = new List<Transform>();

    void FixedUpdate()
    {
        CollectActiveTargets();

        // オフセットを一時的に記憶する辞書 (Transform -> Vector3)
        Dictionary<Transform, Vector3> moveOffsets = new Dictionary<Transform, Vector3>();

        int count = objectsToAvoid.Count;
        for (int i = 0; i < count; i++)
        {
            Transform t1 = objectsToAvoid[i];
            if (t1 == null) continue;

            for (int j = i + 1; j < count; j++)
            {
                Transform t2 = objectsToAvoid[j];
                if (t2 == null) continue;

                Vector3 dir = t1.position - t2.position;
                dir.y = 0f; // 2DゲームならYは無視、3Dなら削除しても良い

                float dist = dir.magnitude;

                if (dist == 0f)
                {
                    // 完全重なりはランダム方向に押す
                    dir = Random.insideUnitSphere;
                    dir.y = 0f;
                    dist = 0.001f;
                }

                float effectiveRadius = avoidRadius;
                if (useScaleSize)
                {
                    float scale1 = t1.localScale.magnitude;
                    float scale2 = t2.localScale.magnitude;
                    effectiveRadius = avoidRadius * 0.5f * (scale1 + scale2);
                }

                if (dist < effectiveRadius)
                {
                    float pushAmount = (effectiveRadius - dist) * 0.5f * pushForce;

                    Vector3 pushDir = dir.normalized;
                    Vector3 offset = pushDir * pushAmount * Time.fixedDeltaTime;

                    // それぞれのオフセットを蓄積
                    if (!moveOffsets.ContainsKey(t1)) moveOffsets[t1] = Vector3.zero;
                    if (!moveOffsets.ContainsKey(t2)) moveOffsets[t2] = Vector3.zero;

                    moveOffsets[t1] += offset;
                    moveOffsets[t2] -= offset;
                }
            }
        }

        // 蓄積したオフセットを一括適用
        foreach (var kvp in moveOffsets)
        {
            ApplyOffset(kvp.Key, kvp.Value);
        }
    }

    void CollectActiveTargets()
    {
        objectsToAvoid.Clear();

        foreach (string tag in targetTags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objs)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    objectsToAvoid.Add(obj.transform);
                }
            }
        }
    }

    void ApplyOffset(Transform t, Vector3 offset)
    {
        if (useRigidbodyIfExists)
        {
            Rigidbody rb = t.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.MovePosition(rb.position + offset);
                return;
            }
        }
        t.position += offset;
    }
}
