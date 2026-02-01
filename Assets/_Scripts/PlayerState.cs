using System;
using UnityEngine;

public enum MaskType
{
    Mask1 = 0,
    Mask2 = 1,
    Mask3 = 2,
    Mask4 = 3
}

public class PlayerState : MonoBehaviour
{
    [Header("Health")]
    [Min(1)] public int maxHealth = 5;
    public int currentHealth = 5;

    [Header("Currency / Score")]
    public int coins = 0;

    [Header("Masks")]
    public bool[] masksOwned = new bool[4] { true, true, true, true };
    public MaskType activeMask = MaskType.Mask1;

    [Header("Cooldowns (seconds)")]
    public float[] cooldownDuration = new float[4] { 5f, 6f, 7f, 8f };
    [NonSerialized] public float[] cooldownRemaining = new float[4];

    public event Action OnStateChanged;

    private void Awake()
    {
        EnsureArrays();
        ClampHealth();
        RaiseChanged();
    }

    private void Update()
    {
        // Tick cooldowns down each frame
        bool changed = TickCooldowns();

        // Jam test controls (you can remove later)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleMask(-1);
            changed = true;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            CycleMask(1);
            changed = true;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryUseActiveMaskAbility();
            changed = true;
        }

        if (changed)
            RaiseChanged();
    }

    // ---------- Public API ----------
    public void AddCoins(int amount)
    {
        coins = Mathf.Max(0, coins + amount);
        RaiseChanged();
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Abs(amount), 0, maxHealth);
        RaiseChanged();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + Mathf.Abs(amount), 0, maxHealth);
        RaiseChanged();
    }

    public void SetActiveMask(MaskType mask)
    {
        int idx = (int)mask;
        if (idx < 0 || idx >= 4) return;
        if (!masksOwned[idx]) return;

        activeMask = mask;
        RaiseChanged();
    }

    public void CycleMask(int direction)
    {
        // direction should be -1 or +1
        if (direction == 0) return;

        int start = (int)activeMask;
        int idx = start;

        for (int attempts = 0; attempts < 4; attempts++)
        {
            idx = (idx + direction + 4) % 4;
            if (masksOwned[idx])
            {
                activeMask = (MaskType)idx;
                RaiseChanged();
                return;
            }
        }
    }

    public bool TryUseActiveMaskAbility()
    {
        int idx = (int)activeMask;
        if (!masksOwned[idx]) return false;
        if (cooldownRemaining[idx] > 0f) return false;

        cooldownRemaining[idx] = Mathf.Max(0.01f, cooldownDuration[idx]);
        RaiseChanged();
        return true;
    }

    // ---------- Internals ----------
    private bool TickCooldowns()
    {
        bool changed = false;

        for (int i = 0; i < 4; i++)
        {
            if (cooldownRemaining[i] > 0f)
            {
                cooldownRemaining[i] = Mathf.Max(0f, cooldownRemaining[i] - Time.deltaTime);
                changed = true;
            }
        }

        return changed;
    }

    private void EnsureArrays()
    {
        if (masksOwned == null || masksOwned.Length != 4)
            masksOwned = new bool[4] { true, true, true, true };

        if (cooldownDuration == null || cooldownDuration.Length != 4)
            cooldownDuration = new float[4] { 5f, 6f, 7f, 8f };

        if (cooldownRemaining == null || cooldownRemaining.Length != 4)
            cooldownRemaining = new float[4];
    }

    private void ClampHealth()
    {
        if (maxHealth < 1) maxHealth = 1;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    private void RaiseChanged()
    {
        OnStateChanged?.Invoke();
    }
}
