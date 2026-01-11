using UnityEngine;

public class PlayerControllerSingle : MonoBehaviour
{
    //코드 담당자: 최은주 
    //싱글 플레이 컨트롤러
    private Animator animator;

    [Header("Movement Speeds")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5.0f;
    public float crouchSpeed = 1.5f;
    public float injurySpeedMultiplier = 0.5f;

    [Header("Look Settings")]
    public Camera playerCamera;
    public float lookSensitivity = 30f;
    public float minYAngle = -75f;
    public float maxYAngle = 45f;

    [Header("Crouch Settings")]
    public Vector3 standingCameraPosition = new Vector3(0, 1.65f, 0.03f);
    public Vector3 crouchingCameraPosition = new Vector3(0, 1.25f, 0.03f);
    public float crouchTransitionSpeed = 10f;

    private void Awake()
    {    
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(h, 0, v).normalized;

        transform.position += dir * walkSpeed * Time.deltaTime;
    }
}
