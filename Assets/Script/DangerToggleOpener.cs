using UnityEngine;
using UnityEngine.UI;

public class DangerToggleOpener : MonoBehaviour
{
    public enum DangerType
    {
        ResetProgress,
        DeleteAccount
    }

    [Header("Refs")]
    public Toggle toggle;

    [Header("Type")]
    public DangerType dangerType = DangerType.ResetProgress;

    // used so that when we set isOn = false in code it doesn't reopen the panel
    bool _suppress;

    private void Awake()
    {
        if (!toggle)
            toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        if (toggle != null)
            toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDisable()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (_suppress) return;          // ignore programmatic changes

        if (!isOn) return;              // only react when turned ON

        if (DangerActionsManager.Instance == null)
        {
            Debug.LogWarning("[DangerToggleOpener] No DangerActionsManager in scene.");
            return;
        }

        switch (dangerType)
        {
            case DangerType.ResetProgress:
                DangerActionsManager.Instance.OpenReset(this);
                break;

            case DangerType.DeleteAccount:
                DangerActionsManager.Instance.OpenDelete(this);
                break;
        }
    }

    public void ForceOff()
    {
        if (toggle == null) return;

        _suppress = true;
        toggle.isOn = false;   // AnchorHandleToggle will move the handle left
        _suppress = false;
    }
}
