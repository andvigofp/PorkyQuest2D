using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float speed = 5;
    private Rigidbody2D rb2D;

    public float jumoForce = 4;
    private bool isGrounted;
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    private Animator animator;
    private int coints;
    public TMP_Text textCoints;


    private float move;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();    
    }

   
    void Update()
    {
        move = Input.GetAxisRaw("Horizontal");
        rb2D.linearVelocity = new Vector2(move*speed, rb2D.linearVelocity.y);

        if(move != 0)
            transform.localScale = new Vector3(Mathf.Sign(move),1, 1);

        if(Input.GetButtonDown("Jump") && isGrounted)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumoForce);
        }
        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
        animator.SetBool("IsGrounded", isGrounted);    
    }

    private void FixedUpdate()
    {
        isGrounted = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            coints++;
            textCoints.text = coints.ToString();
        }

        if(collision.transform.CompareTag("Spikes"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
