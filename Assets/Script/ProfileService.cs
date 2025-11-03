// ProfileService.cs
using System;
using UnityEngine;

[Serializable] public class ProfileData { public string displayName = "Player"; }

public class ProfileService : MonoBehaviour
{
    public static ProfileService Instance { get; private set; }
    public ProfileData Current = new ProfileData();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[ProfileService] ready");
    }

    public void SetName(string name)
    {
        Current.displayName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
        Debug.Log("[ProfileService] Name set: " + Current.displayName);
    }
}
