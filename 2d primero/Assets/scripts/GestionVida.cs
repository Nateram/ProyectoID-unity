using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Add for Any() extension method

public class GestionVida : MonoBehaviour
{
    public int indexActual;
    public List<Image> Listacorazones = new List<Image>();

    [SerializeField] private Sprite Completo;
    [SerializeField] private Sprite Vacio;    
    [SerializeField] private PlayerController playerController; // Change to PlayerController type
    [SerializeField] private GameObject Vidaprefab;

    void Awake()
    {
        playerController.cambioVida.AddListener(CambiarCorazones);

    }

    private void CambiarCorazones(int vidaActual)
    {

        if(!Listacorazones.Any()) // Correct method name
        {
            CrearCorazones(vidaActual);
        }
        else
        {

            CambiarVida(vidaActual);
        }
    }

    private void CrearCorazones(int vidaActual)
    {
        for(int i = 0; i < vidaActual; i++)
        {
            GameObject corazon = Instantiate(Vidaprefab, transform);
            Listacorazones.Add(corazon.GetComponent<Image>()); // Correct method name
        }
        indexActual = vidaActual-1;
    }

private void CambiarVida(int vidaActual)
{

    if(vidaActual <= indexActual)
    {
        // Si perdemos vida
        for(int i = indexActual; i >= vidaActual; i--)
        {
            if(i >= 0 && i < Listacorazones.Count)
            {
                Listacorazones[i].sprite = Vacio;
            }
        }
        indexActual = vidaActual - 1;
    }
    else
    {
        // Si recuperamos vida
        for(int i = indexActual + 1; i < vidaActual; i++)
        {
            if(i >= 0 && i < Listacorazones.Count)
            {
                Listacorazones[i].sprite = Completo;
            }
        }
        indexActual = vidaActual - 1;
    }


}
    private void QuitarCorazones(int vidaActual)
    {
        for(int i = indexActual; i >= vidaActual; i--)
        {
           indexActual=i;
           Listacorazones[indexActual].sprite = Vacio;
        }
    }
    private void AgregarCorazones(int vidaActual)
    {
        for(int i = indexActual; i < vidaActual; i++)
        {
            indexActual=i;
            Listacorazones[indexActual].sprite = Completo;
        }
    }
}