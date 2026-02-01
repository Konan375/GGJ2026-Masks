using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthHUD : MonoBehaviour
{
    [Header("References")]
    public PlayerState playerState;

    [Header("Hearts Container (optional)")]
    [Tooltip("If set, hearts will be gathered from children of this transform. If null, uses this object.")]
    public Transform heartsContainer;

    [Header("Optional Text")]
    public TMP_Text healthText;

    [Header("Settings")]
    [Tooltip("If true, the script will auto-gather heart Images every time it runs (safe but slightly slower).")]
    public bool autoGatherHearts = true;

    private readonly List<Image> hearts = new List<Image>();

    private void Awake()
    {
        GatherHearts();
    }

    private void OnEnable()
    {
        GatherHearts();
    }

    private void Update()
    {
        if (playerState == null) return;

        if (autoGatherHearts && hearts.Count == 0)
            GatherHearts();

        int max = Mathf.Max(1, playerState.maxHealth);
        int cur = Mathf.Clamp(playerState.currentHealth, 0, max);

        // Update hearts based on what's available
        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i] == null) continue;

            bool inRange = i < max;
            hearts[i].gameObject.SetActive(inRange);

            if (inRange)
                hearts[i].enabled = (i < cur);
        }

        if (healthText != null)
            healthText.text = $"{cur}/{max}";
    }

    public void GatherHearts()
    {
        hearts.Clear();

        Transform root = heartsContainer != null ? heartsContainer : transform;

        // Grab Image components from direct children only (keeps ordering predictable)
        for (int i = 0; i < root.childCount; i++)
        {
            var img = root.GetChild(i).GetComponent<Image>();
            if (img != null)
                hearts.Add(img);
        }
    }
}
