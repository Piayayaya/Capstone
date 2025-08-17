using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public string Username { get; private set; }
    public int Level { get; private set; }
    public int CurrentEXP { get; private set; }
    private int baseEXP = 100;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadPlayerData();
    }

    public void LoadPlayerData()
    {
        (string playerName, int level, int currentEXP) = DatabaseManager.Instance.LoadProgress();
        if (!string.IsNullOrEmpty(playerName))
        {
            Username = playerName;
            Level = level;
            CurrentEXP = currentEXP;
        }
        else
        {
            ShowUsernameInputPanel();
        }
    }

    private void ShowUsernameInputPanel()
    {
        UIManager.Instance.ShowUsernameInputPanel();
    }

    public void SetUsername(string username)
    {
        Username = username;
        Level = 1;
        CurrentEXP = 0;
        DatabaseManager.Instance.InitializePlayerData(Username);
    }

    public void AddEXP(int expToAdd)
    {
        CurrentEXP += expToAdd;
        CheckLevelUp();
        SaveProgress();
    }

    private void CheckLevelUp()
    {
        int expToLevelUp = GetEXPToLevelUp();

        while (CurrentEXP >= expToLevelUp)
        {
            CurrentEXP -= expToLevelUp;
            Level++;
            expToLevelUp = GetEXPToLevelUp();
            Debug.Log($"🎉 Level Up! New Level: {Level}");
        }
    }

    private int GetEXPToLevelUp()
    {
        return baseEXP + Mathf.CeilToInt((baseEXP / 2f) * (Level - 1));
    }

    private void SaveProgress()
    {
        DatabaseManager.Instance.SaveProgress(Username, Level, CurrentEXP);
    }
}
