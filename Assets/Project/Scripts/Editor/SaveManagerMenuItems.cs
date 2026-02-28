using UnityEditor;
using UnityEngine;
using Expedition0.Save;
using System;
using System.Linq;

// This class contains all the static methods hooked to the Unity menu bar.
public static class SaveManagerMenuItems
{
    private const string MENU_ROOT = "E0 Tools/Game Progress/";
    private const int SAVE_BITS = 5; // Corresponds to the highest bit (SeenIllusion = 1 << 4)

    // =========================================================
    // SECTION A: Core Utility Methods
    // =========================================================

    [MenuItem(MENU_ROOT + "0. View Current Save Data", false, 10)]
    public static void ViewCurrentSaveData()
    {
        int saveData = SaveManager.LoadSave();
        string binary = Convert.ToString(saveData, 2).PadLeft(SAVE_BITS, '0');
        
        // This is a more detailed view using your provided SaveManager methods
        Debug.Log($"<color=cyan>SaveManager Debug:</color> Current Game Progress Status\n" +
                  $"- Key: GameProgressBits\n" +
                  $"- Int Value: {saveData}\n" +
                  $"- Binary Mask: <b>0b{binary}</b>\n" +
                  $"- Enabled Flags: {SaveManager.LoadProgress()}\n" +
                  $"- Main Levels Completed: {SaveManager.MainLevelsCompletedCount(saveData)}");
    }

    [MenuItem(MENU_ROOT + "1. Reset Save (Delete Key)", false, 11)]
    public static void ResetGameSave()
    {
        SaveManager.ResetSave();
        Debug.Log("<color=red>SaveManager Debug:</color> Game progress has been FULLY RESET.");
        ViewCurrentSaveData();
    }
    
    [MenuItem(MENU_ROOT + "2. Check for Forbidden Combination", false, 12)]
    public static void CheckForbiddenCombination()
    {
        int saveData = SaveManager.LoadSave();
        bool isForbidden = SaveManager.IsForbiddenCombination(saveData);

        if (isForbidden)
        {
            Debug.LogError($"<color=red>SaveManager WARNING:</color> Forbidden combination detected! Save may be corrupted or cheated: 0b{Convert.ToString(saveData, 2)}");
        }
        else
        {
            Debug.Log("<color=green>SaveManager Debug:</color> Current save data passes all integrity checks.");
        }
    }

    // =========================================================
    // SECTION B: Management Logic (Setter/Unsetter)
    // =========================================================

    /// <summary>Sets a single flag using the SaveManager method.</summary>
    private static void SetFlag(GameProgress flag)
    {
        SaveManager.SetCompleted(flag);
        Debug.Log($"<color=lime>SaveManager:</color> Flag '<b>{flag}</b>' has been set.");
        ViewCurrentSaveData();
    }

    /// <summary>Unsets a single flag using direct PlayerPrefs manipulation.</summary>
    private static void UnsetFlag(GameProgress flag)
    {
        // Note: You don't have an Unset method in SaveManager, so we implement it here.
        const string SAVE_KEY = "GameProgressBits"; 
        int currentSave = PlayerPrefs.GetInt(SAVE_KEY, 0);
        
        // Bitwise AND with the NOT of the flag value clears the bit.
        currentSave &= ~(int)flag;
        
        PlayerPrefs.SetInt(SAVE_KEY, currentSave);
        PlayerPrefs.Save();
        
        Debug.Log($"<color=yellow>SaveManager:</color> Flag '<b>{flag}</b>' has been UNSET.");
        ViewCurrentSaveData();
    }

    // --- LEVEL 0 (TUTORIAL) ---

    // Validation: Only show "SET" option if the flag is NOT set.
    [MenuItem(MENU_ROOT + "Set Flag/0. Level0_Tutorial", true, 30)]
    private static bool ValidateSetFlag_L0() => !SaveManager.IsCompleted(GameProgress.Level0_Tutorial);
    // Execution
    [MenuItem(MENU_ROOT + "Set Flag/0. Level0_Tutorial", false, 30)]
    private static void ExecuteSetFlag_L0() => SetFlag(GameProgress.Level0_Tutorial);
    
    // Validation: Only show "UNSET" option if the flag IS set.
    [MenuItem(MENU_ROOT + "Unset Flag/0. Level0_Tutorial", true, 50)]
    private static bool ValidateUnsetFlag_L0() => SaveManager.IsCompleted(GameProgress.Level0_Tutorial);
    // Execution
    [MenuItem(MENU_ROOT + "Unset Flag/0. Level0_Tutorial", false, 50)]
    private static void ExecuteUnsetFlag_L0() => UnsetFlag(GameProgress.Level0_Tutorial);

    // --- LEVEL 1 (GREENHOUSE) ---
    
    [MenuItem(MENU_ROOT + "Set Flag/1. Level1_Greenhouse", true, 31)]
    private static bool ValidateSetFlag_L1() => !SaveManager.IsCompleted(GameProgress.Level1_Greenhouse);
    [MenuItem(MENU_ROOT + "Set Flag/1. Level1_Greenhouse", false, 31)]
    private static void ExecuteSetFlag_L1() => SetFlag(GameProgress.Level1_Greenhouse);
    
    [MenuItem(MENU_ROOT + "Unset Flag/1. Level1_Greenhouse", true, 51)]
    private static bool ValidateUnsetFlag_L1() => SaveManager.IsCompleted(GameProgress.Level1_Greenhouse);
    [MenuItem(MENU_ROOT + "Unset Flag/1. Level1_Greenhouse", false, 51)]
    private static void ExecuteUnsetFlag_L1() => UnsetFlag(GameProgress.Level1_Greenhouse);

    // --- LEVEL 2 (OUTER SKELETON) ---

    [MenuItem(MENU_ROOT + "Set Flag/2. Level2_OuterSkeleton", true, 32)]
    private static bool ValidateSetFlag_L2() => !SaveManager.IsCompleted(GameProgress.Level2_OuterSkeleton);
    [MenuItem(MENU_ROOT + "Set Flag/2. Level2_OuterSkeleton", false, 32)]
    private static void ExecuteSetFlag_L2() => SetFlag(GameProgress.Level2_OuterSkeleton);
    
    [MenuItem(MENU_ROOT + "Unset Flag/2. Level2_OuterSkeleton", true, 52)]
    private static bool ValidateUnsetFlag_L2() => SaveManager.IsCompleted(GameProgress.Level2_OuterSkeleton);
    [MenuItem(MENU_ROOT + "Unset Flag/2. Level2_OuterSkeleton", false, 52)]
    private static void ExecuteUnsetFlag_L2() => UnsetFlag(GameProgress.Level2_OuterSkeleton);
    
    // --- LEVEL 3 (MACHINE HALL) ---

    [MenuItem(MENU_ROOT + "Set Flag/3. Level3_MachineHall", true, 33)]
    private static bool ValidateSetFlag_L3() => !SaveManager.IsCompleted(GameProgress.Level3_MachineHall);
    [MenuItem(MENU_ROOT + "Set Flag/3. Level3_MachineHall", false, 33)]
    private static void ExecuteSetFlag_L3() => SetFlag(GameProgress.Level3_MachineHall);
    
    [MenuItem(MENU_ROOT + "Unset Flag/3. Level3_MachineHall", true, 53)]
    private static bool ValidateUnsetFlag_L3() => SaveManager.IsCompleted(GameProgress.Level3_MachineHall);
    [MenuItem(MENU_ROOT + "Unset Flag/3. Level3_MachineHall", false, 53)]
    private static void ExecuteUnsetFlag_L3() => UnsetFlag(GameProgress.Level3_MachineHall);
    
    // --- SEEN ILLUSION ---

    [MenuItem(MENU_ROOT + "Set Flag/4. SeenIllusion", true, 34)]
    private static bool ValidateSetFlag_SeenIllusion() => !SaveManager.IsCompleted(GameProgress.SeenIllusion);
    [MenuItem(MENU_ROOT + "Set Flag/4. SeenIllusion", false, 34)]
    private static void ExecuteSetFlag_SeenIllusion() => SetFlag(GameProgress.SeenIllusion);
    
    [MenuItem(MENU_ROOT + "Unset Flag/4. SeenIllusion", true, 54)]
    private static bool ValidateUnsetFlag_SeenIllusion() => SaveManager.IsCompleted(GameProgress.SeenIllusion);
    [MenuItem(MENU_ROOT + "Unset Flag/4. SeenIllusion", false, 54)]
    private static void ExecuteUnsetFlag_SeenIllusion() => UnsetFlag(GameProgress.SeenIllusion);
    
    // --- QUICK SET (For testing conditional logic) ---
    [MenuItem(MENU_ROOT + "Quick Set/Set Everything Completed", false, 70)]
    private static void QuickSetEverything() => SetFlag(GameProgress.All);
    [MenuItem(MENU_ROOT + "Quick Set/Set All Main Levels Completed", false, 70)]
    private static void QuickSetAllMain() => SetFlag(GameProgress.MainLevels);
    [MenuItem(MENU_ROOT + "Quick Set/Set Encore Mode Enabled", false, 71)]
    private static void QuickSetEncoreMode()
    {
        // To guarantee Encore mode is enabled, we set enough flags (e.g., Level 1, 2)
        SetFlag(GameProgress.Level1_Greenhouse | GameProgress.Level2_OuterSkeleton);
    }
}