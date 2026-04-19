using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos que sea el jugador quien colisionó
        if (collision.CompareTag("Player"))
        {
            Debug.Log("¡Nivel Completado!");
            LevelManager.Instance.NextLevel();
        }
    }
}