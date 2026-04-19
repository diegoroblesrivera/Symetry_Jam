using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrailController : MonoBehaviour
{
    TrailRenderer trailSideA;
    const int MAX_POSITIONS = 500;
    Vector3[] TrailRecorded = new Vector3[MAX_POSITIONS];
    public GameObject levelSideB;
    Mesh mesh;

    void Awake()
    {
        trailSideA = GetComponent<TrailRenderer>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = new Mesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (InputSystem.actions.FindAction("Interact").IsPressed()) // CAMBIAR ESTO A CUANDO TERMINA EL LADO A
        {
            SaveTrail();
        }
    }

    void SaveTrail ()
    {
        trailSideA.BakeMesh(mesh,true);
        
        MeshFilter meshFilter = levelSideB.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }
}
