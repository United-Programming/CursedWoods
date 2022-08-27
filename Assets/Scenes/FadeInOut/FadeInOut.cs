using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
    public enum FadeStatus {
        Idle,
        Running,
        FadeIn,
        FadeOut,
    }

    public FadeStatus fadeStatus = FadeStatus.Idle;
    public float fadeSpeed = 5f;
    public TextMeshProUGUI levelText;
    public CanvasGroup canvasGroup;
    
    void Start()
    {
        levelText.text = "Level XX - Unknown";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) {
            FadeIn();
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            FadeOut();
        }

        switch (fadeStatus) {
            case FadeStatus.FadeIn:
                StartCoroutine(FadeCoroutine(true));
                break;
            case FadeStatus.FadeOut:
                StartCoroutine(FadeCoroutine(false));
                break;
            case FadeStatus.Running:
            case FadeStatus.Idle: 
            default: 
                break;
        }
    }
    
    public void FadeIn() {
        if (fadeStatus != FadeStatus.Idle) return;
        fadeStatus = FadeStatus.FadeIn;
    }

    public void FadeOut() {
        if (fadeStatus != FadeStatus.Idle) return;
        fadeStatus = FadeStatus.FadeOut;
    }

    private IEnumerator FadeCoroutine(bool isObjectFadingIn) {
        fadeStatus = FadeStatus.Running;
        
        if (isObjectFadingIn) {
            while (canvasGroup.alpha < 1f) {
                canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + fadeSpeed * Time.deltaTime);
                yield return null;
            }
        }
        else {
            while (canvasGroup.alpha > 0f) {
                canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha - fadeSpeed * Time.deltaTime);
                yield return null;
            }
        }

        fadeStatus = FadeStatus.Idle;
    }
}
