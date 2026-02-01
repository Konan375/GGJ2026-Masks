using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("References")]
    public PlayerState playerState;

    [Header("Masks UI (4 slots)")]
    public Image[] maskSlots = new Image[4];          // icons
    public Image cooldownOverlay;                     // your CoolDownOverlay Image
    public RectTransform activeFrame;                 // optional (we can add later)

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

    public void Refresh()
    {
        if (playerState == null) return;

        int active = (int)playerState.activeMask;

        // --- Score (if assigned) ---
        if (scoreText != null)
            scoreText.text = playerState.coins.ToString();

        // --- Health text (temporary until hearts) ---
        if (healthText != null)
            healthText.text = $"{playerState.currentHealth}/{playerState.maxHealth}";

        // --- Masks owned / active visual ---
        for (int i = 0; i < maskSlots.Length && i < 4; i++)
        {
            if (maskSlots[i] == null) continue;

            // Dim if not owned
            var c = maskSlots[i].color;
            c.a = playerState.masksOwned[i] ? 1f : 0.25f;
            maskSlots[i].color = c;
        }

        // --- Cooldown overlay: move it to active slot and set fill ---
        if (cooldownOverlay != null && active < maskSlots.Length && maskSlots[active] != null)
        {
            // Parent overlay under active slot so it sits on top of that icon
            cooldownOverlay.transform.SetParent(maskSlots[active].transform, false);

            float remain = playerState.cooldownRemaining[active];
            float dur = Mathf.Max(0.01f, playerState.cooldownDuration[active]);

            // Fill amount: 1 = full overlay (cooldown just started), 0 = ready
            cooldownOverlay.fillAmount = remain / dur;

            // Hide overlay when ready
            cooldownOverlay.gameObject.SetActive(remain > 0f);
        }
    }
}
