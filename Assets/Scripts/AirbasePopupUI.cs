using System.Collections.Generic;
using UnityEngine;

public class AirbasePopupUI : MonoBehaviour
{
    public Transform deploySpawnTarget;  // 👉 Airbase의 Transform
    public float deployOffset = 5f;

    public List<AirbaseItemData> itemDataList;
    public GameObject itemPrefab;
    public Transform itemContainer;
    public Transform deploySpawnPoint;

    public Canvas uiCanvas; // ✅ JetStatus가 사용할 UI 캔버스

    private AirbaseItemUI selectedItem;

    void Start()
    {
        foreach (var data in itemDataList)
        {
            GameObject itemGO = Instantiate(itemPrefab, itemContainer);
            var itemUI = itemGO.GetComponent<AirbaseItemUI>();

            itemUI.Setup(
                data,
                () => OnItemSelected(itemUI),
                () => OnDeployClicked(data),
                () => OnCancelClicked()
            );
        }
    }

    private void OnItemSelected(AirbaseItemUI itemUI)
    {
        if (selectedItem != null)
            selectedItem.SetSelected(false);

        selectedItem = itemUI;
        selectedItem.SetSelected(true);
    }

    private void OnDeployClicked(AirbaseItemData data)
    {
        if (data.unitPrefab == null || deploySpawnTarget == null) return;

        // 출격 위치 계산
        Vector3 spawnPos = deploySpawnTarget.position + deploySpawnTarget.forward * deployOffset;
        Quaternion spawnRot = deploySpawnTarget.rotation;

        GameObject unit = Instantiate(data.unitPrefab, spawnPos, spawnRot);

        // ✅ JetMover 설정
        var mover = unit.GetComponent<JetMover>();
        if (mover != null)
        {
            mover.originItemUI = selectedItem;
            mover.homePosition = deploySpawnTarget.position;

            // 출격 완료 후 복귀 시 UI 다시 활성화
            var itemToTrack = selectedItem;
            mover.onReturnComplete = () =>
            {
                itemToTrack?.SetAvailable(true);
            };
        }

        // ✅ JetStatus 설정
        var status = unit.GetComponent<JetStatus>();
        if (status != null && uiCanvas != null)
        {
            status.uiCanvas = uiCanvas;
        }

        // ✅ 선택된 아이템 처리
        if (selectedItem != null)
        {
            selectedItem.SetSelected(false);
            selectedItem.SetDeployed(true);
        }

        selectedItem = null;

        // ✅ 팝업 닫기 + 타겟 지정 페이즈로 진입
        gameObject.SetActive(false);

        var targetManager = FindObjectOfType<TargetSelectionManager>();
        if (targetManager != null)
        {
            targetManager.BeginTargeting(mover);
        }
        else
        {
            Debug.LogWarning("[AirbasePopupUI] TargetSelectionManager not found.");
        }
    }

    private void OnCancelClicked()
    {
        if (selectedItem != null)
            selectedItem.SetSelected(false);

        selectedItem = null;
    }
}
