using UnityEngine;

public class GameController : MonoBehaviour
{
    public WaveManager waveManager;

    void Start()
    {
        waveManager.timeBetweenWaves = 5f;               // 直接アクセスもOK
        waveManager.SetWaveSpawnInterval(0, 0.5f);       // メソッドでもOK
        waveManager.SetWaveDuration(0, 20f);
    }
}

