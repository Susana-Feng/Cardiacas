using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attach to every bad piece. No other components needed.
/// When dropped, notifies BadPieceManager directly and destroys itself.
/// </summary>
public class DesaparecerObjeto : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnDrop);
    }

    private void OnDrop(SelectExitEventArgs args)
    {
        grabInteractable.selectExited.RemoveListener(OnDrop);

        if (BadPieceManager.Instance != null)
            BadPieceManager.Instance.OnBadPieceRemoved(gameObject);

        Destroy(gameObject);
    }
}