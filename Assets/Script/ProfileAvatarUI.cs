using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEditor;   // for file picker in Editor/PC
#endif

public class ProfileAvatarUI : MonoBehaviour
{
    [Header("Wiring")]
    public Image avatarImage;                 // the circular UI Image showing the avatar
    public Sprite defaultAvatar;              // optional fallback

    [Header("Generate Picture")]
    public Sprite[] randomSprites;            // assign your random avatar sprites here (or leave empty to load from Resources/Avatars)

    [Header("Navigation")]
    public string nextScene = "Dashboard";    // used by Let's Learn

    // -----------------------
    // Buttons
    // -----------------------

    // Generate Picture
    public void OnGeneratePicture()
    {
        // Allow Resources fallback if you prefer folder-based setup
        if ((randomSprites == null || randomSprites.Length == 0))
        {
            var loaded = Resources.LoadAll<Sprite>("Avatars"); // put images under Assets/Resources/Avatars
            if (loaded != null && loaded.Length > 0) randomSprites = loaded;
        }

        if (randomSprites == null || randomSprites.Length == 0)
        {
            Debug.LogWarning("[ProfileAvatarUI] No randomSprites assigned (or none in Resources/Avatars).");
            if (defaultAvatar) SetAvatarSprite(defaultAvatar);
            return;
        }

        var pick = randomSprites[Random.Range(0, randomSprites.Length)];
        SetAvatarSprite(pick);
    }

    // Attach File
    public void OnAttachFile()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        string path = EditorUtility.OpenFilePanel("Select image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            var bytes = System.IO.File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(bytes))
            {
                SetAvatarSprite(SpriteFromTexture(tex));
            }
            else
            {
                Debug.LogError("[ProfileAvatarUI] Failed to load image.");
            }
        }
#elif UNITY_ANDROID || UNITY_IOS
        // Requires: https://github.com/yasirkula/UnityNativeGallery (free)
        // Import the NativeGallery plugin, then uncomment the block below.

        /* NativeGallery.Permission perm = NativeGallery.GetImageFromGallery((path) =>
        {
            if (path == null) return;
            var tex = NativeGallery.LoadImageAtPath(path, 1024, false, false, true);
            if (tex != null) SetAvatarSprite(SpriteFromTexture(tex));
            else Debug.LogError("[ProfileAvatarUI] Couldn't load texture from: " + path);
        }, "Select a profile picture", "image/*");
        if (perm == NativeGallery.Permission.Denied) Debug.LogWarning("Gallery permission denied."); */
#else
        Debug.LogWarning("[ProfileAvatarUI] File picker not implemented on this platform.");
#endif
    }

    // Let's Learn
    public void OnLetsLearn()
    {
        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }

    // -----------------------
    // Helpers
    // -----------------------

    void SetAvatarSprite(Sprite s)
    {
        if (!avatarImage) return;
        avatarImage.sprite = s;
        avatarImage.preserveAspect = true;
        avatarImage.enabled = true;
    }

    Sprite SpriteFromTexture(Texture2D tex)
    {
        // center-crop to square so it fits a circular mask nicely
        int size = Mathf.Min(tex.width, tex.height);
        int x = (tex.width - size) / 2;
        int y = (tex.height - size) / 2;
        var cropped = new Texture2D(size, size, TextureFormat.RGBA32, false);
        cropped.SetPixels(tex.GetPixels(x, y, size, size));
        cropped.Apply();

        return Sprite.Create(cropped, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
