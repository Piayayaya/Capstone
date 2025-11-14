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
    public Sprite[] randomSprites; // or use Resources/Avatars

    [Header("Navigation")]
    public string nextScene = "Dashboard";

    public void OnGeneratePicture()
    {
        Debug.Log("[ProfileAvatarUI] Generate clicked");
        if (randomSprites == null || randomSprites.Length == 0)
        {
            var loaded = Resources.LoadAll<Sprite>("Avatars");
            if (loaded != null && loaded.Length > 0) randomSprites = loaded;
        }

        Sprite pick = null;
        if (randomSprites != null && randomSprites.Length > 0)
            pick = randomSprites[Random.Range(0, randomSprites.Length)];
        else
            pick = defaultAvatar;

        if (pick != null)
        {
            SetAvatarSprite(pick);
            if (AvatarService.Instance)
                AvatarService.Instance.SetAvatarFromSprite(pick, true);
        }
        else
        {
            Debug.LogWarning("[ProfileAvatarUI] No sprite to set (no randomSprites and no defaultAvatar)");
        }
    }

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
                var sq = CropSquare(tex);
                var sprite = Sprite.Create(sq, new Rect(0, 0, sq.width, sq.height), new Vector2(0.5f, 0.5f), 100f);
                Debug.Log("[ProfileAvatarUI] Attach loaded -> setting avatar");
                SetAvatarSprite(sprite);
                if (AvatarService.Instance)
                    AvatarService.Instance.SetAvatarFromTexture(tex, true);
            }
        }
#else
        Debug.LogWarning("[ProfileAvatarUI] Implement gallery picker for mobile (e.g., NativeGallery).");
#endif
    }

    public void OnLetsLearn()
    {
        Debug.Log("[ProfileAvatarUI] Let's Learn -> " + nextScene);
        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }

    public void SetAvatarSprite(Sprite s)
    {
        if (!avatarImage) { Debug.LogError("[ProfileAvatarUI] avatarImage not assigned"); return; }
        avatarImage.sprite = s;
        avatarImage.preserveAspect = true;
        avatarImage.enabled = true;
        Debug.Log("[ProfileAvatarUI] avatarImage set to: " + (s ? s.name : "null"));
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
