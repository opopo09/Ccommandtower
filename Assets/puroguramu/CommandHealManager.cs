using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CommandHealManager : MonoBehaviour
{
    public float healAmount = 30f;

    [Header("回復用コマンド設定")]
    public List<int> healCommand = new List<int>(); // 例: 0:A, 1:B, 2:X, 3:Yなど

    private List<int> currentInput = new List<int>();
    private int currentStep = 0;

    void Update()
    {
        // ここでコマンド入力監視（Castle.csと同様に）

        for (int i = 0; i <= 7; i++)
        {
            if (Input.GetKeyDown("joystick button " + i))
            {
                HandleInput(i);
                return;
            }
        }

        // 十字キーも必要ならここで監視する
    }

    void HandleInput(int input)
    {
        if (healCommand.Count == 0) return;

        int expected = healCommand[currentStep];
        if (input == expected)
        {
            currentStep++;
            if (currentStep >= healCommand.Count)
            {
                HealAllies();
                currentStep = 0;
            }
        }
        else
        {
            currentStep = 0; // ミスでリセット
        }
    }

    void HealAllies()
    {
        Ally[] allies = Object.FindObjectsByType<Ally>(FindObjectsSortMode.None);
        foreach (Ally ally in allies)
        {
            ally.Heal(healAmount);
        }
        Debug.Log($"味方を{healAmount}回復しました");
    }
}
