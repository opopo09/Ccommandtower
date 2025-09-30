using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// このコンポーネントがアタッチされたオブジェクトを、
/// Allyが回避すべき対象として認識させるためのマーカー（目印）です。
/// オブジェクトのSpriteRendererのサイズに基づいて、自身の回避半径を自動で設定します。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))] // このコンポーネントにはSpriteRendererが必須です
public class AvoidanceTarget : MonoBehaviour
{
    // このクラスのすべてのインスタンスを保持する静的な（グローバルな）リスト
    public static readonly List<AvoidanceTarget> AllTargets = new List<AvoidanceTarget>();

    [Header("回避範囲の設定")]
    [Tooltip("このオブジェクト自身の回避半径。0のままだと下の設定に基づいて自動計算されます。")]
    public float avoidanceRadius = 0f;

    [Tooltip("自動計算された半径に対する倍率。少し大きめに避けたい場合などに調整します。")]
    public float radiusMultiplier = 1.0f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // 参照をキャッシュしておく
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 手動で半径が設定されていない場合、スプライトのサイズから自動で計算する
        if (avoidanceRadius <= 0f && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // スプライトの境界(bounds)の大きさの、幅と高さの大きい方を半径の基準とする
            float extentsX = spriteRenderer.bounds.extents.x;
            float extentsY = spriteRenderer.bounds.extents.y;
            avoidanceRadius = Mathf.Max(extentsX, extentsY) * radiusMultiplier;
        }
    }

    private void OnEnable()
    {
        // オブジェクトが有効になった時、リストに自分自身を追加する
        if (!AllTargets.Contains(this))
        {
            AllTargets.Add(this);
        }
    }

    private void OnDisable()
    {
        // オブジェクトが無効または破棄された時、リストから自分自身を削除する
        if (AllTargets.Contains(this))
        {
            AllTargets.Remove(this);
        }
    }

    // シーンビューで回避範囲を視覚的に確認するためのギズモ
    void OnDrawGizmosSelected()
    {
        // エディタ上でもサイズが分かるように、実行時と同じロジックで半径を計算して表示
        float radius = avoidanceRadius;
        if (Application.isPlaying == false && radius <= 0 && GetComponent<SpriteRenderer>() != null && GetComponent<SpriteRenderer>().sprite != null)
        {
            radius = Mathf.Max(GetComponent<SpriteRenderer>().bounds.extents.x, GetComponent<SpriteRenderer>().bounds.extents.y) * radiusMultiplier;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}