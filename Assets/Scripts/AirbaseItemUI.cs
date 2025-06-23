using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AirbaseItemUI : MonoBehaviour
{
    public Image jetImage;
    public Image pilotImage;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI healingText;
    public GameObject buttonGroup;
    
    public Button selectButton;    
    public Image background; // 배경 Image 연결
    public Color normalColor = Color.white;
    public Color disabledColor = new Color(0.6f, 0.6f, 0.6f); // 회색조

    private Button deployButton;
    private Button cancelButton;
    private Action onDeploy;
    private Action onCancel;

    private bool isSelected = false;
    private bool isAvailable = true;

    void Awake()
    {
        if (selectButton == null)
            selectButton = GetComponent<Button>();
    }


    public void Setup(AirbaseItemData data, Action onClick, Action onDeployClicked, Action onCancelClicked)
    {
        if (jetImage != null) jetImage.sprite = data.jetSprite;
        if (pilotImage != null) pilotImage.sprite = data.pilotSprite;
        if (infoText != null) infoText.text = data.description;

        onDeploy = onDeployClicked;
        onCancel = onCancelClicked;

        deployButton = buttonGroup.transform.Find("DeployButton").GetComponent<Button>();
        cancelButton = buttonGroup.transform.Find("CancelButton").GetComponent<Button>();

        deployButton.onClick.AddListener(() => onDeploy?.Invoke());
        cancelButton.onClick.AddListener(() => onCancel?.Invoke());

        // 아이템 전체를 클릭했을 때 선택
        GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        buttonGroup.SetActive(selected);
        GetComponent<Image>().color = selected ? new Color(1f, 1f, 0.85f, 1f) : Color.white;
    }

    public void SetInteractable(bool value)
    {
        if (selectButton != null)
            selectButton.interactable = value;

        if (background != null)
            background.color = value ? normalColor : disabledColor;

        if (!value && buttonGroup != null)
            buttonGroup.SetActive(false);
    }

    public void SetDeployed(bool isDeployed)
    {
        // 예시: 버튼 숨김 + 이미지 회색 처리
        buttonGroup.SetActive(false);

        // 회색조 처리 예시
        Color grey = new Color(0.6f, 0.6f, 0.6f);
        jetImage.color = grey;
        pilotImage.color = grey;        

        // 필요 시 상호작용 비활성화
        GetComponent<Button>().interactable = !isDeployed;
    }

    public void SetAvailable(bool available)
    {
        isAvailable = available;

        var cg = GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Debug.LogError("[AirbaseItemUI] CanvasGroup이 없습니다.");
        }
        else
        {
            cg.alpha = available ? 1f : 0.5f;
            cg.interactable = available;
            cg.blocksRaycasts = available;
        }

        if (selectButton == null)
        {
            Debug.LogWarning("[AirbaseItemUI] selectButton이 null입니다. GetComponent<Button>() 시도.");
            selectButton = GetComponent<Button>();
        }

        if (selectButton != null)
        {
            selectButton.interactable = available;
        }
        else
        {
            Debug.LogError("[AirbaseItemUI] 여전히 selectButton이 null입니다. 할당 실패.");
        }

        if (background != null)
            background.color = available ? normalColor : disabledColor;

        if (jetImage != null)
            jetImage.color = available ? Color.white : disabledColor;

        if (pilotImage != null)
            pilotImage.color = available ? Color.white : disabledColor;

        if (buttonGroup != null)
            buttonGroup.SetActive(false);
    }
    public void SetHealingState(bool isHealing, float currentHP, float maxHP)
    {
        if (healingText == null)
        {
            Debug.LogWarning("[AirbaseItemUI] HealingText is not assigned.");
            return;
        }

        if (isHealing)
        {
            healingText.gameObject.SetActive(true);
            healingText.text = $"Healing... ({Mathf.FloorToInt(currentHP)} / {Mathf.FloorToInt(maxHP)})";

            if (infoText != null)
                infoText.gameObject.SetActive(false);
        }
        else
        {
            healingText.gameObject.SetActive(false);
            if (infoText != null)
                infoText.gameObject.SetActive(true);
        }
    }

}
