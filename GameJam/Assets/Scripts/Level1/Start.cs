using UnityEngine;

public class GameStart : MonoBehaviour
{
    public PlayerMovement jumpForce;
    void Start()
    {
        jumpForce = FindFirstObjectByType<PlayerMovement>();
        jumpForce.jumpForce = 10f;
    }
}
