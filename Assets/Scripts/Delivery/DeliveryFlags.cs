using System;
using System.Collections.Generic;
using UnityEngine;

public enum DeliveryFlag
{
    TutorialStep1,
    TutorialStep2,
    TutorialStep3,
    TutorialStep4,

    Story1,
    Story2,
    Story3,
    Story4,
    Story5,
    Story6,
    Story7,
    Story8
}

public class DeliveryFlags : MonoBehaviour
{
    public static DeliveryFlags Instance;

    [Serializable]
    public class FlagConfig
    {
        public DeliveryFlag flag;                 // Delivery flag identifier
        public int maxSimultaneousDeliveries;     // Max missions allowed at once for this flag
        public int baseReward;                    // Base reward amount for this flag
    }

    [Header("Current Progress")]
    public DeliveryFlag currentFlag;

    [Header("Configurations")]
    public List<FlagConfig> flagConfigs = new();

    public DeliveryFlag CurrentFlag => currentFlag;

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Persist across scenes
        DontDestroyOnLoad(gameObject);
    }

    public FlagConfig GetConfigForFlag(DeliveryFlag flag)
    {
        FlagConfig result = null;

        foreach (var config in flagConfigs)
        {
            if (flag == config.flag)
            {
                result = config;
                break;
            }
        }
        return result;
    }

    public int GetMaxDeliveries()
    {
        var config = GetConfigForFlag(currentFlag);
        return config != null ? config.maxSimultaneousDeliveries : 0;
    }

    public int GetBaseRewardForFlag(DeliveryFlag flag)
    {
        var config = GetConfigForFlag(flag);
        return config != null ? config.baseReward : 0;
    }

    public bool IsFlagActive(DeliveryFlag flag)
    {
        // Check if a flag is active or passed
        return currentFlag >= flag;
    }

    public void SetFlag(DeliveryFlag newFlag)
    {
        // Prevent regressing progress
        if (newFlag < currentFlag)
            return;

        currentFlag = newFlag;
        Debug.Log($"[DeliveryFlags] Progress advanced to {currentFlag}");
    }
}