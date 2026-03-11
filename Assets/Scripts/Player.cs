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

 //Para que arranque los componentes de rigiBody 2d y la animacion
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();    
    }

   //Para que el personaje se pueda mover, las animaciones del perosnaje
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

    //Para saber si el personaje toca el suele se active el check
    private void FixedUpdate()
    {
        isGrounted = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    //Cuando el personaje recoge la moneda se destruye y se va sumando el texto
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            coints++;
            textCoints.text = coints.ToString();
        }

        //Al pinchar el personaje en los pinchos o se cae por un barranco se reincia el nivel
        if(collision.transform.CompareTag("Spikes"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if(collision.transform.CompareTag("Barrel"))
        {   //Para saber en que direccion queremos mandar al personaje    
            Vector2 knockbackDir = (rb2D.position-(Vector2)collision.transform.position).normalized;
        }

        if(collision.transform.CompareTag("Barrel"))
        {   //Para saber en que direccion queremos mandar al personaje    
            Vector2 knockbackDir = (rb2D.position-(Vector2)collision.transform.position).normalized;
            //Para reinciar un poco la velocidad
            rb2D.linearVelocity = Vector2.zero;
            rb2D.AddForce(knockbackDir*3, ForceMode2D.Impulse);

            //Recorremos todos los boxCollider del barril 
            BoxCollider2D[] colliders = collision.gameObject.GetComponents<BoxCollider2D>();

            foreach(BoxCollider2D col in colliders)
            {   
                col.enabled = false;
            }

            collision.GetComponent<Animator>().enabled=true;
            Destroy(collision.gameObject, 0.5f);
        }
}
}

