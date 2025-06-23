using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterInfo
    {
        public string characterName;
        public GameObject characterPrefab;
        public Sprite icon;
    }

    public CharacterInfo[] characters;

    [Header("視点固定UI")]
    public Image fixedViewCharacterIcon;  // カメラ視点の画面上に置くImage
    public Text fixedViewCharacterName;   // 同じく名前表示

    private GameObject selectedPrefab;

    void Start()
    {
        ClearFixedViewUI();
    }

    // UIボタンから呼ぶ
    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= characters.Length) return;

        selectedPrefab = characters[index].characterPrefab;
        fixedViewCharacterIcon.sprite = characters[index].icon;
        fixedViewCharacterIcon.enabled = true;
        fixedViewCharacterName.text = characters[index].characterName;
    }

    void Update()
    {
        if (selectedPrefab != null && (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire2")))
        {
            Vector3 spawnPos = GetSpawnPosition();
            Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        }
    }

    Vector3 GetSpawnPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;
        return worldPos;
    }

    void ClearFixedViewUI()
    {
        selectedPrefab = null;
        fixedViewCharacterIcon.enabled = false;
        fixedViewCharacterName.text = "";
    }
}
