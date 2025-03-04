using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveStep = 10f;         // How far each grid step goes
    public float moveDuration = 55f;   // How long to move one step
    public float rotationSpeed = 5f;    // How quickly to rotate toward movement direction

    private Rigidbody rb;
    private bool isMoving = false;

    [Header("Key Info")]
    public Transform keyHoldPoint;
    public bool hasKey = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        
    }

    public void Move(Vector3 gridDirection)
    {
        if (!isMoving && gridDirection != Vector3.zero)
        {
            // Figure out the grid's target position
            Vector3 targetPosition = transform.position + (gridDirection.normalized * moveStep);

            // Rotate the player toward movement direction
            Quaternion targetRotation = Quaternion.LookRotation(gridDirection);
            StartCoroutine(MoveToPosition(targetPosition, targetRotation));
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPos, Quaternion targetRot)
    {
        isMoving = true;

        Vector3 startPos = rb.position;          // current position
        Quaternion startRot = rb.rotation;       // current rotation
        float elapsed = 0f;


        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float timeMove = 0f;
            // Lerp toward position
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, timeMove);
            rb.MovePosition(newPos);

            // Slerp toward rotation
            Quaternion newRot = Quaternion.Slerp(startRot, targetRot, timeMove);
            rb.MoveRotation(newRot);

            yield return null;
        }

        // Snap to final
        rb.MovePosition(targetPos);
        rb.MoveRotation(targetRot);

        isMoving = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key"))
        {
            PickupKey(other.gameObject);
        }

        if (other.CompareTag("Trap"))
        {
            ActivateTrap();
        }
    }

    public void ActivateTrap()
    {
        Debug.Log("Trap Activated");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PickupKey(GameObject key)
    {
        Debug.Log("PickupKey Called!");


        key.transform.SetParent(keyHoldPoint, worldPositionStays: true);
        key.transform.position = keyHoldPoint.position;
        key.transform.rotation = keyHoldPoint.rotation;

        Collider keyCollider = key.GetComponent<Collider>();
        if (keyCollider != null)
        {
            keyCollider.enabled = false; // So it won't keep firing triggers or block movement
        }

        hasKey = true;
    }
}
