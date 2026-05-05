using UnityEngine;

public class BloqueItem : MonoBehaviour
{
    public PlayerAbilityType habilidadOtorgada; // Selecciona la habilidad en el Inspector
    public int cantidadExtra = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.OtorgarHabilidad(habilidadOtorgada, cantidadExtra);
            }
            Destroy(gameObject);
        }
    }
}