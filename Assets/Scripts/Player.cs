using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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

   public int vida = 3;
   public TMP_Text textoVidas;
   public bool muerto = false;

    private float move;
 
    private Vector3 posicionInicial;

    private int comboStep = 0;

    public Transform attackPoint;

    public LayerMask enemyLayer;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        posicionInicial = transform.position;
        // Mostrar vida inicial
        textoVidas.text = vida.ToString();
    }

    void Update()
    {
     move = 0;

    // Movimiento izquierda/derecha
    if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        move = -1;

    if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        move = 1;

    // Girar personaje
    if (move != 0)
        transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);

    // Salto
    if ((Keyboard.current.spaceKey.wasPressedThisFrame) && isGrounted)
    {
        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumoForce);
    }

    if (Keyboard.current.jKey.wasPressedThisFrame && isGrounted)
    {
        comboStep++;

        if (comboStep == 1)
        {
            animator.SetTrigger("Attack1");
            Atacar();
        }
        else if (comboStep == 2)
        {
            animator.SetTrigger("Attack2");
            Atacar();
            comboStep = 0;
        }
    }

        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
        animator.SetBool("IsGrounded", isGrounted);
        animator.SetTrigger("Hit");
    }

    void Atacar()
    {
        Collider2D[] enemigos = Physics2D.OverlapCircleAll(
        attackPoint.position,
        0.5f,
        enemyLayer
    );

        foreach (Collider2D enemigo in enemigos)
    {
        enemigo.GetComponent<EnemyController>()?.RecibeDanio(1);
    }
    }

    void Respawn()
    {
        transform.position = posicionInicial;
        rb2D.linearVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        // Movimiento horizontal SOLO AQUÍ
        rb2D.linearVelocity = new Vector2(move * speed, rb2D.linearVelocity.y);

        // Comprobación suelo
        isGrounted = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // -------- MONEDAS --------
        if(collision.transform.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            coints++;
            textCoints.text = coints.ToString();
        }

        // -------- PINCHOS --------
        if(collision.transform.CompareTag("Spikes"))
        {
            RecibeDanio(1);
        }

         // -------- CAÍDA --------
        if(collision.transform.CompareTag("FallZone"))
        {
            RecibeDanio(1);
            Respawn();
        }

        // -------- BARRIL --------
        if(collision.transform.CompareTag("Barrel"))
        {
            Vector2 knockbackDir = (rb2D.position - (Vector2)collision.transform.position).normalized;

            rb2D.linearVelocity = Vector2.zero;
            rb2D.AddForce(knockbackDir * 3, ForceMode2D.Impulse);

            BoxCollider2D[] colliders = collision.gameObject.GetComponents<BoxCollider2D>();

            foreach(BoxCollider2D col in colliders)
            {
                col.enabled = false;
            }

            collision.GetComponent<Animator>().enabled = true;
            Destroy(collision.gameObject, 0.5f);

            RecibeDanio(1);
        }
    }

    // ---------------- RECIBIR DAÑO ----------------

    public void RecibeDanio(int danio)
    {
        if(muerto) return;

        vida -= danio;

        textoVidas.text = vida.ToString();

        if(vida <= 0)
        {
            Morir();
        }
    }

    // ---------------- GANAR VIDA ----------------

    public void GanarVida(int cantidad)
    {
        vida += cantidad;
        textoVidas.text = vida.ToString();
    }

    // ---------------- MORIR ----------------

    void Morir()
    {
        muerto = true;

        Debug.Log("Jugador muerto");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}