using UnityEngine;
using TMPro;

public class JetStatus : MonoBehaviour
{
    public enum TeamType { Blue, Red }
    public enum JetType { Fighter, Attacker, ElectronicWarfare }

    public JetType jetType = JetType.Fighter;
    public TeamType teamType = TeamType.Blue;
    public string title = "Jet";
    public float maxHP = 100f;
    public float currentHP;

    public bool isHealing = false;

    public GameObject uiPrefab;
    public Canvas uiCanvas;
    private GameObject createdUI;

    private TextMeshProUGUI tmpText;
    private WorldTextFollower follower;

    private AirbaseItemUI originItemUI;

    public void SetOriginUI(AirbaseItemUI ui)
    {
        originItemUI = ui;
    }

    public void ResetHP()
    {
        currentHP = maxHP;
        isHealing = false;
    }

    void Start()
    {
        GameStateManager.Instance?.RegisterJet(this);

        currentHP = maxHP;
        SetUnitLabel();

        if (uiPrefab != null && uiCanvas != null)
        {
            createdUI = Instantiate(uiPrefab, uiCanvas.transform);
            tmpText = createdUI.GetComponent<TextMeshProUGUI>();
            follower = createdUI.GetComponent<WorldTextFollower>();

            if (tmpText != null && follower != null)
            {
                follower.target = transform;
                follower.jetStatus = this;
                follower.uiText = tmpText;
                follower.label = title;

                float depth = transform.localScale.z;
                float padding = 0.5f;
                float distance = depth * 0.5f + padding;
                Vector3 direction = (teamType == TeamType.Red) ? transform.forward : -transform.forward;
                follower.offset = direction * distance;

                tmpText.alignment = (teamType == TeamType.Red)
                    ? TextAlignmentOptions.TopGeoAligned
                    : TextAlignmentOptions.BottomGeoAligned;
            }
        }
    }

    void Update()
    {
        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        if (!isHealing) return;

        float regen = PrototypeGameManager.Instance?.jetRegenPerSecond ?? 1f;
        currentHP += regen * Time.deltaTime;
        currentHP = Mathf.Min(currentHP, maxHP);

        originItemUI?.SetHealingState(true, currentHP, maxHP);

        if (currentHP >= maxHP)
        {
            isHealing = false;
            originItemUI?.SetHealingState(false, currentHP, maxHP);
            originItemUI?.SetAvailable(true);
            gameObject.SetActive(false);
        }
    }

    public int GetHPInt() => Mathf.FloorToInt(currentHP);

    public void ApplyDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public CombatUnitType GetCombatUnitType()
    {
        return jetType switch
        {
            JetType.Fighter => CombatUnitType.Fighter,
            JetType.Attacker => CombatUnitType.Attacker,
            JetType.ElectronicWarfare => CombatUnitType.ElectronicWarfare,
            _ => CombatUnitType.Fighter
        };
    }

    void SetUnitLabel()
    {
        TextMeshPro label = GetComponentInChildren<TextMeshPro>();
        if (label == null) return;

        string code = jetType switch
        {
            JetType.Fighter => "Ftr",
            JetType.Attacker => "Atk",
            JetType.ElectronicWarfare => "EW",
            _ => "???"
        };

        label.text = code;
        label.color = teamType switch
        {
            TeamType.Blue => Color.blue,
            TeamType.Red => Color.red,
            _ => Color.white
        };
    }

    public void StartHealing()
    {
        isHealing = true;
        originItemUI?.SetAvailable(false);
        originItemUI?.SetHealingState(true, currentHP, maxHP);
    }

    void OnDestroy()
    {
        GameStateManager.Instance?.UnregisterJet(this);

        if (originItemUI != null)
        {
            Destroy(originItemUI.gameObject);
            originItemUI = null;
        }
    }
}
