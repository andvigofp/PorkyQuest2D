using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    public Image rellenoBarraVida;
    private Player playerController;
    private float vidaMaxima;
    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<Player>();
        vidaMaxima = playerController.vida;
    }

    // Update is called once per frame
    void Update()
    {
        rellenoBarraVida.fillAmount = playerController.vida / vidaMaxima;
    }
}
