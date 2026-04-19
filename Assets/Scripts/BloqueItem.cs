using UnityEngine;

public class BloqueItem : MonoBehaviour
{
    public int cantidadExtra = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerController>();
            player.AumentarLimiteBloques(cantidadExtra);
            Destroy(gameObject);
        }
    }
}