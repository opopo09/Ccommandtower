// Enemy.cs
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    public virtual void TakeDamage(float damage)
    {
        Debug.Log($"Enemy �� {damage} �_���[�W���󂯂��i���j");
    }
}

