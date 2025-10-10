#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class TileAssetCreator
{
    // この関数をUnityのメニューに追加する
    [MenuItem("Assets/Create/2D/My Basic Tile")]
    public static void CreateBasicTile()
    {
        // 1. 新しいタイルアセットのインスタンスを作成する
        var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();

        // 2. 現在Projectウィンドウで選択しているフォルダのパスを取得する
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets"; // もし何も選択されていなければ、Assetsフォルダを基準にする
        }
        else if (System.IO.Path.GetExtension(path) != "")
        {
            path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        // 3. アセットのファイルパスを作成し、重複しないようにする
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Basic Tile.asset");

        // 4. アセットをプロジェクトに保存する
        AssetDatabase.CreateAsset(tile, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 5. 作成したアセットを選択状態にする
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = tile;
    }
}
#endif