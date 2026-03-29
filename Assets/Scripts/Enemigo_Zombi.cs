using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 3f;
    public float speed = 2f;

    public int vida = 3;
    public GameObject prefabVidaExtra;

    public Transform puntoA;
    public Transform puntoB;
    public float tiempoEsperaPatrulla = 2f;

    private Transform objetivoActual;
    private Rigidbody2D rb;
    private Animator animator;

    private bool playerDetectado = false;
    private bool muerto = false;
    private bool golpeado = false;

    private bool isMovingRight = true;

    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        objetivoActual = puntoB;
        isMovingRight = (puntoB.position.x > transform.position.x);

        StartCoroutine(Patrullar());

        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (muerto) return;

        float distancia = Vector2.Distance(transform.position, player.position);
        float diferenciaY = Mathf.Abs(transform.position.y - player.position.y);

        if (distancia < detectionRadius && diferenciaY < 1f)
        {
            playerDetectado = true;
            PerseguirJugador();
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
                    animator.SetBool("enMovimiento", false);

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

                animator.SetBool("enMovimiento", true);

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

        animator.SetBool("enMovimiento", true);

        transform.localScale = new Vector3(isMovingRight ? -1 : 1, 1, 1);
    }

    // ---------------- RECIBIR DAÑO ----------------
    public void RecibeDanio(int danio)
    {
        if (muerto || golpeado) return;

        golpeado = true;

        vida -= danio;

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
        yield return new WaitForSeconds(0.2f);
        golpeado = false;
    }

    // ---------------- DAÑO AL PLAYER ----------------
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