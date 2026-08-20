using UnityEngine;
using UnityEngine.UI;

public class TimingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image radialFillImage;

    [Header("Timing")]
    [SerializeField] private float minCycleDuration = 1.5f;
    [SerializeField] private float maxCycleDuration = 2.5f;
    [SerializeField] private float perfectStart = 0.95f;
    [SerializeField] private float perfectEnd = 1f;
    [Tooltip("How long a fully filled pizza still accepts a tap before Too Late.")]
    [SerializeField] private float fullHoldSeconds = 0.1f;
    [Tooltip("Internal fill cap while the fully filled visual is being held.")]
    [SerializeField] private float lateHoldFill = 1.05f;
    [Tooltip("After a cycle starts, taps in this window are ignored to avoid accidental Too Early.")]
    [SerializeField] private float inputGraceSeconds = 0.3f;

    [Header("Score Difficulty")]
    [SerializeField, Min(1)] private int scorePerDifficultyStep = 10;
    [SerializeField, Min(0f)] private float cycleReductionPerStep = 0.1f;
    [SerializeField, Min(0.01f)] private float minimumMinCycleDuration = 1f;
    [SerializeField, Min(0.01f)] private float minimumMaxCycleDuration = 1.3f;
    [SerializeField, Min(1)] private int highScoreFullHoldThreshold = 100;
    [SerializeField, Min(0f)] private float highScoreFullHoldSeconds = 0.05f;

    private float currentCycleDuration;
    private float currentFillAmount;
    private float cycleElapsedSeconds;
    private float lastAppliedVisualFill = -1f;
    private float activeMinCycleDuration;
    private float activeMaxCycleDuration;
    private float activeFullHoldSeconds;
    private bool isRunning;

    public float CurrentFillAmount => currentFillAmount;
    public float PerfectStart => perfectStart;
    public float PerfectEnd => perfectEnd;

    /// <summary>False during the post-reset grace window so panic taps do not count as Too Early.</summary>
    public bool IsInputArmed => isRunning && cycleElapsedSeconds >= inputGraceSeconds;
    /// <summary>True after the pizza has been fully filled for the configured hold duration.</summary>
    public bool HasFullHoldExpired =>
        isRunning && cycleElapsedSeconds >= currentCycleDuration + activeFullHoldSeconds;

    private void Awake()
    {
        ResetScoreDifficulty();

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

        // Do not auto-reset on late fill — GameManager owns Too Late + ResetCycle to avoid script-order races.
        float holdCap = Mathf.Max(perfectEnd + 0.01f, lateHoldFill);
        currentFillAmount += Time.deltaTime / currentCycleDuration;
        currentFillAmount = Mathf.Min(currentFillAmount, holdCap);
        cycleElapsedSeconds += Time.deltaTime;
        ApplyFillVisual();
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
        ResetScoreDifficulty();
        currentFillAmount = 0f;
        cycleElapsedSeconds = 0f;
        ApplyFillVisual(force: true);
    }

    public void ApplyScoreDifficulty(int score)
    {
        int safeStepSize = Mathf.Max(1, scorePerDifficultyStep);
        int difficultySteps = Mathf.Max(0, score) / safeStepSize;
        float reduction = difficultySteps * Mathf.Max(0f, cycleReductionPerStep);

        activeMinCycleDuration = Mathf.Max(minimumMinCycleDuration, minCycleDuration - reduction);
        activeMaxCycleDuration = Mathf.Max(minimumMaxCycleDuration, maxCycleDuration - reduction);
        activeMaxCycleDuration = Mathf.Max(activeMinCycleDuration, activeMaxCycleDuration);
        activeFullHoldSeconds = score >= Mathf.Max(1, highScoreFullHoldThreshold)
            ? Mathf.Max(0f, highScoreFullHoldSeconds)
            : Mathf.Max(0f, fullHoldSeconds);
    }

    public void ResetScoreDifficulty()
    {
        ApplyScoreDifficulty(0);
    }

    public void ResetCycle(bool randomizeDuration)
    {
        currentFillAmount = 0f;
        cycleElapsedSeconds = 0f;
        if (randomizeDuration)
        {
            currentCycleDuration = Random.Range(activeMinCycleDuration, activeMaxCycleDuration);
        }

        ApplyFillVisual(force: true);
    }

    private void ApplyFillVisual(bool force = false)
    {
        if (radialFillImage == null)
        {
            return;
        }

        float visual = Mathf.Clamp01(currentFillAmount);
        if (!force && Mathf.Abs(visual - lastAppliedVisualFill) < 0.0005f)
        {
            return;
        }

        lastAppliedVisualFill = visual;
        radialFillImage.fillAmount = visual;
    }
}
