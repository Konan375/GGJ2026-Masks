using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("References")]
    public PlayerState playerState;

    [Header("Masks UI (4 slots)")]
    public Image[] maskSlots = new Image[4];
    public Image cooldownOverlay;
    public RectTransform activeFrame;

    [Header("Score UI (optional)")]
    public TMP_Text scoreText;

    [Header("Health UI (optional later)")]
    public TMP_Text healthText;

    private void OnEnable()
    {
        if (playerState != null)
            playerState.OnStateChanged += Refresh;
    }

    private void OnDisable()
    {
        if (playerState != null)
            playerState.OnStateChanged -= Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    // Jam-safe: if event wiring ever fails, HUD still updates
    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (playerState == null) return;

        int active = (int)playerState.activeMask;

        // Score
        if (scoreText != null)
            scoreText.text = playerState.coins.ToString();

        // Health text (temporary)
        if (healthText != null)
            healthText.text = $"{playerState.currentHealth}/{playerState.maxHealth}";

        // Dim masks if not owned
        for (int i = 0; i < 4 && i < maskSlots.Length; i++)
        {
            if (maskSlots[i] == null) continue;

            Color c = maskSlots[i].color;
            c.a = playerState.masksOwned[i] ? 1f : 0.25f;
            maskSlots[i].color = c;
        }

        // Cooldown overlay on active slot
        if (cooldownOverlay != null &&
            active >= 0 && active < 4 &&
            active < maskSlots.Length &&
            maskSlots[active] != null)
        {
            cooldownOverlay.transform.SetParent(maskSlots[active].transform, false);

            float remain = playerState.cooldownRemaining[active];
            float dur = Mathf.Max(0.01f, playerState.cooldownDuration[active]);

            cooldownOverlay.fillAmount = remain / dur;
            cooldownOverlay.gameObject.SetActive(remain > 0f);
        }

        // Optional active frame highlight
        if (activeFrame != null &&
            active >= 0 && active < 4 &&
            active < maskSlots.Length &&
            maskSlots[active] != null)
        {
            activeFrame.SetParent(maskSlots[active].transform, false);
            activeFrame.anchoredPosition = Vector2.zero;
        }
    }
}
