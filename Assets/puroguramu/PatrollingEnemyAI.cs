using UnityEngine;
using System.Collections.Generic; // �������C�����܂����I

public class PatrollingEnemyAI : MonoBehaviour
{
    [Header("�ړ��ݒ�")]
    public float patrolSpeed = 1.5f; // ���񎞂̑��x
    public float chaseSpeed = 3.0f;  // �ǐՎ��̑��x

    [Header("���G�E�U���ݒ�")]
    public float detectionRange = 5f; // ���G�͈�
    public float attackRange = 1.5f;  // �U���͈�
    public float attackCooldown = 1f; // �U���N�[���_�E��

    [Header("�^�[�Q�b�g�^�O")]
    public string targetTag = "Ally"; // �ǂ�������I�u�W�F�N�g�̃^�O (��: "Ally", "Player")
    public string baseTag = "Base";   // �ŏI�I�ȋ��_�I�u�W�F�N�g�̃^�O (�I�v�V����)

    [Header("����ݒ�")]
    public List<Transform> patrolPoints; // ���񂷂�E�F�C�|�C���g�̃��X�g
    public float waypointThreshold = 0.5f; // �E�F�C�|�C���g�ɓ��B�����Ƃ݂Ȃ�����

    [Header("�͈͊�_")]
    public Transform rangeOrigin; // ���G�E�U���͈͂̊�_�ƂȂ�Transform (�q�I�u�W�F�N�g��ݒ萄��)

    // AI�̏�ԊǗ��Ɋւ���ǉ�
    public enum AIState
    {
        Patrolling,  // ����
        Chasing,     // �^�[�Q�b�g��ǐՒ�
        Attacking,   // �U����
        Fleeing      // ������
    }
    private AIState currentState = AIState.Patrolling; // ������Ԃ͏���

    [Header("�����ݒ�")]
    public float fleeDistance = 8f; // ���̋����܂œ������珄��ɖ߂� (�^�[�Q�b�g���痣��鋗��)
    public float reEngageCooldown = 3f; // ������A�ēx�^�[�Q�b�g��ǂ�������܂ł̃N�[���_�E��
    private float lastAttackFleeTime = -999f; // �Ō�ɍU�����ē������J�n��������

    private Transform currentTarget;
    private float lastAttackTime = -999f;
    private Enemy enemy; // HP, �U����, ���x�Ȃǂ��Ǘ�����R���|�[�l���g (������Enemy.cs��z��)

    private Vector3 currentPatrolTarget; // ���݌������Ă��鏄��n�_
    private int currentPatrolIndex = 0;  // ���݂̃E�F�C�|�C���g�̃C���f�b�N�X

    void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Enemy �R���|�[�l���g���A�^�b�`����Ă��܂���B����AI�͐���ɓ��삵�܂���B", this);
            enabled = false; // Enemy�R���|�[�l���g���Ȃ��ꍇ�A���̃X�N���v�g�𖳌��ɂ���
            return;
        }

        // rangeOrigin���ݒ肳��Ă��Ȃ��ꍇ�A���g��Transform���f�t�H���g�Ƃ��Ďg��
        if (rangeOrigin == null)
        {
            rangeOrigin = this.transform;
            Debug.LogWarning("Range Origin ���ݒ肳��Ă��܂���B�G��Transform.position����_�Ƃ��Ďg�p����܂��B", this);
        }

        // ����|�C���g���ݒ肳��Ă���΁A�ŏ��̃|�C���g��ݒ�
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            SetNextPatrolTarget();
        }
    }

    void Update()
    {
        if (enemy == null) return;

        // ��ԂɊ�Â��čs��������
        switch (currentState)
        {
            case AIState.Patrolling:
                // ���񒆂ɍ��G
                FindTargetInDetectionRange();
                if (currentTarget != null)
                {
                    // �^�[�Q�b�g����������ǐՏ�Ԃ�
                    currentState = AIState.Chasing;
                    Debug.Log("�X�e�[�g�ύX: Patrolling -> Chasing");
                }
                Patrol();
                break;

            case AIState.Chasing:
                // �ǐՒ��Ƀ^�[�Q�b�g���͈͊O�ɏo�����A�U���͈͂ɓ��������`�F�b�N
                FindTargetInDetectionRange(); // �^�[�Q�b�g�̋����`�F�b�N�����˂�

                if (currentTarget == null)
                {
                    // �^�[�Q�b�g�����������珄���Ԃ�
                    currentState = AIState.Patrolling;
                    Debug.Log("�X�e�[�g�ύX: Chasing -> Patrolling (�^�[�Q�b�g������)");
                }
                else if (Vector3.Distance(rangeOrigin.position, currentTarget.position) <= attackRange)
                {
                    // �U���͈͂ɓ�������U����Ԃ�
                    currentState = AIState.Attacking;
                    Debug.Log("�X�e�[�g�ύX: Chasing -> Attacking");
                }
                else
                {
                    ChaseTarget(); // �ǐՐ�p���\�b�h���Ăяo��
                }
                break;

            case AIState.Attacking:
                // �U����
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    PerformAttack(); // �U�����s
                    lastAttackTime = Time.time;

                    // �U����A������Ԃֈڍs
                    currentState = AIState.Fleeing;
                    lastAttackFleeTime = Time.time; // �����J�n���Ԃ��L�^
                    Debug.Log("�X�e�[�g�ύX: Attacking -> Fleeing (�U����)");
                }
                else if (currentTarget == null || Vector3.Distance(rangeOrigin.position, currentTarget.position) > attackRange)
                {
                    // �^�[�Q�b�g�����������A�܂��͍U���͈͊O�ɏo���ꍇ
                    currentState = AIState.Patrolling; // ��������Chasing�ɖ߂����I��
                    Debug.Log("�X�e�[�g�ύX: Attacking -> Patrolling (�U�����f)");
                }
                // �U���N�[���_�E�����͉��������ҋ@
                break;

            case AIState.Fleeing:
                // ������
                FleeFromTarget();

                // �����N�[���_�E���������Ă���΍��G���ĊJ
                if (Time.time - lastAttackFleeTime >= reEngageCooldown)
                {
                    // �^�[�Q�b�g�����G�͈͓��ɖ߂��Ă��Ă����Chasing�A�����łȂ����Patrolling
                    FindTargetInDetectionRange(); // ������currentTarget���X�V�����\��������
                    if (currentTarget != null)
                    {
                        currentState = AIState.Chasing;
                        Debug.Log("�X�e�[�g�ύX: Fleeing -> Chasing (�Č��)");
                    }
                    else
                    {
                        currentState = AIState.Patrolling;
                        Debug.Log("�X�e�[�g�ύX: Fleeing -> Patrolling (���������A�^�[�Q�b�g�Ȃ�)");
                    }
                }
                break;
        }
    }

    /// <summary>
    /// ���G�͈͓��Ƀ^�[�Q�b�g�����݂��邩���`�F�b�N���AcurrentTarget��ݒ肵�܂��B
    /// </summary>
    void FindTargetInDetectionRange()
    {
        // ���݃^�[�Q�b�g������ꍇ�A���ꂪ���G�͈͓��ɂ��邩���m�F
        if (currentTarget != null)
        {
            // �^�[�Q�b�g��Destroy���ꂽ�ꍇ��null�ɂȂ�̂Ń`�F�b�N
            if (currentTarget == null)
            {
                Debug.Log("�ǐՒ��̃^�[�Q�b�g���������܂����B");
                currentTarget = null;
                return;
            }

            // �^�[�Q�b�g�����G�͈͊O�ɏo���ꍇ�̂݁AcurrentTarget��null�ɂ���
            // Fleeing�X�e�[�g����̍Č�픻��ł́A���G�͈͂ɓ��蒼���K�v�����邽�߁A
            // �����ł�Chasing�X�e�[�g����̑J�ڂł̂ݔ͈͊O�`�F�b�N���s��
            if (currentState == AIState.Chasing && Vector3.Distance(rangeOrigin.position, currentTarget.position) > detectionRange)
            {
                Debug.Log($"�^�[�Q�b�g({currentTarget.name})�����G�͈͊O�ɏo�����߁A�ǐՂ��I�����܂��B");
                currentTarget = null;
                return;
            }
            // �^�[�Q�b�g�����݂��A���͈͓��ł���΁A�V�����^�[�Q�b�g��T���K�v�͂Ȃ�
            return;
        }

        // �V�����^�[�Q�b�g��T�� (�w�肳�ꂽ�^�O�̃I�u�W�F�N�g��D��)
        Collider[] hitColliders = Physics.OverlapSphere(rangeOrigin.position, detectionRange);
        Transform newTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            // �w�肳�ꂽ�^�O�����I�u�W�F�N�g��T�� (��: "Ally")
            if (hitCollider.CompareTag(targetTag))
            {
                float dist = Vector3.Distance(rangeOrigin.position, hitCollider.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    newTarget = hitCollider.transform;
                }
            }
        }

        if (newTarget != null)
        {
            currentTarget = newTarget;
            // �����ł̓X�e�[�g�ύX�����AUpdate()�̌Ăяo�����ŕύX����
            Debug.Log($"�V�����^�[�Q�b�g({currentTarget.name})�����G���܂����I");
        }
        else
        {
            // �w�肳�ꂽ�^�O�̃I�u�W�F�N�g�����Ȃ���΁A���_��T�� (�I�v�V����)
            GameObject baseObj = GameObject.FindGameObjectWithTag(baseTag);
            if (baseObj != null)
            {
                // ���_�����G�͈͓��ɂ���ꍇ�̂݃^�[�Q�b�g�ɂ���
                if (Vector3.Distance(rangeOrigin.position, baseObj.transform.position) <= detectionRange)
                {
                    currentTarget = baseObj.transform;
                    // �����ł̓X�e�[�g�ύX�����AUpdate()�̌Ăяo�����ŕύX����
                    Debug.Log($"���_({currentTarget.name})�����G���܂����I");
                }
            }
        }
    }

    /// <summary>
    /// �^�[�Q�b�g��ǐՂ��܂��B�i�U���͍s���܂���j
    /// </summary>
    void ChaseTarget()
    {
        if (currentTarget == null) return;

        Vector3 dir = (currentTarget.position - transform.position).normalized;
        transform.position += dir * chaseSpeed * Time.deltaTime;
        AdjustSpriteDirection(dir.x);
    }

    /// <summary>
    /// �ݒ肳�ꂽ�E�F�C�|�C���g�Ԃ����񂵂܂��B
    /// </summary>
    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            // ����|�C���g���ݒ肳��Ă��Ȃ��ꍇ�͉������Ȃ�
            return;
        }

        // �E�F�C�|�C���g�ɋ߂Â����玟�̃E�F�C�|�C���g��ݒ�
        if (Vector3.Distance(transform.position, currentPatrolTarget) < waypointThreshold)
        {
            SetNextPatrolTarget();
        }

        // �ړ�
        Vector3 dir = (currentPatrolTarget - transform.position).normalized;
        transform.position += dir * patrolSpeed * Time.deltaTime;

        // �X�v���C�g�̌����𒲐�
        AdjustSpriteDirection(dir.x);
    }

    /// <summary>
    /// �^�[�Q�b�g���瓦�����܂��B
    /// </summary>
    void FleeFromTarget()
    {
        if (currentTarget == null)
        {
            // �^�[�Q�b�g�����Ȃ���΁A�����N�[���_�E����ɏ���֖߂�
            // Fleeing�X�e�[�g�̂܂܁AreEngageCooldown��������̂�҂�
            return;
        }

        // �^�[�Q�b�g���牓����������ֈړ�
        Vector3 fleeDirection = (transform.position - currentTarget.position).normalized;
        transform.position += fleeDirection * chaseSpeed * Time.deltaTime; // �ǐՂƓ������x�œ�����

        AdjustSpriteDirection(fleeDirection.x);
    }


    /// <summary>
    /// ���̏���ڕW�n�_��ݒ肵�܂��B
    /// </summary>
    void SetNextPatrolTarget()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        currentPatrolTarget = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count; // ���̃C���f�b�N�X�ցi���[�v�j
    }

    /// <summary>
    /// �X�v���C�g�̌����𒲐����܂��B�i�E�����X�v���C�g��O��j
    /// </summary>
    /// <param name="directionX">�ړ�������X����</param>
    void AdjustSpriteDirection(float directionX)
    {
        if (directionX != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(directionX);
            transform.localScale = scale;
        }
    }

    /// <summary>
    /// �^�[�Q�b�g�ɑ΂��čU�������s���܂��B
    /// </summary>
    void PerformAttack()
    {
        if (currentTarget == null) return; // �^�[�Q�b�g���������Ă����牽�����Ȃ�

        // �^�[�Q�b�g��Ally�R���|�[�l���g�������Ă��邩�m�F
        Ally ally = currentTarget.GetComponent<Ally>();
        if (ally != null)
        {
            ally.TakeDamage(enemy.damage);
            Debug.Log($"{currentTarget.name} �� {enemy.damage} �_���[�W��^���܂����B");
            return;
        }

        // �^�[�Q�b�g��BaseHP�R���|�[�l���g�������Ă��邩�m�F
        BaseHP baseHP = currentTarget.GetComponent<BaseHP>();
        if (baseHP != null)
        {
            baseHP.TakeDamage(enemy.damage);
            Debug.Log($"{currentTarget.name} �� {enemy.damage} �_���[�W��^���܂����B");
            return;
        }

        Debug.LogWarning($"�^�[�Q�b�g {currentTarget.name} �ɍU���ł���R���|�[�l���g (Ally�܂���BaseHP) ������܂���ł����B");
    }

    /// <summary>
    /// Unity�G�f�B�^�[��Scene�r���[�ŁA���o�I�ȃf�o�b�O����\�����܂��B
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // rangeOrigin ���ݒ肳��Ă��Ȃ��ꍇ�A���g�̈ʒu����Ƃ���
        Vector3 originPos = (rangeOrigin != null) ? rangeOrigin.position : transform.position;

        // ���G�͈͂̕`�� (���F)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(originPos, detectionRange);

        // �U���͈͂̕`�� (��)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(originPos, attackRange);

        // ���񃋁[�g�̕`�� (��)
        if (patrolPoints != null && patrolPoints.Count > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.2f); // �E�F�C�|�C���g���̂�`��
                    if (i < patrolPoints.Count - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position); // �E�F�C�|�C���g�Ԃ̐���`��
                    }
                }
            }
            // �Ō�̃|�C���g�ƍŏ��̃|�C���g�����ԁi���[�v����̏ꍇ�j
            if (patrolPoints.Count > 1 && patrolPoints[patrolPoints.Count - 1] != null && patrolPoints[0] != null)
            {
                Gizmos.DrawLine(patrolPoints[patrolPoints.Count - 1].position, patrolPoints[0].position);
            }
        }

        // ���݂̏���ڕW�n�_�̕`�� (�V�A��)
        if (patrolPoints != null && patrolPoints.Count > 0 && currentPatrolTarget != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentPatrolTarget, waypointThreshold);
        }

        // �^�[�Q�b�g�ւ̐��̕`�� (�}�[���^)
        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}