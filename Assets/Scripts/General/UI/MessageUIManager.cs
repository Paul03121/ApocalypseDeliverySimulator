using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageUIManager : MonoBehaviour
{
    public static MessageUIManager Instance;

    [Header("UI References")]
    [SerializeField] private CanvasGroup messagePanel;
    [SerializeField] private TMP_Text messageText;

    [Header("Other HUD Elements")]
    [SerializeField] private CanvasGroup healthBar;
    [SerializeField] private CanvasGroup weaponIcon;

    [Header("Typing Settings")]
    [SerializeField] private float characterDelay = 0.04f;
    [SerializeField] private float extraVisibleTime = 2.5f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    private Coroutine messageRoutine;

    public bool IsShowingMessage => messageRoutine != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        HideInstant();
    }

    // Display message using typing and paging effects
    public void ShowMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        // Stop previous message if one is active
        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        // Enable message panel
        messagePanel.gameObject.SetActive(true);

        // Fade message in and HUD out
        yield return FadeMultiple(0f, 1f, 1f, 0f);

        // Split message into pages that fit the text area
        List<string> pages = SplitMessageIntoPages(message);

        // Show each page sequentially
        foreach (string page in pages)
        {
            yield return TypeText(page);
            yield return new WaitForSeconds(extraVisibleTime);

            messageText.text = string.Empty;
        }

        // Fade message out and HUD back in
        yield return FadeMultiple(1f, 0f, 0f, 1f);

        ClearAndHide();

        messageRoutine = null;
    }

    // Types text character by character
    private IEnumerator TypeText(string message)
    {
        messageText.text = string.Empty;

        foreach (char character in message)
        {
            messageText.text += character;
            yield return new WaitForSeconds(characterDelay);
        }
    }

    // Splits a message into pages that fit the text area
    private List<string> SplitMessageIntoPages(string message)
    {
        List<string> pages = new();
        messageText.text = string.Empty;

        // Split by manual line breaks
        string[] manualBlocks = message.Split('\n');

        foreach (string block in manualBlocks)
        {
            string[] words = block.Split(' ');
            string currentPage = "";

            foreach (string word in words)
            {
                string testPage;

                // Build page text
                if (string.IsNullOrEmpty(currentPage))
                    testPage = word;
                else
                    testPage = currentPage + " " + word;

                // Test overflow
                messageText.text = testPage;
                messageText.ForceMeshUpdate();

                if (messageText.isTextOverflowing)
                {
                    // Save current page and start a new one
                    pages.Add(currentPage);
                    currentPage = word;
                }
                else
                {
                    currentPage = testPage;
                }
            }

            // Add remaining text
            if (!string.IsNullOrWhiteSpace(currentPage))
                pages.Add(currentPage);
        }

        messageText.text = string.Empty;
        return pages;
    }

    // Fades message panel and HUD elements simultaneously
    private IEnumerator FadeMultiple(float fromMessage, float toMessage, float fromHUD, float toHUD)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            messagePanel.alpha = Mathf.Lerp(fromMessage, toMessage, t);
            healthBar.alpha = Mathf.Lerp(fromHUD, toHUD, t);
            weaponIcon.alpha = Mathf.Lerp(fromHUD, toHUD, t);

            yield return null;
        }

        // Ensure final values
        messagePanel.alpha = toMessage;
        healthBar.alpha = toHUD;
        weaponIcon.alpha = toHUD;
    }

    private void HideInstant()
    {
        messagePanel.alpha = 0f;
        messagePanel.gameObject.SetActive(false);
        messageText.text = string.Empty;

        healthBar.alpha = 1f;
        weaponIcon.alpha = 1f;
    }

    private void ClearAndHide()
    {
        messagePanel.alpha = 0f;
        messageText.text = string.Empty;
        messagePanel.gameObject.SetActive(false);
    }
}