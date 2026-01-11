using UnityEngine;

public class ItemD : MonoBehaviour
{
    [Header("호버링 설정")]
    [Tooltip("초당 얼마나 빠르게 위아래로 움직일지")]
    public float hoverSpeed = 1.0f;
    [Tooltip("얼마나 높이 떠서 움직일지")]
    public float hoverHeight = 0.02f;

    [Header("회전 설정")]
    [Tooltip("초당 회전 속도")]
    public float rotationSpeed = 50.0f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}