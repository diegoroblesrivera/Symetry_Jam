using UnityEngine;

public class MiddleLine : MonoBehaviour
{
    public GameObject Sonidi;
    public GameObject ContinueLevelSpawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // 🔊 SONIDO
        if (Sonidi != null)
        {
            var instancia = Instantiate(Sonidi, transform.position, Quaternion.identity);
            Destroy(instancia, 5f);
        }
        else
        {
            Debug.LogWarning("Sonidi no asignado");
        }

        // 🧠 PLAYER (más seguro)
        PlayerController2D player = collision.GetComponentInParent<PlayerController2D>();

        if (player != null)
        {
            player.SpawnVisualBurst();
            player.BloquearColocacionBloques();
        }
        else
        {
            Debug.LogError("PlayerController2D no encontrado en el Player");
        }

        // 📍 TELEPORT (seguro)
        if (ContinueLevelSpawnPoint != null)
        {
            collision.transform.position = ContinueLevelSpawnPoint.transform.position;

            if (collision.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
            }

            ContinueLevelSpawnPoint.SetActive(false);
        }
        else
        {
            Debug.LogError("ContinueLevelSpawnPoint NO asignado");
        }

        // 🚫 DESACTIVAR
        gameObject.SetActive(false);
    }
}