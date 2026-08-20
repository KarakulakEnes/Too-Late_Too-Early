using UnityEngine;

/// <summary>
/// Persists XP/level/avatars/start score and applies run XP. Attach to GameSystems (or same object as GameManager).
/// Avatar unlocks are derived from current level (5 avatars per tier, 25 total).
/// </summary>
public class ProgressionService : MonoBehaviour
{
    private const string KeyTotalXp = "progress.totalXp";
    private const string KeySelectedAvatar = "progress.selectedAvatar";
    private const string KeySelectedStartScore = "progress.selectedStartScore";
    private const string KeyStartScoreSchemaVersion = "progress.startScoreSchemaVersion";
    private const int CurrentStartScoreSchemaVersion = 2;

    private int _totalXp;
    private int _selectedAvatarIndex;
    private int _selectedStartScore;

    private void Awake()
    {
        Load();
    }

    public void Load()
    {
        _totalXp = PlayerPrefs.GetInt(KeyTotalXp, 0);
        int loadedAvatar = ProgressionData.ClampAvatarIndex(PlayerPrefs.GetInt(KeySelectedAvatar, 0));
        _selectedAvatarIndex = loadedAvatar;

        EnsureSelectedAvatarUnlocked();

        ProgressionData.GetLevelProgress(_totalXp, out int level, out _, out _);
        int loadedStartScore;
        bool migratedStartScore = false;
        if (PlayerPrefs.HasKey(KeySelectedStartScore))
        {
            loadedStartScore = PlayerPrefs.GetInt(KeySelectedStartScore, 0);
            int schemaVersion = PlayerPrefs.GetInt(KeyStartScoreSchemaVersion, 1);
            if (schemaVersion < CurrentStartScoreSchemaVersion)
            {
                loadedStartScore = MigrateLegacyStartScore(loadedStartScore);
                migratedStartScore = true;
            }
        }
        else
        {
            // Existing saves: keep previous auto-max behavior until the player picks manually.
            loadedStartScore = ProgressionData.GetStartScoreForLevel(level);
            migratedStartScore = true;
        }

        _selectedStartScore = loadedStartScore;
        EnsureSelectedStartScoreUnlocked();

        bool dirty = migratedStartScore
            || _selectedAvatarIndex != loadedAvatar
            || _selectedStartScore != loadedStartScore;
        if (dirty)
        {
            Save();
        }
    }

    public void Save()
    {
        PlayerPrefs.SetInt(KeyTotalXp, _totalXp);
        PlayerPrefs.SetInt(KeySelectedAvatar, _selectedAvatarIndex);
        PlayerPrefs.SetInt(KeySelectedStartScore, _selectedStartScore);
        PlayerPrefs.SetInt(KeyStartScoreSchemaVersion, CurrentStartScoreSchemaVersion);
        PlayerPrefs.Save();
    }

    private static int MigrateLegacyStartScore(int legacyStartScore)
    {
        switch (legacyStartScore)
        {
            case 15: return 10;
            case 30: return 20;
            case 50: return 30;
            case 70: return 40;
            default: return legacyStartScore;
        }
    }

    public static int ComputeRunXp(int finalScore)
    {
        return 20 + finalScore * 8;
    }

    /// <summary>Award XP for a finished run (idempotent calls should be guarded by caller).</summary>
    public ProgressionSnapshot ApplyRunXp(int finalScore)
    {
        int gained = ComputeRunXp(finalScore);
        _totalXp = Mathf.Max(0, _totalXp + gained);
        EnsureSelectedAvatarUnlocked();
        EnsureSelectedStartScoreUnlocked();
        Save();
        return BuildSnapshot();
    }

    public ProgressionSnapshot BuildSnapshot()
    {
        ProgressionData.GetLevelProgress(_totalXp, out int level, out int xpInto, out int xpNeed);
        return new ProgressionSnapshot
        {
            Level = level,
            XpIntoCurrentLevel = xpInto,
            XpNeededForNext = xpNeed,
            Tier = ProgressionData.GetTierForLevel(level),
            SelectedAvatarIndex = _selectedAvatarIndex,
            SelectedStartScore = _selectedStartScore
        };
    }

    public int GetStartScoreForCurrentProgress()
    {
        EnsureSelectedStartScoreUnlocked();
        return _selectedStartScore;
    }

    public bool IsStartScoreUnlocked(int startScore)
    {
        ProgressionData.GetLevelProgress(_totalXp, out int level, out _, out _);
        return ProgressionData.IsStartScoreUnlockedForLevel(startScore, level);
    }

    public bool TrySelectStartScore(int startScore)
    {
        if (!IsStartScoreUnlocked(startScore))
        {
            return false;
        }

        _selectedStartScore = startScore;
        Save();
        return true;
    }

    public bool TrySelectAvatar(int index)
    {
        index = ProgressionData.ClampAvatarIndex(index);
        if (!IsAvatarUnlocked(index))
        {
            return false;
        }

        _selectedAvatarIndex = index;
        Save();
        return true;
    }

    public bool IsAvatarUnlocked(int index)
    {
        ProgressionData.GetLevelProgress(_totalXp, out int level, out _, out _);
        return ProgressionData.IsAvatarUnlockedAtLevel(index, level);
    }

    public ProgressionSnapshot ResetProgressionToDefaults()
    {
        _totalXp = 0;
        _selectedAvatarIndex = 0;
        _selectedStartScore = 0;
        Save();
        return BuildSnapshot();
    }

    private void EnsureSelectedAvatarUnlocked()
    {
        if (IsAvatarUnlocked(_selectedAvatarIndex))
        {
            return;
        }

        for (int i = 0; i < ProgressionData.AvatarCount; i++)
        {
            if (IsAvatarUnlocked(i))
            {
                _selectedAvatarIndex = i;
                return;
            }
        }

        _selectedAvatarIndex = 0;
    }

    private void EnsureSelectedStartScoreUnlocked()
    {
        ProgressionData.GetLevelProgress(_totalXp, out int level, out _, out _);
        _selectedStartScore = ProgressionData.ClampStartScoreToUnlocked(_selectedStartScore, level);
    }
}
