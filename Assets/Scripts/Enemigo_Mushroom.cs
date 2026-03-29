using UnityEngine;
using System.Collections;

public class EnemyMushroomController : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 3f;
    public float attackDistance = 1f;
    public float speed = 3f;

    public int vida = 3;
    public GameObject prefabVidaExtra;

    public Transform puntoA;
    public Transform puntoB;
    public float tiempoEsperaPatrulla = 2f;

    public float attackCooldown = 1.5f;

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
        if (muerto) return;

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
    }

    IEnumerator Patrullar()
    {
        while (!muerto)
        {
            if (!playerDetectado)
            {
                float distancia = Mathf.Abs(transform.position.x - objetivoActual.position.x);

                if (distancia < 0.3f)
                {
                    transform.position = new Vector3(
                        objetivoActual.position.x,
                        transform.position.y,
                        transform.position.z
                    );

                    rb.linearVelocity = Vector2.zero;
                    animator.SetFloat("Speed", 0);

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

                    yield return null;
                }

                float direccionX = Mathf.Sign(objetivoActual.position.x - transform.position.x);

                rb.MovePosition(rb.position + new Vector2(direccionX, 0) * speed * Time.deltaTime);

                animator.SetFloat("Speed", Mathf.Abs(direccionX * speed));

                transform.localScale = new Vector3(isMovingRight ? -1 : 1, 1, 1);
            }

            yield return null;
        }
    }

    void PerseguirJugador()
    {
        Vector2 direccion = (player.position - transform.position).normalized;

        isMovingRight = direccion.x > 0;

        rb.MovePosition(rb.position + new Vector2(direccion.x, 0) * speed * Time.deltaTime);

        animator.SetFloat("Speed", Mathf.Abs(direccion.x * speed));

        transform.localScale = new Vector3(isMovingRight ? -1 : 1, 1, 1);
    }

    void Atacar()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0);

        if (Time.time >= tiempoUltimoAtaque + attackCooldown)
        {
            tiempoUltimoAtaque = Time.time;
            animator.SetTrigger("attack");
        }
    }

    //LLAMADO DESDE ANIMATION EVENT
    public void HacerDanio()
    {
        if (player == null) return;

        float distancia = Vector2.Distance(transform.position, player.position);

        if (distancia <= attackDistance + 0.3f)
        {
            player.GetComponent<Player>()?.RecibeDanio(1);
        }
    }

    // ---------------- RECIBIR DAÑO ----------------
    public void RecibeDanio(int danio)
    {
        if (muerto || golpeado) return;

        golpeado = true;

        vida -= danio;

        animator.SetTrigger("hit");

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(-transform.localScale.x * 2f, 1f), ForceMode2D.Impulse);

        StartCoroutine(FlashBlanco());

        if (vida <= 0)
        {
            muerto = true;

            animator.SetBool("muerto", true);
            rb.linearVelocity = Vector2.zero;

            StartCoroutine(EliminarDespues());
        }
        else
        {
            StartCoroutine(ResetGolpe());
        }
    }

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

    // ---------------- DAÑO AL PLAYER (COLISIÓN) ----------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.GetComponent<Player>()?.RecibeDanio(1);
        }
    }

    // ---------------- MUERTE ----------------
    IEnumerator EliminarDespues()
    {
        yield return new WaitForSeconds(1f);

        if (Random.value < 0.2f && prefabVidaExtra != null)
        {
            Instantiate(prefabVidaExtra, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public void EliminarCuerpo()
    {
        Destroy(gameObject);
    }
}