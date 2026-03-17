using UnityEngine;

// Script para el objeto que da vida al jugador
public class VidaExtra : MonoBehaviour
{
    // Cantidad de vida que se añadirá al jugador
    public int cantidadVida = 1;

    // Se ejecuta cuando algo entra en el trigger del objeto
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Mensaje para comprobar en consola qué objeto tocó la vida
        Debug.Log("VidaExtra: Trigger con " + other.name);

        // Comprobamos que el objeto que tocó sea el Player
        if (other.CompareTag("Player"))
        {
            // Buscamos el script PlayerController del jugador
            Player jugador = other.GetComponent<Player>();

            // Si el jugador tiene el script
            if (jugador != null)
            {
                // Le añadimos vida
                jugador.GanarVida(cantidadVida);

                Debug.Log("VidaExtra: Vida añadida! Destruyendo objeto...");

                // Destruimos la vida extra después de recogerla
                Destroy(gameObject);
            }
        }
    }
}