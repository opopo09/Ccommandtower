using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを使用するために必要

public class BaseController : MonoBehaviour
{
    private Image baseImage;

    void Start()
    {
        baseImage = GetComponent<Image>();
        // 初期状態は半透明
        var tempColor = baseImage.color;
        tempColor.a = 0.5f;
        baseImage.color = tempColor;
    }

    // Imageがクリックされたときに呼び出される
    public void OnBaseClicked()
    {
        // 不透明にする
        var tempColor = baseImage.color;
        tempColor.a = 1f;
        baseImage.color = tempColor;

        // ここに基地が展開されるアニメーションやエフェクトの処理を追加
        Debug.Log("基地が展開されました！");
    }
}