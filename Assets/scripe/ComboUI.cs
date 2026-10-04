using System.Collections;
using UnityEngine;
using TMPro;

public class ComboUI : MonoBehaviour
{
    public static ComboUI Instance;

    [Header("UI References")]
    public GameObject panel;            
    public TextMeshProUGUI comboText; 

    [Header("Animation Settings")]
    public float displayDuration = 1.5f; 
    public float punchScale = 1.35f;     

    private Coroutine hideCoroutine;
    private Vector3 originalScale;

    void Awake()
    {
        Instance = this;
        if (panel != null)
        {
            originalScale = panel.transform.localScale;
            panel.SetActive(false); 
        }
    }

    public void ShowCombo(int count)
    {
        if (panel == null || comboText == null) return;

        comboText.text = $"COMBO\nx{count}";

        panel.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(PopEffectRoutine());
    }

    private IEnumerator PopEffectRoutine()
    {
        panel.transform.localScale = originalScale * punchScale;
        float elapsed = 0f;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            panel.transform.localScale = Vector3.Lerp(originalScale * punchScale, originalScale, elapsed / duration);
            yield return null;
        }
        panel.transform.localScale = originalScale;

        yield return new WaitForSeconds(displayDuration);
        panel.SetActive(false);
    }

    public void HideImmediate()
    {
        if (panel != null) panel.SetActive(false);
    }
}