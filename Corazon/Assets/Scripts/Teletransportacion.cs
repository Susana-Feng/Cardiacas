using UnityEngine;

public class Teletransportacion : MonoBehaviour
{
    public Transform Target;
    public float offsetFrente = 1.5f;

    private void OnTriggerEnter(Collider other)
    {
        Vector3 spawnPos = Target.position;

        if (Physics.Raycast(Target.position, Vector3.down, out RaycastHit hit, 5f))
        {
            spawnPos = hit.point;
        }

        other.transform.position = spawnPos + Target.right * offsetFrente;
    }
}