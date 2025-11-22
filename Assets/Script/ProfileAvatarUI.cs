// ProfileAvatarUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEditor;
#endif

public class ProfileAvatarUI : MonoBehaviour
{
    [Header("Wiring")]
    public Image avatarImage;
    public Sprite defaultAvatar;

    [Header("Generate Picture")]
    public Sprite[] randomSprites;

    [Header("Navigation")]
    public string nextScene = "Dashboard";

    public void OnGeneratePicture()
    {
        if (randomSprites == null || randomSprites.Length == 0)
        {
            var loaded = Resources.LoadAll<Sprite>("Avatars");
            if (loaded != null && loaded.Length > 0)
                randomSprites = loaded;
        }

        Sprite pick = (randomSprites != null && randomSprites.Length > 0)
            ? randomSprites[Random.Range(0, randomSprites.Length)]
            : defaultAvatar;

        if (pick == null) return;

        SetAvatarSprite(pick);

        if (AvatarService.Instance != null)
            AvatarService.Instance.SetAvatarFromSprite(pick, true);
    }

    public void OnAttachFile()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        string path = EditorUtility.OpenFilePanel("Select image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
            LoadAvatarFromPath(path);

#elif UNITY_ANDROID
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (string.IsNullOrEmpty(path)) return;
            LoadAvatarFromPath(path);

        }, "Select a profile picture", "image/*");
#else
        Debug.LogWarning("AttachFile not implemented on this platform.");
#endif
    }

    void LoadAvatarFromPath(string path)
    {
        try
        {
            var bytes = System.IO.File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!tex.LoadImage(bytes)) return;

            var sq = CropSquare(tex);

            var sprite = Sprite.Create(
                sq,
                new Rect(0, 0, sq.width, sq.height),
                new Vector2(0.5f, 0.5f),
                100f
            );

            SetAvatarSprite(sprite);

            if (AvatarService.Instance != null)
                AvatarService.Instance.SetAvatarFromTexture(sq, true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load avatar: " + e.Message);
        }
    }

    public void OnLetsLearn()
    {
        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }

    public void SetAvatarSprite(Sprite s)
    {
        if (!avatarImage) return;
        avatarImage.sprite = s;
        avatarImage.preserveAspect = true;
        avatarImage.enabled = true;
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
}
