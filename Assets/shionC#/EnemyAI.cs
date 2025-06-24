using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // public�ϐ��͍폜�܂��͎c���Ă�OK�i�����l�Q�Ɨp�Ȃ�c���j
    // public float speed = 2f;
    // public float damage = 10f;
    public float attackCooldown = 1f;
    public float attackRange = 1.5f;

    [Header("�^�[�Q�b�g�^�O")]
    public string allyTag = "Ally";
    public string baseTag = "Base";

    private Transform currentTarget;
    private float lastAttackTime = -999f;
    private Enemy enemy;  // HP�ȂǂƍU���͂⑬�x���Ǘ�����R���|�[�l���g

    void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Enemy �R���|�[�l���g������܂���");
        }
    }

    void Update()
    {
        if (enemy == null) return;

        GameObject[] allies = GameObject.FindGameObjectsWithTag(allyTag);
        currentTarget = GetClosestTarget(allies);

        // ���������Ȃ���΋��_��T��
        if (currentTarget == null)
        {
            GameObject baseObj = GameObject.FindGameObjectWithTag(baseTag);
            if (baseObj != null)
            {
                currentTarget = baseObj.transform;
            }
        }

        if (currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance > attackRange)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            // Enemy�R���|�[�l���g��speed���g��
            transform.position += dir * enemy.speed * Time.deltaTime;

            // �X�v���C�g�̌����𒲐��i�E�����O��j
            if (dir.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dir.x);
                transform.localScale = scale;
            }
        }
        else
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Ally ally = currentTarget.GetComponent<Ally>();
                BaseHP baseHP = currentTarget.GetComponent<BaseHP>();

                if (ally != null)
                {
                    // Enemy�R���|�[�l���g��damage���g��
                    ally.TakeDamage(enemy.damage);
                }
                else if (baseHP != null)
                {
                    baseHP.TakeDamage(enemy.damage);
                }

                lastAttackTime = Time.time;
            }
        }
    }

    Transform GetClosestTarget(GameObject[] targets)
    {
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject obj in targets)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = obj.transform;
            }
        }

        return closest;
    }
}
