using UnityEngine;

public class AutoDespawn : MonoBehaviour
{
    [Tooltip("Tiempo en minutos antes de que el objeto se elimine")]
    public float minutosParaDestruir = 1f;

    void Start()
    {
        // Convertimos minutos a segundos (Unity usa segundos para el delay)
        float segundos = minutosParaDestruir * 60f;

        // La función Destroy acepta un segundo parámetro opcional para el retraso
        Destroy(gameObject, segundos);
    }
}
