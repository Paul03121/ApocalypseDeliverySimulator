using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeliveryResultUIManager : MonoBehaviour
{
    public static DeliveryResultUIManager Instance;

    [Header("Time Texts")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI integrityText;

    [Header("Reward Texts")]
    public TextMeshProUGUI baseRewardText;
    public TextMeshProUGUI timePenaltyText;
    public TextMeshProUGUI integrityPenaltyText;
    public TextMeshProUGUI finalRewardText;

    [Header("Buttons")]
    public Button acceptButton;

    [Header("References")]
    private DeliveryMission mission;
    private PlayerWallet wallet;

    private void Awake()
    {
        Instance = this;

        // Cache required player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        wallet = player.GetComponent<PlayerWallet>();

        if (wallet == null)
            Debug.LogError("PlayerWallet not found on Player");
    }

    // Display the delivery result UI for a completed mission
    public void Show(DeliveryMission mission)
    {
        this.mission = mission;

        // Prepare UI content and button logic
        PopulateUI();
        SetupAcceptButton();

        // Switch game to delivery result state
        GameStateManager.Instance.SetState(GameState.DeliveryResult);

        // Hide all UI elements and reveal them gradually
        HideAll();
        StartCoroutine(RevealResults());
    }

    // Populates all UI texts based on the mission results
    private void PopulateUI()
    {
        timeText.text =
            $"Tiempo de entrega: {FormatTime(mission.DeliveryTime)}";

        integrityText.text =
            $"Integridad del paquete: {(mission.Integrity * 100f):F0}%";

        baseRewardText.text =
            $"Ganancia base: ${mission.BaseReward}";

        timePenaltyText.text =
            $"Penalización por demora: - ${mission.TimePenalty}";

        integrityPenaltyText.text =
            $"Penalización por integridad: - ${mission.IntegrityPenalty}";

        finalRewardText.text =
            $"Ganancia final: ${mission.FinalReward}";
    }

    // Configures the accept button behavior
    private void SetupAcceptButton()
    {
        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(OnAcceptPressed);
    }

    // Handles reward confirmation and mission finalization
    private void OnAcceptPressed()
    {
        // Apply reward to the player
        wallet.AddMoney(mission.FinalReward);

        // Return game to gameplay state
        GameStateManager.Instance.SetState(GameState.Gameplay);
    }

    // Formats a time value (in seconds) into MM:SS
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }

    // Hides all result UI elements before starting the reveal sequence
    private void HideAll()
    {
        timeText.gameObject.SetActive(false);
        integrityText.gameObject.SetActive(false);

        baseRewardText.gameObject.SetActive(false);
        timePenaltyText.gameObject.SetActive(false);
        integrityPenaltyText.gameObject.SetActive(false);
        finalRewardText.gameObject.SetActive(false);

        acceptButton.gameObject.SetActive(false);
    }

    // Reveals result UI elements progressively using real-time delays
    private IEnumerator RevealResults()
    {
        yield return new WaitForSecondsRealtime(0.35f);
        timeText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.35f);
        integrityText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(0.6f);
        baseRewardText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.35f);
        timePenaltyText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.35f);
        integrityPenaltyText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.35f);
        finalRewardText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(0.85f);
        acceptButton.gameObject.SetActive(true);
    }
}