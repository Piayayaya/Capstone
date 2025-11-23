using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public static class DeviceKeyService
{
    private const string PrefKey = "brainyme_device_key";

    // Stable per install/device
    public static string GetOrCreateDeviceKey()
    {
        if (PlayerPrefs.HasKey(PrefKey))
            return PlayerPrefs.GetString(PrefKey);

        // Use Unity deviceUniqueIdentifier + hash it
        string raw = SystemInfo.deviceUniqueIdentifier;

        // If deviceUniqueIdentifier is empty, fallback to random GUID once
        if (string.IsNullOrEmpty(raw))
            raw = System.Guid.NewGuid().ToString();

        string hashed = Sha1(raw);

        PlayerPrefs.SetString(PrefKey, hashed);
        PlayerPrefs.Save();

        Debug.Log("[DeviceKeyService] Created new DeviceKey = " + hashed);
        return hashed;
    }

    private static string Sha1(string input)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha1.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }

    // Optional: if you ever want to reset device identity manually
    public static void ClearDeviceKey()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        PlayerPrefs.Save();
        Debug.Log("[DeviceKeyService] DeviceKey cleared.");
    }
}
