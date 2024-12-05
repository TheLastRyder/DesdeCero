using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PratonSimple : MonoBehaviour
{
    [SerializeField] private float velocidad;
    [SerializeField] private Transform[] puntos;

    private int puntoActual;
    void Update()
    {
        if (transform.position != puntos[puntoActual].position)
        {
            transform.position = Vector2.MoveTowards(transform.position, puntos[puntoActual].position, velocidad * Time.deltaTime);
        }
        else
        {
            //puntoActual++;
        }
    }
}
