using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public GameObject playerObject;
    public float gridSize = 1f;

    void Start()
    {
        Cursor.visible = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        Player player = playerObject.GetComponent<Player>();


        if (Input.GetKey(KeyCode.W))
        {
            player.Move(new Vector3(0f, 0f, gridSize));
        }
        if (Input.GetKey(KeyCode.S))
        {
            player.Move(new Vector3(0f, 0f, -gridSize));
        }
        if (Input.GetKey(KeyCode.A))
        {
            player.Move(new Vector3(-gridSize, 0f, 0f));
        }
        if (Input.GetKey(KeyCode.D))
        {
            player.Move(new Vector3(gridSize, 0f, 0f));
        }

    }
}
