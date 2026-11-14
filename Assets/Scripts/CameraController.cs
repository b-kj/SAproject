using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followRate = 10f;

    [Header("Camera Limit")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 20f;

    [Header("Camera Offset")]
    [SerializeField] private float cameraOffsetY = 3f;

    void LateUpdate()
    {
        if (player == null) return;

        float targetX = Mathf.Clamp(player.position.x, minX, maxX);
        float targetY = Mathf.Clamp(player.position.y + cameraOffsetY, minY, maxY);

        Vector3 targetPosition = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followRate * Time.deltaTime);
    }
}