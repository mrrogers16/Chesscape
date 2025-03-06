using System.Collections;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor.UI;
using TMPro;

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

    public LayerMask wallLayer;

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
        if (isMoving || gridDirection == Vector3.zero)
        {
            return;
        }

        if (IsWallOrDoorBlocking(gridDirection))
        {
            Debug.Log("Wall detected. Movement blocked");
            return;
        }

        Vector3 targetPosition = transform.position + (gridDirection.normalized * moveStep);

        Quaternion targetRotation = Quaternion.LookRotation(gridDirection);
        StartCoroutine(MoveToPosition(targetPosition, targetRotation));
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

        if (other.CompareTag("Exit"))
        {
            WinGame();
        }

    }

    public void WinGame()
    {
        Debug.Log("Exit found");
        SceneManager.LoadScene("WinScreen");
    }


    public void ActivateTrap()
    {
        Debug.Log("Trap Activated");
        SceneManager.LoadScene("DeathScreen");
    }

    public void PickupKey(GameObject key)
    {
        Debug.Log("PickupKey Called");


        key.transform.SetParent(keyHoldPoint, worldPositionStays: true);
        // key.transform.position = keyHoldPoint.position;
        // key.transform.rotation = keyHoldPoint.rotation;

        Collider keyCollider = key.GetComponent<Collider>();
        if (keyCollider != null)
        {
            keyCollider.enabled = false; // So it won't keep firing triggers or block movement
        }

        hasKey = true;
    }

    private bool IsWallOrDoorBlocking(Vector3 direction)
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        float rayLength = moveStep * 1.2f;

        // Cast in front of the player
        if (Physics.BoxCast(rayStart, new Vector3(0.4f, 0.5f, 0.4f), direction, out hit, Quaternion.identity, rayLength, wallLayer))
        {
            // If it's a door, see if we have a key
            if (hit.collider.CompareTag("Door"))
            {
                if (hasKey)
                {
                    Debug.Log("Door detected. We have a key. Destroy door.");
                    Destroy(hit.collider.gameObject);

                    if (keyHoldPoint.childCount > 0)
                    {
                        Destroy(keyHoldPoint.GetChild(0).gameObject);
                    }
                    hasKey = false;

                    // Because we destroyed the door, we can pass, so return false
                    return false;
                }
                else
                {
                    Debug.Log("Door detected. We do NOT have a key. Movement blocked.");
                    return true;
                }
            }
            else
            {
                Debug.Log("Wall detected. Movement blocked.");
                return true;
            }
        }
        // No wall or door hit, so we can move
        return false;
    }
}
