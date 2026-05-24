using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DisappearOnDrop : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnDrop);
    }

    private void OnDrop(SelectExitEventArgs args)
    {
        gameObject.SetActive(false); // or use Destroy(gameObject);
    }

    private void OnDestroy()
    {
        grabInteractable.selectExited.RemoveListener(OnDrop);
    }
}