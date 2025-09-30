using UnityEngine;

public class PathManager : MonoBehaviour
{
    // このクラスの唯一のインスタンスを保持する（司令塔は世界に一つだけ）
    public static PathManager instance;

    // シーンに存在する全ての道を、ここに登録する
    public Path[] allPaths;

    private void Awake()
    {
        // シングルトンパターンの実装
        // もし、すでに司令塔が存在するなら、このオブジェクトは不要なので破壊する
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // このオブジェクトを、世界で唯一の司令塔として登録する
        instance = this;
    }

    // 敵から「ランダムな道を一つください」と頼まれた時に呼ばれる関数
    public Path GetRandomPath()
    {
        // 道が一つも登録されていなければ、何もしない（エラー防止）
        if (allPaths == null || allPaths.Length == 0)
        {
            Debug.LogError("PathManagerに道が一つも登録されていません！");
            return null;
        }

        // 登録されている道の中から、ランダムに一つ選ぶ
        int randomIndex = Random.Range(0, allPaths.Length);

        // 選んだ道を返す
        return allPaths[randomIndex];
    }
}