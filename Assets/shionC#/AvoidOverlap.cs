using UnityEngine;
using System.Collections.Generic;

public class ObjectAvoiderManager : MonoBehaviour
{
    [Header("避けたいオブジェクトのタグ")]
    public string[] targetTags = { "Ally", "Enemy", "Item", "Dragon" };

    [Header("避ける半径(距離以下は避ける)")]
    public float avoidRadius = 1.0f;

    [Header("押し戻し速度")]
    public float pushForce = 3.0f;

    void Update()
    {
        // 対象オブジェクトを全部まとめる
        List<Transform> objectsToAvoid = new List<Transform>();

        foreach (var tag in targetTags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (var obj in objs)
            {
                if (obj.activeInHierarchy)
                {
                    objectsToAvoid.Add(obj.transform);
                }
            }
        }

        // ペアで近すぎるものを押し戻す
        int count = objectsToAvoid.Count;
        for (int i = 0; i < count; i++)
        {
            Transform t1 = objectsToAvoid[i];
            for (int j = i + 1; j < count; j++)
            {
                Transform t2 = objectsToAvoid[j];

                Vector3 dir = t1.position - t2.position;
                float dist = dir.magnitude;

                if (dist > 0f && dist < avoidRadius)
                {
                    Vector3 pushDir = dir.normalized;
                    float pushAmount = (avoidRadius - dist) * 0.5f; // 互いに半分ずつ押し戻す量

                    // 移動
                    t1.position += pushDir * pushAmount * pushForce * Time.deltaTime;
                    t2.position -= pushDir * pushAmount * pushForce * Time.deltaTime;
                }
            }
        }
    }
}
