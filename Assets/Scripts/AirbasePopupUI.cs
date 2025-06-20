using System.Collections.Generic;
using UnityEngine;

public class AirbasePopupUI : MonoBehaviour
{
    public Transform deploySpawnTarget;  // 👉 Airbase의 Transform
    public float deployOffset = 5f;      // 👉 앞쪽으로 얼마나 띄울지

    public List<AirbaseItemData> itemDataList;
    public GameObject itemPrefab;
    public Transform itemContainer;
    public Transform deploySpawnPoint;

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

        // Airbase 오브젝트 찾기
        GameObject airbaseGO = GameObject.Find("Airbase");
        if (airbaseGO == null) return;

        Transform airbaseTransform = airbaseGO.transform;
        // 출격 위치 계산: Airbase 앞쪽
        Vector3 spawnPos = deploySpawnTarget.position + deploySpawnTarget.forward * deployOffset;
        Quaternion spawnRot = deploySpawnTarget.rotation;

        GameObject unit = Instantiate(data.unitPrefab, spawnPos, spawnRot);

        // JetMover가 있다면 귀환 위치를 설정
        var mover = unit.GetComponent<JetMover>();
        if (mover != null)
        {
            mover.originItemUI = selectedItem; // 👈 출격한 UI를 연결
            mover.homePosition = deploySpawnTarget.position;
            var status = unit.GetComponent<ArmyStatus>();
            if (status != null)
                mover.selfStatus = status;

            // ✅ 현재 선택된 아이템을 지역 변수로 복사
            var itemToTrack = selectedItem;

            // 출격 완료 후 복귀 시 UI 다시 활성화
            mover.onReturnComplete = () =>
            {
                itemToTrack?.SetAvailable(true);
            };
        }

        Debug.Log($"[JetMover] HomePosition set to {mover.homePosition}");


        if (selectedItem != null)
        {
            selectedItem.SetSelected(false);
            selectedItem.SetDeployed(true); // ✅ 추가: UI 회색 처리 + 비활성화
        }

        selectedItem = null;
    }


    private void OnCancelClicked()
    {
        if (selectedItem != null)
            selectedItem.SetSelected(false);

        selectedItem = null;
    }

}
