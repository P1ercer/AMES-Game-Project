using UnityEngine;

public class FixRotation : MonoBehaviour
{
    public Vector3 desiredRotation;

    void Start()
    {
        transform.rotation = Quaternion.Euler(desiredRotation);
    }
}