using UnityEngine;

public class MiddleLine : MonoBehaviour
{
    public GameObject Sonidi;
    public GameObject ContinueLevelSpawnPoint; // Referencia al siguiente trigger de nivel
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos que sea el jugador quien colision�
        if (collision.CompareTag("Player"))
        {
            Instantiate(Sonidi, transform.position, Quaternion.identity, null);
            Destroy(Sonidi, 5f);
            var player = collision.GetComponent<PlayerController2D>();
            player.SpawnVisualBurst();
            player.BloquearColocacionBloques(); // Bloquea la colocaci�n de bloques

            collision.transform.position = ContinueLevelSpawnPoint.transform.position; // Teletransportamos al jugador al siguiente trigger
            if (collision.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
                // En versiones antiguas usa: rb.velocity = Vector2.zero;
            }
            ContinueLevelSpawnPoint.SetActive(false); // Desactivamos el siguiente trigger para evitar que se active nuevamente 
            this.gameObject.SetActive(false); // Desactivamos el trigger actual para evitar que se active nuevamente
            
        }
    }
}
