using UnityEngine;
using System.Collections;

public class EnemyPigController : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 3f;
    public float attackDistance = 1f;

    public float speed = 2f;

    public int vida = 3;
    public GameObject prefabVidaExtra;

    public Transform puntoA;
    public Transform puntoB;
    public float tiempoEsperaPatrulla = 2f;

    public float attackCooldown = 1.5f;

    public LayerMask playerLayer;

    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public float fuerzaEmpuje = 3f;

    private Transform objetivoActual;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private bool playerDetectado = false;
    private bool muerto = false;
    private bool golpeado = false;

    private bool isMovingRight = true;
    private float tiempoUltimoAtaque;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        objetivoActual = puntoB;
        isMovingRight = (puntoB.position.x > transform.position.x);

        StartCoroutine(Patrullar());
    }

    void Update()
    {
        if (muerto || golpeado) return;

        float distancia = Vector2.Distance(transform.position, player.position);
        float diferenciaY = Mathf.Abs(transform.position.y - player.position.y);

        if (distancia < detectionRadius && diferenciaY < 1f)
        {
            playerDetectado = true;

            if (distancia <= attackDistance)
            {
                Atacar();
            }
            else
            {
                PerseguirJugador();
            }
        }
        else
        {
            playerDetectado = false;
        }

        //ANIMACIONES
        animator.SetFloat("VelocidadHorizontal", Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat("VelocidadVertical", rb.linearVelocity.y);
        animator.SetBool("EnSuelo", true);
    }
    
    //Patrullar hasta Posicon A Hasta B
    IEnumerator Patrullar()
    {
        while (!muerto)
        {
            if (!playerDetectado)
            {
                float distancia = Mathf.Abs(transform.position.x - objetivoActual.position.x);

                if (distancia < 0.2f)
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                    yield return new WaitForSeconds(tiempoEsperaPatrulla);

                    if (objetivoActual == puntoA)
                    {
                        objetivoActual = puntoB;
                        isMovingRight = true;
                    }
                    else
                    {
                        objetivoActual = puntoA;
                        isMovingRight = false;
                    }
                }

                float direccionX = Mathf.Sign(objetivoActual.position.x - transform.position.x);

                rb.linearVelocity = new Vector2(direccionX * speed, rb.linearVelocity.y);

                transform.localScale = new Vector3(isMovingRight ? -1 : 1, 1, 1);
            }

            yield return null;
        }
    }

    //Persigue al Jugador si esta ceraca de su radio
    void PerseguirJugador()
    {
        float direccionX = Mathf.Sign(player.position.x - transform.position.x);

        isMovingRight = direccionX > 0;

        rb.linearVelocity = new Vector2(direccionX * speed, rb.linearVelocity.y);

        transform.localScale = new Vector3(isMovingRight ? -1 : 1, 1, 1);
    }

    //Ataca al enemigo si esta cerca de su radio, salta la aniamcion
    void Atacar()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (Time.time >= tiempoUltimoAtaque + attackCooldown)
        {
            tiempoUltimoAtaque = Time.time;

            animator.SetTrigger("Atacar");
        }
    }

    //LLAMADO DESDE ANIMATION EVENT
    public void HacerDanio()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRadius,
            playerLayer
        );

        if (hit != null)
        {
            Player playerScript = hit.GetComponent<Player>();

            if (playerScript != null)
            {
                playerScript.RecibeDanio(1);

                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();

                if (playerRb != null)
                {
                    Vector2 direccion = (hit.transform.position - transform.position).normalized;

                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.AddForce(direccion * fuerzaEmpuje, ForceMode2D.Impulse);
                }
            }
        }
    }

    //Si Recibe Daño salta la animacion de daño
    // ---------------- RECIBIR DAÑO ----------------
    public void RecibeDanio(int danio)
    {
        if (muerto || golpeado) return;

        golpeado = true;

        vida -= danio;

        animator.SetTrigger("Golpe");

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(-transform.localScale.x * 2f, 1f), ForceMode2D.Impulse);

        StartCoroutine(FlashBlanco());

        if (vida <= 0)
        {
            Morir();
        }
        else
        {
            StartCoroutine(ResetGolpe());
        }
    }

    //Si muere el enemigo salta la animacion de muerte
    void Morir()
    {
        muerto = true;

        animator.SetBool("Ocupado", true);

        rb.linearVelocity = Vector2.zero;

        StartCoroutine(EliminarDespues());
    }

    //Si recibe el golpe el enemigo emula la animacion de recibir golpe a color rojo
    IEnumerator FlashBlanco()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    IEnumerator ResetGolpe()
    {
        yield return new WaitForSeconds(0.3f);
        golpeado = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.GetComponent<Player>()?.RecibeDanio(1);
        }
    }

    IEnumerator EliminarDespues()
    {
        yield return new WaitForSeconds(1f);

        if (Random.value < 0.2f && prefabVidaExtra != null)
        {
            Instantiate(prefabVidaExtra, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public void AtaqueTermino()
    {
        // opcional (para evitar error de AnimationEvent)
    }

    void OnDrawGizmos()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}