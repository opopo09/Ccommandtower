// Enemy.cs
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    public virtual void TakeDamage(float damage)
    {
        Debug.Log($"Enemy が {damage} ダメージを受けた（仮）");
    }
}

