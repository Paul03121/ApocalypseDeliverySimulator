using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int startingMoney = 100;

    private int currentMoney;

    public int CurrentMoney => currentMoney;

    // Event fired whenever money changes
    public event Action<int> OnMoneyChanged;

    private void Awake()
    {
        // Initialize wallet with starting money
        currentMoney = startingMoney;

        // Notify listeners
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public void AddMoney(int amount)
    {
        // Returns false if amount is negative
        if (amount <= 0)
            return;

        currentMoney += amount;

        // Notify listeners
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        // Returns false if amount is negative
        if (amount <= 0)
            return false;

        // Returns false if not enough money
        if (!HasEnough(amount))
            return false;

        currentMoney -= amount;

        // Notify listeners
        OnMoneyChanged?.Invoke(currentMoney);
        return true;
    }

    public void SetMoney(int amount)
    {
        // Prevent negative amount of money
        currentMoney = Mathf.Max(0, amount);

        // Notify listeners
        OnMoneyChanged?.Invoke(currentMoney);
    }

    // Checks if the player has enough money
    public bool HasEnough(int amount)
    {
        return currentMoney >= amount;
    }
}