using UnityEngine;

public class GameController : MonoBehaviour
{
    public WaveManager waveManager;

    void Start()
    {
        waveManager.timeBetweenWaves = 5f;               // ���ڃA�N�Z�X��OK
        waveManager.SetWaveSpawnInterval(0, 0.5f);       // ���\�b�h�ł�OK
        waveManager.SetWaveDuration(0, 20f);
    }
}

