using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrailController : MonoBehaviour
{
    TrailRenderer trailSideA;
    TrailRenderer trailSideB;
    const int MAX_POSITIONS = 500;
    Vector3[] TrailRecorded = new Vector3[MAX_POSITIONS];
    public GameObject levelSideB;

    void Awake()
    {
        trailSideA = GetComponent<TrailRenderer>();
        trailSideB = levelSideB.GetComponent<TrailRenderer>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (InputSystem.actions.FindAction("Interact").IsPressed())
        {
            SaveTrail();
        }
    }

    void SaveTrail ()
    {
        int positions = trailSideA.GetPositions(TrailRecorded);
        trailSideB.AddPositions(TrailRecorded);
    }
}
