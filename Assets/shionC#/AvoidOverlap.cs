using UnityEngine;
using System.Collections.Generic;

public class ObjectAvoiderManager : MonoBehaviour
{
    [Header("���������I�u�W�F�N�g�̃^�O")]
    public string[] targetTags = { "Ally", "Enemy", "Item", "Dragon" };

    [Header("�����锼�a(�����ȉ��͔�����)")]
    public float avoidRadius = 1.0f;

    [Header("�����߂����x")]
    public float pushForce = 3.0f;

    void Update()
    {
        // �ΏۃI�u�W�F�N�g��S���܂Ƃ߂�
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

        // �y�A�ŋ߂�������̂������߂�
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
                    float pushAmount = (avoidRadius - dist) * 0.5f; // �݂��ɔ����������߂���

                    // �ړ�
                    t1.position += pushDir * pushAmount * pushForce * Time.deltaTime;
                    t2.position -= pushDir * pushAmount * pushForce * Time.deltaTime;
                }
            }
        }
    }
}
