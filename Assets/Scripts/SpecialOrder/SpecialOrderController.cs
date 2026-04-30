using System.Collections;
using System.Text;
using System.Linq;
using UnityEngine;

public class SpecialOrderController : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private OrderSlotDropTarget[] slotTargets;
    [SerializeField] private DraggableIngredient[] sourceBowls;
    [SerializeField] private Sprite[] ingredientSprites;

    private Coroutine _roundRoutine;
    private System.Action _onWin;
    private System.Action _onLose;
    private System.Action<float> _onTimeTick;

    public void BeginRound(float timeSeconds, System.Action onSuccess, System.Action onFail, System.Action<float> onTimeRemaining)
    {
        if (_roundRoutine != null) StopCoroutine(_roundRoutine);
        _onWin = onSuccess;
        _onLose = onFail;
        _onTimeTick = onTimeRemaining;

        NormalizeBowlsOrder();
        RandomizeAndConfigureSlots();
        for (int i = 0; i < sourceBowls.Length; i++)
        {
            DraggableIngredient bowl = sourceBowls[i];
            if (bowl != null) bowl.ResetForRound();
            if (bowl != null)
            {
                bowl.SetIngredient(ResolveBowlIngredientType(bowl, i));
            }
        }

        _roundRoutine = StartCoroutine(CoRun(timeSeconds));
    }

    public void ForceStop()
    {
        if (_roundRoutine != null) StopCoroutine(_roundRoutine);
        _roundRoutine = null;
    }

    private void RandomizeAndConfigureSlots()
    {
        for (int i = 0; i < slotTargets.Length; i++)
        {
            var t = (IngredientType)UnityEngine.Random.Range(0, 4);
            Sprite sp = ResolveSpriteForType(t);
            if (slotTargets[i] != null) slotTargets[i].Configure(t, sp);
        }
    }

    private bool AllSlotsFilled()
    {
        for (int i = 0; i < slotTargets.Length; i++)
        {
            if (slotTargets[i] == null || !slotTargets[i].IsFilled) return false;
        }
        return slotTargets.Length >= 5;
    }

    private IEnumerator CoRun(float timeLeft)
    {
        while (timeLeft > 0f)
        {
            if (AllSlotsFilled())
            {
                _onWin?.Invoke();
                _roundRoutine = null;
                yield break;
            }
            timeLeft -= Time.deltaTime;
            if (_onTimeTick != null) _onTimeTick(Mathf.Max(0f, timeLeft));
            yield return null;
        }

        if (AllSlotsFilled())
        {
            _onWin?.Invoke();
        }
        else
        {
            _onLose?.Invoke();
        }
        _roundRoutine = null;
    }

    public void ResetToIdle()
    {
        ForceStop();
        foreach (var s in slotTargets)
        {
            if (s != null) s.ClearSlot();
        }
        foreach (var b in sourceBowls)
        {
            if (b != null) b.ResetForRound();
        }
    }

    private static IngredientType ResolveBowlIngredientType(DraggableIngredient bowl, int fallbackIndex)
    {
        string n = bowl.gameObject.name;
        int parsedIndex = ExtractTrailingNumber(n);
        if (parsedIndex >= 0 && parsedIndex < 4)
        {
            return (IngredientType)parsedIndex;
        }

        if (fallbackIndex >= 0 && fallbackIndex < 4)
        {
            return (IngredientType)fallbackIndex;
        }

        return IngredientType.Sucuk;
    }

    private static int ExtractTrailingNumber(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return -1;
        }

        StringBuilder sb = new StringBuilder();
        for (int i = input.Length - 1; i >= 0; i--)
        {
            char c = input[i];
            if (char.IsDigit(c))
            {
                sb.Insert(0, c);
            }
            else
            {
                break;
            }
        }

        if (sb.Length == 0)
        {
            return -1;
        }

        if (int.TryParse(sb.ToString(), out int value))
        {
            return value;
        }

        return -1;
    }

    private void NormalizeBowlsOrder()
    {
        if (sourceBowls == null || sourceBowls.Length <= 1)
        {
            return;
        }

        sourceBowls = sourceBowls
            .Where(b => b != null)
            .OrderBy(b => ExtractTrailingNumber(b.gameObject.name))
            .ToArray();
    }

    private Sprite ResolveSpriteForType(IngredientType type)
    {
        int idx = (int)type;
        if (ingredientSprites != null && idx >= 0 && idx < ingredientSprites.Length && ingredientSprites[idx] != null)
        {
            return ingredientSprites[idx];
        }

        if (sourceBowls != null)
        {
            for (int i = 0; i < sourceBowls.Length; i++)
            {
                DraggableIngredient bowl = sourceBowls[i];
                if (bowl == null) continue;
                if (bowl.Ingredient != type) continue;
                Sprite s = bowl.IngredientSprite;
                if (s != null) return s;
            }
        }

        return null;
    }
}
