using System;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class AvatarService : MonoBehaviour
{
    public static AvatarService Instance { get; private set; }

    [Header("Shown when no avatar chosen yet")]
    public Sprite defaultAvatar;

    public event Action<Sprite> OnAvatarChanged;

    private Sprite _current;
    string SavePath => Path.Combine(Application.persistentDataPath, "avatar.png");

    void Awake()
    {
        if (Instance && Instance != this) { Debug.Log("[AvatarService] Duplicate, destroying self"); Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[AvatarService] Awake, loading disk...");
        LoadFromDiskIfAny();
        Debug.Log("[AvatarService] Awake done. Current=" + (_current ? _current.name : "null") + " | Default=" + (defaultAvatar ? defaultAvatar.name : "null"));
    }

    public Sprite CurrentAvatar => _current ? _current : defaultAvatar;

    public void SetAvatarFromSprite(Sprite s, bool persist = true)
    {
        Debug.Log("[AvatarService] SetAvatarFromSprite: " + (s ? s.name : "null"));
        _current = s;
        OnAvatarChanged?.Invoke(_current);

        if (persist && s && s.texture)
        {
            try
            {
                var sq = CropSquare(s.texture);
                File.WriteAllBytes(SavePath, sq.EncodeToPNG());
                Debug.Log("[AvatarService] Saved to " + SavePath);
            }
            catch (Exception e) { Debug.LogWarning("[AvatarService] Save failed: " + e.Message); }
        }
    }

    public void SetAvatarFromTexture(Texture2D tex, bool persist = true)
    {
        var sq = CropSquare(tex);
        _current = Sprite.Create(sq, new Rect(0, 0, sq.width, sq.height), new Vector2(0.5f, 0.5f), 100f);
        Debug.Log("[AvatarService] SetAvatarFromTexture -> sprite created");
        OnAvatarChanged?.Invoke(_current);

        if (persist)
        {
            try
            {
                File.WriteAllBytes(SavePath, sq.EncodeToPNG());
                Debug.Log("[AvatarService] Saved to " + SavePath);
            }
            catch (Exception e) { Debug.LogWarning("[AvatarService] Save failed: " + e.Message); }
        }
    }

    Texture2D CropSquare(Texture2D src)
    {
        int size = Mathf.Min(src.width, src.height);
        int x = (src.width - size) / 2;
        int y = (src.height - size) / 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.SetPixels(src.GetPixels(x, y, size, size));
        tex.Apply();
        return tex;
    }

    void LoadFromDiskIfAny()
    {
        try
        {
            if (!File.Exists(SavePath)) { Debug.Log("[AvatarService] No saved avatar on disk"); return; }
            var bytes = File.ReadAllBytes(SavePath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(bytes))
            {
                _current = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                Debug.Log("[AvatarService] Loaded avatar from disk");
            }
        }
        catch (Exception e) { Debug.LogWarning("[AvatarService] Load failed: " + e.Message); }
    }
}
