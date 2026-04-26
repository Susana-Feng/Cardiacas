using UnityEngine;

public class ReleaseOnGrab : MonoBehaviour
{
    private Rigidbody rb;
    private bool alreadyReleased = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // al inicio está fijo contra la pared
    }

    // Este método lo conectas al evento Select Entered
    public void Release()
    {
        if (!alreadyReleased)
        {
            rb.isKinematic = false; // se libera
            alreadyReleased = true; // nunca vuelve a fijarse
        }
    }
}

