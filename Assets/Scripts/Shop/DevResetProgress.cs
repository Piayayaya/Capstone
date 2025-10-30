using UnityEngine;

public class DevResetProgress : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))   // press R to reset
        {
            ShopSave.ResetAll();
            // refresh visible UI
            foreach (var b in FindObjectsOfType<ShopCardBinder>()) b.Refresh();
            var hud = FindObjectOfType<CoinHudBinder>(); if (hud) hud.Refresh();
            Debug.Log("Progress reset.");
        }
    }
}
