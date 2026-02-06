using UnityEngine;

public class CameraEdgeColliders : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [Header("Edge Colliders")]
    [SerializeField] private BoxCollider2D topCollider;
    [SerializeField] private BoxCollider2D bottomCollider;
    [SerializeField] private BoxCollider2D leftCollider;
    [SerializeField] private BoxCollider2D rightCollider;

    [Header("Settings")]
    [SerializeField] private float topOffset = 1f;     
    [SerializeField] private float bottomOffset = 0.5f; 
    [SerializeField] private float colliderThickness = 1f;

    private void Start()
    {
        SetupColliders();
    }

    private void SetupColliders()
    {
        float camHeight = mainCamera.orthographicSize * 2f;
        float camWidth = camHeight * mainCamera.aspect;

        Vector3 camPos = mainCamera.transform.position;

        // TOP (slightly down)
        topCollider.size = new Vector2(camWidth, colliderThickness);
        topCollider.transform.position =
            camPos + Vector3.up * (camHeight / 2f - topOffset);

        // BOTTOM (slightly up)
        bottomCollider.size = new Vector2(camWidth, colliderThickness);
        bottomCollider.transform.position =
            camPos + Vector3.down * (camHeight / 2f - bottomOffset);

        // LEFT
        leftCollider.size = new Vector2(colliderThickness, camHeight);
        leftCollider.transform.position =
            camPos + Vector3.left * (camWidth / 2f);

        // RIGHT
        rightCollider.size = new Vector2(colliderThickness, camHeight);
        rightCollider.transform.position =
            camPos + Vector3.right * (camWidth / 2f);
    }
}
