using System.IO;
using System.IO.Compression;
using UnityEngine;

public static class VoskModelInstaller
{
    // returns absolute extracted path
    public static string EnsureExtracted(string modelFolderName)
    {
        string dst = Path.Combine(Application.persistentDataPath, modelFolderName);
        if (Directory.Exists(dst)) return dst;

        string zipPath = Path.Combine(Application.streamingAssetsPath, modelFolderName + ".zip");
#if UNITY_ANDROID && !UNITY_EDITOR
        // StreamingAssets is inside APK; use UnityWebRequest to read bytes
        var www = UnityEngine.Networking.UnityWebRequest.Get(zipPath);
        www.SendWebRequest();
        while (!www.isDone) { } // simple sync block; acceptable on first boot
        if (!string.IsNullOrEmpty(www.error)) { Debug.LogError("Model load error: " + www.error); return null; }
        byte[] data = www.downloadHandler.data;

        string tmpZip = Path.Combine(Application.temporaryCachePath, modelFolderName + ".zip");
        File.WriteAllBytes(tmpZip, data);
        ZipFile.ExtractToDirectory(tmpZip, dst);
        try { File.Delete(tmpZip); } catch { }
#else
        ZipFile.ExtractToDirectory(zipPath, dst);
#endif
        return dst;
    }
}
