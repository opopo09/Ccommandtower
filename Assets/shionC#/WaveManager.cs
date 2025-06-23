using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject[] enemyPrefabs;
        public int enemyCount = 5;

        // publicで外から自由にいじれるように
        public float spawnInterval = 1f;
        public float waveDuration = 30f;
    }

    public Wave[] waves;

    public Transform[] spawnPoints;

    // publicにしてインスペクターや他クラスから直接変更可能に
    public float timeBetweenWaves = 10f;

    public Text waveTimerText;
    public Text waveDurationText;

    private int currentWaveIndex = 0;
    private float intermissionTimer = 0f;
    private float waveTimeLeft = 0f;

    private bool isIntermission = true;
    private bool spawning = false;

    void Start()
    {
        intermissionTimer = timeBetweenWaves;
        UpdateIntermissionText();
    }

    void Update()
    {
        if (currentWaveIndex >= waves.Length) return;

        if (isIntermission)
        {
            intermissionTimer -= Time.deltaTime;
            UpdateIntermissionText();

            if (intermissionTimer <= 0f)
            {
                isIntermission = false;
                StartCoroutine(SpawnWave(waves[currentWaveIndex]));
                waveTimeLeft = waves[currentWaveIndex].waveDuration;
            }
        }
        else
        {
            waveTimeLeft -= Time.deltaTime;
            UpdateWaveTimerText();

            if (waveTimeLeft <= 0f && spawning == false)
            {
                EndWave();
            }
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        spawning = true;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject prefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(prefab, point.position, Quaternion.identity);
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        spawning = false;
    }

    void EndWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= waves.Length)
        {
            waveTimerText.text = "All waves completed!";
            waveDurationText.text = "";
            return;
        }

        isIntermission = true;
        intermissionTimer = timeBetweenWaves;
        UpdateIntermissionText();
        waveDurationText.text = "";
    }

    void UpdateIntermissionText()
    {
        if (waveTimerText != null)
        {
            waveTimerText.gameObject.SetActive(true);  // インターバル用テキストを表示
            waveTimerText.text = $"Next Wave: {intermissionTimer:F1} sec";
        }
        if (waveDurationText != null)
        {
            waveDurationText.gameObject.SetActive(false); // ウェーブ用テキストを非表示
        }
    }

    void UpdateWaveTimerText()
    {
        if (waveDurationText != null)
        {
            waveDurationText.gameObject.SetActive(true);   // ウェーブ用テキストを表示
            waveDurationText.text = $"Wave Time Left: {waveTimeLeft:F1} sec";
        }
        if (waveTimerText != null)
        {
            waveTimerText.gameObject.SetActive(false);    // インターバル用テキストを非表示
        }
    }


    // publicで外部から呼べるようにした関数（オプション）

    public void SetTimeBetweenWaves(float time)
    {
        timeBetweenWaves = time;
        if (isIntermission)
        {
            intermissionTimer = timeBetweenWaves;
            UpdateIntermissionText();
        }
    }

    public void SetWaveSpawnInterval(int waveIndex, float interval)
    {
        if (waveIndex >= 0 && waveIndex < waves.Length)
        {
            waves[waveIndex].spawnInterval = interval;
        }
    }

    public void SetWaveDuration(int waveIndex, float duration)
    {
        if (waveIndex >= 0 && waveIndex < waves.Length)
        {
            waves[waveIndex].waveDuration = duration;
        }
    }
    
}

