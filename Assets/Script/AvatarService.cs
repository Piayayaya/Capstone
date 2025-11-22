// AvatarService.cs
using UnityEngine;

public class AvatarService : MonoBehaviour
{
    public static AvatarService Instance { get; private set; }

    [Header("Default (shown when none chosen yet)")]
    public Sprite defaultAvatar;   // drag profile1 here

    const string SpriteKeyBase = "profile_avatar_sprite"; // stores sprite name
    const string PathKeyBase = "profile_avatar_path";   // stores custom png path

    public Sprite CurrentAvatar { get; private set; }

    public event System.Action<Sprite> OnAvatarChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFromPrefs();
    }

    string SpriteKeyFor(string uid) => string.IsNullOrEmpty(uid) ? SpriteKeyBase : $"{SpriteKeyBase}_{uid}";
    string PathKeyFor(string uid) => string.IsNullOrEmpty(uid) ? PathKeyBase : $"{PathKeyBase}_{uid}";

    public void LoadFromPrefs()
    {
        string uid = UserIdProvider.ActiveUserId;

        string pathKey = PathKeyFor(uid);
        string spriteKey = SpriteKeyFor(uid);

        // 1) try custom image path
        if (PlayerPrefs.HasKey(pathKey))
        {
            string path = PlayerPrefs.GetString(pathKey, "");
            var spr = LoadSpriteFromFile(path);
            if (spr != null)
            {
                CurrentAvatar = spr;
                return;
            }
        }

        // 2) try sprite name from Resources/Avatars
        if (PlayerPrefs.HasKey(spriteKey))
        {
            string spriteName = PlayerPrefs.GetString(spriteKey, "");
            if (!string.IsNullOrEmpty(spriteName))
            {
                var resSprite = Resources.Load<Sprite>("Avatars/" + spriteName);
                if (resSprite != null)
                {
                    CurrentAvatar = resSprite;
                    return;
                }
            }
        }

        // 3) fallback
        CurrentAvatar = defaultAvatar;
    }

    public bool HasAvatar()
        => CurrentAvatar != null;

    public void SetAvatarFromSprite(Sprite s, bool save = true)
    {
        if (s == null) s = defaultAvatar;

        CurrentAvatar = s;

        if (save)
        {
            string uid = UserIdProvider.ActiveUserId;
            PlayerPrefs.SetString(SpriteKeyFor(uid), s ? s.name : "");
            PlayerPrefs.DeleteKey(PathKeyFor(uid)); // clear custom path
            PlayerPrefs.Save();
        }

        OnAvatarChanged?.Invoke(CurrentAvatar);
    }

    public void SetAvatarFromTexture(Texture2D tex, bool save = true)
    {
        if (tex == null) return;

        // save png to disk
        string uid = UserIdProvider.ActiveUserId;
        string file = $"{uid}_avatar.png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, file);

        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());

        var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100f);
        CurrentAvatar = spr;

        if (save)
        {
            PlayerPrefs.SetString(PathKeyFor(uid), path);
            PlayerPrefs.DeleteKey(SpriteKeyFor(uid)); // clear sprite name
            PlayerPrefs.Save();
        }

        OnAvatarChanged?.Invoke(CurrentAvatar);
    }

    Sprite LoadSpriteFromFile(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                return null;

            byte[] bytes = System.IO.File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes)) return null;

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100f);
        }
        catch { return null; }
    }
}
