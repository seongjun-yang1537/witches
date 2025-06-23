using System.Collections.Generic;
using UnityEngine;

public class AirbasePopupUI : MonoBehaviour
{
    public Transform deploySpawnTarget;
    public float deployOffset = 5f;

    public List<AirbaseItemData> itemDataList;
    public GameObject itemPrefab;
    public Transform itemContainer;
    public Canvas uiCanvas;

    private AirbaseItemUI selectedItem;

    // ✅ itemUI → jet 매핑
    private Dictionary<AirbaseItemUI, GameObject> jetInstances = new();

    void Start()
    {
        foreach (var data in itemDataList)
        {
            GameObject itemGO = Instantiate(itemPrefab, itemContainer);
            var itemUI = itemGO.GetComponent<AirbaseItemUI>();

            itemUI.Setup(
                data,
                () => OnItemSelected(itemUI),
                () => OnDeployClicked(itemUI),
                () => OnCancelClicked()
            );

            // ✅ Jet 미리 생성
            Vector3 offscreen = new Vector3(9999, 9999, 9999);
            GameObject jet = Instantiate(data.unitPrefab, offscreen, Quaternion.identity);
            jet.SetActive(false);
            jetInstances[itemUI] = jet;

            var status = jet.GetComponent<JetStatus>();
            if (status != null)
            {
                status.uiCanvas = uiCanvas;
                status.title = data.unitName;
                status.SetOriginUI(itemUI);
            }

            var mover = jet.GetComponent<JetMover>();
            if (mover != null)
            {
                mover.originItemUI = itemUI;
                mover.homePosition = deploySpawnTarget.position;
            }
        }
    }

    private void OnItemSelected(AirbaseItemUI itemUI)
    {
        if (selectedItem != null)
            selectedItem.SetSelected(false);

        selectedItem = itemUI;
        selectedItem.SetSelected(true);
    }

    private void OnDeployClicked(AirbaseItemUI itemUI)
    {
        if (!jetInstances.TryGetValue(itemUI, out var jet) || jet == null)
        {
            Debug.LogWarning("[AirbasePopupUI] JetInstance is missing or destroyed.");
            return;
        }

        Vector3 spawnPos = deploySpawnTarget.position + deploySpawnTarget.forward * deployOffset;
        jet.transform.position = spawnPos;
        jet.transform.rotation = deploySpawnTarget.rotation;
        jet.SetActive(true);

        var mover = jet.GetComponent<JetMover>();
        if (mover != null)
        {
            mover.enabled = true;
            mover.ResetForNewDeployment();
        }

        var status = jet.GetComponent<JetStatus>();
        if (status != null)
        {
            status.ResetHP();
            status.SetOriginUI(itemUI);
        }

        itemUI.SetSelected(false);
        itemUI.SetDeployed(true);
        selectedItem = null;

        gameObject.SetActive(false);
        FindObjectOfType<TargetSelectionManager>()?.BeginTargeting(mover);
    }

    private void OnCancelClicked()
    {
        if (selectedItem != null)
            selectedItem.SetSelected(false);

        selectedItem = null;
    }
}
