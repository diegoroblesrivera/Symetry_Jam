using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Niveles")]
    public List<GameObject> levelPrefabs;

    private int currentLevelIndex = 0;
    private GameObject currentLevelInstance;

    public GameObject canva1;
    public GameObject canva2;  
    public GameObject canva3;

    private void Awake()
    {
        // Singleton sencillo
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return; // Importante: salir para evitar que Start() se ejecute en el duplicado
        }
    }

    private void Start()
    {
        // Carga automática del primer nivel (índice 0) al iniciar
        //LoadLevel(0);
    }

    public void LoadLevel(int index)
    {
        // Validar que el nivel exista
        if (index < 0 || index >= levelPrefabs.Count)
        {
            Debug.Log("¡Fin del juego! No hay más niveles.");
            canva1.SetActive(true);
            canva2.SetActive(false);
            canva3.SetActive(true);
            Destroy(currentLevelInstance);
            return;

        }

        // 1. Limpiar el nivel anterior (borra al jugador anterior, el mapa, etc.)
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        // 2. Instanciar el nuevo nivel completo
        currentLevelIndex = index;
        currentLevelInstance = Instantiate(levelPrefabs[currentLevelIndex], Vector3.zero, Quaternion.identity);

        Debug.Log($"Cargado nivel {currentLevelIndex}: {levelPrefabs[currentLevelIndex].name}");
    }

    // Función para llamar desde la colisión de victoria
    public void NextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void CerrarJuego()
    {
          Application.Quit();
    }
}