using UnityEngine;
using UnityEngine.UI;

public class TimingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image radialFillImage;

    [Header("Timing")]
    [SerializeField] private float minCycleDuration = 1.5f;
    [SerializeField] private float maxCycleDuration = 2.6f;
    [SerializeField] private float perfectStart = 0.95f;
    [SerializeField] private float perfectEnd = 1f;
    [SerializeField] private float lateResetThreshold = 1.1f;

    private float currentCycleDuration;
    private float currentFillAmount;
    private bool isRunning;

    public float CurrentFillAmount => currentFillAmount;
    public float PerfectStart => perfectStart;
    public float PerfectEnd => perfectEnd;

    private void Awake()
    {
        if (radialFillImage != null)
        {
            radialFillImage.type = Image.Type.Filled;
            radialFillImage.fillMethod = Image.FillMethod.Radial360;
        }
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        if (currentCycleDuration <= Mathf.Epsilon)
        {
            currentCycleDuration = minCycleDuration;
        }

        currentFillAmount += Time.deltaTime / currentCycleDuration;
        currentFillAmount = Mathf.Clamp(currentFillAmount, 0f, lateResetThreshold);
        ApplyFillVisual();

        if (currentFillAmount >= lateResetThreshold)
        {
            ResetCycle(true);
        }
    }

    public void Begin()
    {
        isRunning = true;
        ResetCycle(true);
    }

    public void Stop()
    {
        isRunning = false;
    }

    public void Clear()
    {
        Stop();
        currentFillAmount = 0f;
        ApplyFillVisual();
    }

    public void ResetCycle(bool randomizeDuration)
    {
        currentFillAmount = 0f;
        if (randomizeDuration)
        {
            currentCycleDuration = Random.Range(minCycleDuration, maxCycleDuration);
        }

        ApplyFillVisual();
    }

    private void ApplyFillVisual()
    {
        if (radialFillImage != null)
        {
            radialFillImage.fillAmount = Mathf.Clamp01(currentFillAmount);
        }
    }
}
