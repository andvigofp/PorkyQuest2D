using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 3f;
    public float speed = 2f;

    public int vida = 2;
    public GameObject prefabVidaExtra;

    public Transform puntoA;
    public Transform puntoB;
    public float tiempoEsperaPatrulla = 2f;

    private Transform objetivoActual;
    private Rigidbody2D rb;
    private Animator animator;

    private bool playerDetectado = false;
    private bool muerto = false;

    private bool isMovingRight = true; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        objetivoActual = puntoB;

        //dirección inicial
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
            //SOLO EN X (clave)
            float distancia = Mathf.Abs(transform.position.x - objetivoActual.position.x);

            if (distancia < 0.3f)
            {
                //FORZAR POSICIÓN EXACTA
                transform.position = new Vector3(
                    objetivoActual.position.x,
                    transform.position.y,
                    transform.position.z
                );

                //PARAR COMPLETAMENTE
                rb.linearVelocity = Vector2.zero;

                //IDLE
                animator.SetBool("enMovimiento", false);

                yield return new WaitForSeconds(tiempoEsperaPatrulla);

                //CAMBIO DE DIRECCIÓN REAL
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

                yield return null; // evita ejecución doble
            }

            //DIRECCIÓN SOLO HORIZONTAL
            float direccionX = Mathf.Sign(objetivoActual.position.x - transform.position.x);

            //MOVIMIENTO CONTROLADO
            rb.MovePosition(rb.position + new Vector2(direccionX, 0) * speed * Time.deltaTime);

            //ANIMACIÓN CORRER
            animator.SetBool("enMovimiento", true);

            //GIRO CORRECTO
            transform.localScale = new Vector3(isMovingRight ? -1 : 1, 1, 1);
        }

        yield return null;
        }
    }
    
    void PerseguirJugador()
    {
        Vector2 direccion = (player.position - transform.position).normalized;

        //calcular dirección REAL
        isMovingRight = direccion.x > 0;

        rb.MovePosition(rb.position + new Vector2(direccion.x, 0) * speed * Time.deltaTime);

        animator.SetBool("enMovimiento", true);

        //GIRO CORRECTO
        transform.localScale = new Vector3(isMovingRight ? -1 : 1, 1, 1);
    }

    public void RecibeDanio(int danio)
    {
        if (muerto) return;

        vida -= danio;

        if (vida <= 0)
        {
            muerto = true;

            animator.SetBool("muerto", true);

            rb.linearVelocity = Vector2.zero;

            StartCoroutine(EliminarDespues());
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
}