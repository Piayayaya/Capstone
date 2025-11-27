using UnityEngine;

public class DangerConfirmPanel : MonoBehaviour
{
    // Called by the YES button
    public void OnClickYes()
    {
        // TODO: here you will call:
        // - your Reset Progress code  (for the Reset panel)
        // - your Delete Account code (for the Delete panel)

        Debug.Log("[DangerConfirmPanel] YES clicked on " + gameObject.name);

        gameObject.SetActive(false);
    }

    // Called by the NO / CANCEL button
    public void OnClickNo()
    {
        Debug.Log("[DangerConfirmPanel] NO clicked on " + gameObject.name);
        gameObject.SetActive(false);
    }
}
