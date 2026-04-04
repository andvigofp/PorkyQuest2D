using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Mov : MonoBehaviour
{
    private Rigidbody2D rb;
    private Transform playerTransform;

    [SerializeField] float velocityx = 5f;

    public enum PlayerDir
    {
        Right,
        Left
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = transform;
    }

    void Update()
    {
        float move = 0;

        // 👉 ESTADO REAL DEL TECLADO (NO EVENTOS)
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            move = 1;

        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            move = -1;

        rb.linearVelocity = new Vector2(move * velocityx, rb.linearVelocity.y);

        // Girar
        if (move > 0) FlipDir(PlayerDir.Right);
        else if (move < 0) FlipDir(PlayerDir.Left);
    }

    private void FlipDir(PlayerDir direction)
    {
        float dirMultiplier = (direction == PlayerDir.Left) ? -1f : 1f;

        playerTransform.localScale = new Vector3(
            Mathf.Abs(playerTransform.localScale.x) * dirMultiplier,
            playerTransform.localScale.y,
            playerTransform.localScale.z
        );
    }

    public float GetMove()
    {
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            return 1;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            return -1;

        return 0;
    }
}