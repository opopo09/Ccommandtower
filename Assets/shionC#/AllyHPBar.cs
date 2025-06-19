using UnityEngine;
using UnityEngine.UI;

public class AllyHPBar : MonoBehaviour
{
    public Image hpBar;           // �ʏ�HP�o�[ (Fill)
    public Image afterImageBar;   // �c��HP�o�[ (AfterImage)

    public float afterImageSpeed = 1.0f;  // �c���Q�[�W�����鑬��

    void Start()
    {
        hpBar.fillAmount = 1f;
        afterImageBar.fillAmount = 1f;
    }

    void Update()
    {
        // �c���o�[�����݂�HP�o�[���傫���ꍇ�A������茸�炷
        if (afterImageBar.fillAmount > hpBar.fillAmount)
        {
            afterImageBar.fillAmount -= afterImageSpeed * Time.deltaTime;

            if (afterImageBar.fillAmount < hpBar.fillAmount)
                afterImageBar.fillAmount = hpBar.fillAmount;
        }
        else
        {
            // �񕜎��Ȃǂ͒ʏ�o�[�Ɠ����ɂ���
            afterImageBar.fillAmount = hpBar.fillAmount;
        }
    }

    // HP�̊������X�V����֐��i�O������Ăԁj
    public void SetHP(float currentHP, float maxHP)
    {
        float hpRatio = Mathf.Clamp01(currentHP / maxHP);
        hpBar.fillAmount = hpRatio;
        // �c���o�[��Update�Œǂ����̂ł����͕ς��Ȃ�
    }
}
