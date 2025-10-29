using UnityEngine;

public class DriftCamera : MonoBehaviour
{
    public Transform target;             // Car to follow
    public Vector3 offset = new Vector3(0, 5, -10);  // Camera offset
    public float smoothSpeed = 0.125f;   // Smoothing factor
    public float rotationSmoothSpeed = 5f; // Rotation smoothing

    private void LateUpdate()
    {
        if (!target) return;

        // Target position with offset
        Vector3 desiredPosition = target.TransformPoint(offset);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;

        // Smooth rotation
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}
