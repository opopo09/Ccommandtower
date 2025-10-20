using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyGroup
    {
        public GameObject enemyPrefab;
        public int count = 5;
        public float spawnInterval = 1f;
        public float delay = 0f;
        [Header("倍率パラメータ")]
        public float hpMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
    }

    [System.Serializable]
    public class Wave
    {
        public EnemyGroup[] enemyGroups;
        public float waveDuration = 30f;
        [Tooltip("このウェーブ中の敵の最大存在数")]
        public int spawnLimit = 10;
    }

    [Header("連携するコンポーネント")]
    [Tooltip("シーンに配置されているSpawnLimitCheckerをここに設定")]
    public SpawnLimitChecker spawnLimitChecker;

    public Wave[] waves;
    public Transform[] spawnPoints;
    public float timeBetweenWaves = 10f;
    public Text waveTimerText;
    public Text waveDurationText;
    public Text waveCountText;
    public string nextSceneName = "ResultScene";

    private int currentWaveIndex = 0;
    private float intermissionTimer = 0f;
    private float waveTimeLeft = 0f;
    private bool isIntermission = true;
    private bool waveInProgress = false;
    private bool spawningWave = false;
    private bool allWavesCompleted = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        if (spawnLimitChecker == null)
        {
            Debug.LogError("WaveManager: spawnLimitCheckerが設定されていません！インスペクターを確認してください。", this.gameObject);
        }
        currentWaveIndex = 0;
        isIntermission = true;
        intermissionTimer = timeBetweenWaves;
        waveTimeLeft = 0f;
        spawnedEnemies.Clear();
        waveInProgress = false;
        spawningWave = false;
        allWavesCompleted = false;
        UpdateIntermissionText();
        UpdateWaveCountText();
        UpdateWaveTimerText();
    }

    void Update()
    {
        if (allWavesCompleted) return;

        if (currentWaveIndex >= waves.Length)
        {
            spawnedEnemies.RemoveAll(e => e == null);
            if (spawnedEnemies.Count == 0 && !spawningWave)
            {
                EndAllWaves();
            }
            return;
        }

        if (isIntermission)
        {
            intermissionTimer -= Time.deltaTime;
            if (intermissionTimer < 0f) intermissionTimer = 0f;
            UpdateIntermissionText();
            if (intermissionTimer <= 0f && !waveInProgress && !spawningWave)
            {
                isIntermission = false;
                waveTimeLeft = waves[currentWaveIndex].waveDuration;
                spawnedEnemies.Clear();
                waveInProgress = true;
                if (spawnLimitChecker != null)
                {
                    spawnLimitChecker.SetMaxCount(waves[currentWaveIndex].spawnLimit);
                }
                UpdateWaveCountText();
                UpdateWaveTimerText();
                StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            }
        }
        else
        {
            waveTimeLeft -= Time.deltaTime;
            if (waveTimeLeft < 0f) waveTimeLeft = 0f;
            UpdateWaveTimerText();
            spawnedEnemies.RemoveAll(e => e == null);
            if (!spawningWave)
            {
                if (waveInProgress && (spawnedEnemies.Count == 0 || waveTimeLeft <= 0f))
                {
                    EndWave();
                }
            }
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        spawningWave = true;
        var spawnCoroutines = new List<Coroutine>();
        foreach (var group in wave.enemyGroups)
        {
            spawnCoroutines.Add(StartCoroutine(SpawnEnemyGroup(group)));
        }
        foreach (var coroutine in spawnCoroutines)
        {
            yield return coroutine;
        }
        spawningWave = false;
    }

    IEnumerator SpawnEnemyGroup(EnemyGroup group)
    {
        yield return new WaitForSeconds(group.delay);
        int spawnedCount = 0;
        while (spawnedCount < group.count)
        {
            if (spawnLimitChecker != null && spawnLimitChecker.CanSpawn())
            {
                if (spawnPoints == null || spawnPoints.Length == 0)
                {
                    Debug.LogError("WaveManagerにグローバルなスポーンポイントが設定されていません！", this.gameObject);
                    yield break;
                }
                Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject enemyObj = Instantiate(group.enemyPrefab, point.position, Quaternion.identity);
                spawnedEnemies.Add(enemyObj);
                Enemy enemyScript = enemyObj.GetComponent<Enemy>();
                if (enemyScript != null) { enemyScript.Initialize(group.hpMultiplier, group.damageMultiplier, group.speedMultiplier); }
                else { BoarEnemy boar = enemyObj.GetComponent<BoarEnemy>(); if (boar != null) { boar.Initialize(group.hpMultiplier, group.damageMultiplier, group.speedMultiplier); } }
                spawnedCount++;
                if (group.spawnInterval > 0)
                {
                    yield return new WaitForSeconds(group.spawnInterval);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    void EndWave()
    {
        if (isIntermission) return;

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();

        waveInProgress = false;
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            return;
        }

        isIntermission = true;
        intermissionTimer = timeBetweenWaves;
        waveTimeLeft = 0f;

        UpdateIntermissionText();
        UpdateWaveCountText();
        if (waveDurationText != null) waveDurationText.text = "";
    }

    void EndAllWaves()
    {
        if (allWavesCompleted) return;
        allWavesCompleted = true;
        if (waveTimerText != null) waveTimerText.text = "All waves completed!";
        if (waveDurationText != null) waveDurationText.text = "";
        if (waveCountText != null) waveCountText.text = "All waves completed!";
        StartCoroutine(LoadNextSceneAfterDelay(3f));
    }

    IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void UpdateIntermissionText()
    {
        if (waveTimerText != null)
        {
            waveTimerText.gameObject.SetActive(true);
            waveTimerText.text = $"Next Wave: {FormatTime(intermissionTimer)}";
        }
        if (waveDurationText != null)
        {
            waveDurationText.gameObject.SetActive(false);
        }
    }

    void UpdateWaveTimerText()
    {
        if (waveDurationText != null)
        {
            waveDurationText.gameObject.SetActive(true);
            waveDurationText.text = $"Wave Time Left: {FormatTime(waveTimeLeft)}";
        }
        if (waveTimerText != null)
        {
            waveTimerText.gameObject.SetActive(false);
        }
    }

    void UpdateWaveCountText()
    {
        if (waveCountText != null)
        {
            if (currentWaveIndex < waves.Length)
            {
                waveCountText.text = $"Wave {currentWaveIndex + 1} / {waves.Length}";
            }
        }
    }

    string FormatTime(float timeSeconds)
    {
        int minutes = Mathf.FloorToInt(timeSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeSeconds % 60f);
        return $"{minutes:D2}:{seconds:D2}";
    }

    public void SetWaveDuration(int waveIndex, float duration)
    {
        if (waveIndex >= 0 && waveIndex < waves.Length)
        {
            waves[waveIndex].waveDuration = duration;
            if (!isIntermission && currentWaveIndex == waveIndex)
            {
                waveTimeLeft = Mathf.Max(waveTimeLeft, duration);
                UpdateWaveTimerText();
            }
        }
    }

    public void SetWaveSpawnInterval(int waveIndex, float interval)
    {
        if (waveIndex >= 0 && waveIndex < waves.Length)
        {
            foreach (var group in waves[waveIndex].enemyGroups)
            {
                group.spawnInterval = interval;
            }
        }
    }
}