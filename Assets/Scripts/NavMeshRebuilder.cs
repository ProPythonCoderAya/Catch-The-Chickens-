using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshRebuilder : MonoBehaviour
{
    private NavMeshSurface _surface;

    private void Awake()
    {
        LateStart.AddLate(BuildLate);
    }

    void Start()
    {
        _surface = GetComponent<NavMeshSurface>();
    }

    void BuildLate()
    {
        _surface.BuildNavMesh();
    }
}
