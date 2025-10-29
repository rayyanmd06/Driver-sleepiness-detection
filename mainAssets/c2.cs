using UnityEngine;

public class DriftCamera2 : MonoBehaviour
{
    public Transform target;                     // Car to follow
    public Vector3 offset = new Vector3(0, 5, -10);  // Camera offset
    public float positionSmoothSpeed = 5f;       // Smooth movement speed
    public float rotationSmoothSpeed = 3f;       // Smooth rotation speed
    public float driftTiltMultiplier = 2f;       // Camera tilt when car drifts

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (!target) return;

        // Smooth follow
        Vector3 targetPos = target.TransformPoint(offset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 1f / positionSmoothSpeed);

        // Look ahead rotation + drift tilt
        Vector3 lookDirection = target.position - transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);

        // Simulate camera tilt based on car's local X velocity (sideways movement)
        float driftTilt = target.InverseTransformDirection(target.GetComponent<Rigidbody>().linearVelocity).x;
        Quaternion tiltRotation = Quaternion.AngleAxis(driftTilt * driftTiltMultiplier, Vector3.forward);

        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation * tiltRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}
