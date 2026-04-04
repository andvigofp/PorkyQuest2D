using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private Animator animator;

    //Referencia al movimiento
    private Player_Mov playerMov;

    public float jumoForce = 4;
    private bool isGrounted;
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    private int coints;
    public TMP_Text textCoints;

    public int vida = 3;
    public TMP_Text textoVidas;
    public bool muerto = false;

    private Vector3 posicionInicial;

    private int comboStep = 0;

    public Transform attackPoint;
    public LayerMask enemyLayer;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerMov = GetComponent<Player_Mov>();

        posicionInicial = transform.position;
        textoVidas.text = vida.ToString();
    }

    void Update()
    {
        //Detectar suelo
        isGrounted = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );

        //Movimient
        float move = Mathf.Abs(playerMov.GetMove());

        //Animaciones
        animator.SetFloat("Speed", move);
        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
        animator.SetBool("IsGrounded", isGrounted);
    }

    // ---------------- SALTO (INPUT SYSTEM) ----------------

   public void OnJump()
    {
        if (isGrounted)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumoForce);
        }
    }

    // ---------------- ATAQUE ----------------

   public void OnAttack()
    {
        if (!isGrounted) return;

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
            enemigo.GetComponent<EnemyPigController>()?.RecibeDanio(1);
        }
    }

    // ---------------- RESPAWN ----------------

    void Respawn()
    {
        transform.position = posicionInicial;
        rb2D.linearVelocity = Vector2.zero;
    }

    // ---------------- TRIGGERS ----------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            coints++;
            textCoints.text = coints.ToString();
        }

        if (collision.CompareTag("Spikes"))
        {
            RecibeDanio(1);
        }

        if (collision.CompareTag("FallZone"))
        {
            RecibeDanio(1);
            Respawn();
        }

        if (collision.CompareTag("Barrel"))
        {
            Vector2 knockbackDir = (rb2D.position - (Vector2)collision.transform.position).normalized;

            rb2D.linearVelocity = Vector2.zero;
            rb2D.AddForce(knockbackDir * 3, ForceMode2D.Impulse);

            foreach (var col in collision.GetComponents<BoxCollider2D>())
                col.enabled = false;

            collision.GetComponent<Animator>().enabled = true;
            Destroy(collision.gameObject, 0.5f);
        }
    }

    // ---------------- DAÑO ----------------

    public void RecibeDanio(int danio)
    {
        if (muerto) return;

        vida -= danio;
        animator.SetTrigger("Hit");

        textoVidas.text = vida.ToString();

        if (vida <= 0)
            Morir();
    }

    public void GanarVida(int cantidad)
    {
        vida += cantidad;
        textoVidas.text = vida.ToString();
    }

    void Morir()
    {
        muerto = true;
        Debug.Log("Jugador muerto");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}