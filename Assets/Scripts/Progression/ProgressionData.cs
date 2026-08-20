using UnityEngine;

/// <summary>
/// Static progression rules: levels 1–15, tier colors, start scores, avatar unlock levels, XP curve.
/// </summary>
public static class ProgressionData
{
    public const int MinLevel = 1;
    public const int MaxLevel = 15;

    public const int AvatarsPerTier = 5;
    public const int AvatarTierCount = 5;
    public const int AvatarCount = AvatarsPerTier * AvatarTierCount; // 25

    /// <summary>Level required to unlock each tier's five avatars (indices tier*5 .. tier*5+4).</summary>
    public static readonly int[] AvatarTierUnlockLevels = { 1, 4, 7, 10, 13 };

    public const int StartScoreOptionCount = 5;

    /// <summary>Selectable run start scores (index 0 = base 0, always unlocked at level 1).</summary>
    public static readonly int[] StartScores = { 0, 10, 20, 30, 40 };

    /// <summary>Level required to unlock each StartScores entry.</summary>
    public static readonly int[] StartScoreUnlockLevels = { 1, 4, 7, 10, 13 };

    /// <summary>XP required to advance from currentLevel to currentLevel+1 (valid for levels 1..14).</summary>
    public static int GetXpToAdvance(int currentLevel)
    {
        if (currentLevel < MinLevel || currentLevel >= MaxLevel)
        {
            return 0;
        }

        return 100 + (currentLevel - 1) * 55;
    }

    /// <summary>Total lifetime XP → current level (1..15) and XP accumulated toward next level.</summary>
    public static void GetLevelProgress(int totalXp, out int level, out int xpIntoCurrentLevel, out int xpNeededForNext)
    {
        level = MinLevel;
        int remaining = Mathf.Max(0, totalXp);

        while (level < MaxLevel)
        {
            int need = GetXpToAdvance(level);
            if (remaining >= need)
            {
                remaining -= need;
                level++;
            }
            else
            {
                xpIntoCurrentLevel = remaining;
                xpNeededForNext = need;
                return;
            }
        }

        xpIntoCurrentLevel = 0;
        xpNeededForNext = 0;
    }

    public static RankTier GetTierForLevel(int level)
    {
        if (level <= 3) return RankTier.Beginner;
        if (level <= 6) return RankTier.Growth;
        if (level <= 9) return RankTier.Mid;
        if (level <= 12) return RankTier.Advanced;
        return RankTier.Endgame;
    }

    /// <summary>Highest start score unlocked by this level (legacy / clamp helper).</summary>
    public static int GetStartScoreForLevel(int level)
    {
        int best = StartScores[0];
        for (int i = 0; i < StartScoreOptionCount; i++)
        {
            if (level >= StartScoreUnlockLevels[i])
            {
                best = StartScores[i];
            }
        }

        return best;
    }

    public static int IndexOfStartScore(int startScore)
    {
        for (int i = 0; i < StartScoreOptionCount; i++)
        {
            if (StartScores[i] == startScore)
            {
                return i;
            }
        }

        return -1;
    }

    public static bool IsStartScoreUnlockedForLevel(int startScore, int level)
    {
        int index = IndexOfStartScore(startScore);
        if (index < 0)
        {
            return false;
        }

        return level >= StartScoreUnlockLevels[index];
    }

    public static int ClampStartScoreToUnlocked(int startScore, int level)
    {
        if (IsStartScoreUnlockedForLevel(startScore, level))
        {
            return startScore;
        }

        return GetStartScoreForLevel(level);
    }

    public static Color GetTierColor(RankTier tier)
    {
        switch (tier)
        {
            case RankTier.Beginner: return new Color32(0x22, 0xAA, 0x44, 0xFF);
            case RankTier.Growth: return new Color32(0xF5, 0xD1, 0x42, 0xFF);
            case RankTier.Mid: return new Color32(0xF5, 0x8A, 0x22, 0xFF);
            case RankTier.Advanced: return new Color32(0xE5, 0x33, 0x33, 0xFF);
            case RankTier.Endgame: return new Color32(0x9B, 0x59, 0xD6, 0xFF);
            default: return Color.white;
        }
    }

    /// <summary>Unlock level for avatar index 0..24 (5 avatars per tier).</summary>
    public static int GetAvatarUnlockLevel(int avatarIndex)
    {
        if (avatarIndex < 0 || avatarIndex >= AvatarCount)
        {
            return MaxLevel;
        }

        int tier = avatarIndex / AvatarsPerTier;
        return AvatarTierUnlockLevels[tier];
    }

    public static bool IsAvatarUnlockedAtLevel(int avatarIndex, int level)
    {
        return avatarIndex >= 0
            && avatarIndex < AvatarCount
            && level >= GetAvatarUnlockLevel(avatarIndex);
    }

    public static int ClampAvatarIndex(int index)
    {
        return Mathf.Clamp(index, 0, AvatarCount - 1);
    }
}

public enum RankTier
{
    Beginner,
    Growth,
    Mid,
    Advanced,
    Endgame
}

/// <summary>Snapshot for UI (localized rank title applied elsewhere).</summary>
public struct ProgressionSnapshot
{
    public int Level;
    public int XpIntoCurrentLevel;
    public int XpNeededForNext;
    public RankTier Tier;
    public int SelectedAvatarIndex;
    public int SelectedStartScore;
}
