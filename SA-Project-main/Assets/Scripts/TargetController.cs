using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    private List<GameObject> targetList = new List<GameObject>();
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject target;
    [SerializeField] private float refreshInterval = 0.2f;
    public GameObject lastAttackTarget;
    private Camera mainCamera;
    private int currentTargetIndex = 0;
    private float refreshTimer = 0f;
    public GameObject CurrentTarget //현재 타깃
    {
        get
        {
            if (targetList.Count == 0)
                return null;

            return targetList[currentTargetIndex];
        }
    }
    private void Awake()
    {
        mainCamera = Camera.main;
        target.SetActive(false);
    }

    private void Update()
    {
        refreshTimer += Time.unscaledDeltaTime;

        if (refreshTimer >= refreshInterval)
        {
            ReloadTargetList();
            refreshTimer = 0f;
        }

        if (targetList.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentTargetIndex++;

                if (currentTargetIndex >= targetList.Count)
                    currentTargetIndex = 0;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentTargetIndex--;

                if (currentTargetIndex < 0)
                    currentTargetIndex = targetList.Count - 1;
            }
        }

        UpdateTargetPosition();
    }

    public void ReloadTargetList() // 타겟 리스트 생성
    {
        if (playerTransform == null || mainCamera == null)
            return;

        // 기존 선택 대상 기억
        GameObject previousTarget = null;

        if (targetList.Count > 0 && currentTargetIndex < targetList.Count)
            previousTarget = targetList[currentTargetIndex];

        targetList.Clear();

        GameObject[] objects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in objects)
        {
            if (obj.layer != LayerMask.NameToLayer("Target"))
                continue;

            if (obj == gameObject)
                continue;

            // 카메라 화면 밖이면 제외
            if (!IsInCameraView(obj))
                continue;

            targetList.Add(obj);
        }

        targetList.Sort((a, b) =>
        {
            float distanceA = ((Vector2)a.transform.position - (Vector2)playerTransform.position).sqrMagnitude;
            float distanceB = ((Vector2)b.transform.position - (Vector2)playerTransform.position).sqrMagnitude;

            return distanceA.CompareTo(distanceB);
        });

        // 마지막 공격 대상 우선
        if (lastAttackTarget != null && targetList.Contains(lastAttackTarget))
        {
            targetList.Remove(lastAttackTarget);
            targetList.Insert(0, lastAttackTarget);
        }

        // 이전에 선택한 대상 유지
        currentTargetIndex = previousTarget != null && targetList.Contains(previousTarget)
            ? targetList.IndexOf(previousTarget)
            : 0;

        UpdateTargetPosition();
    }
    private bool IsInCameraView(GameObject obj)
    {
        Vector3 viewPos = mainCamera.WorldToViewportPoint(obj.transform.position);

        return viewPos.z > 0 &&
               viewPos.x >= 0 && viewPos.x <= 1 &&
               viewPos.y >= 0 && viewPos.y <= 1;
    }

    private void UpdateTargetPosition() // 현재 선택된 적 위치로 타겟 이동
    {
        if (target == null)
            return;

        if (targetList.Count == 0)
            return;

        target.transform.position = targetList[currentTargetIndex].transform.position;
    }
}