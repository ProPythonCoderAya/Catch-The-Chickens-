using UnityEngine;
using System.Collections.Generic;

public class MountainInstancer : MonoBehaviour
{
    [Header("Source Mesh (from OBJ)")]
    public Mesh mountainMesh;

    [Header("Materials (must match submeshes order)")]
    public Material[] mountainMaterials;

    [Header("Wall Settings")]
    public int instanceCount = 200;
    public float radius = 500f;

    [Header("Scale")]
    public float minScale = 1f;
    public float maxScale = 2f;

    [Header("Wall Options")]
    public bool randomRotationY = true;

    [Header("Colliders")]
    public bool generateColliders = true;
    public float colliderSizeMultiplier = 1f;

    private readonly List<Matrix4x4[]> _batches = new();
    private const int BatchSize = 1023;

    private Transform _colliderRoot;

    void Start()
    {
        _colliderRoot = new GameObject("MountainColliders").transform;
        _colliderRoot.SetParent(transform);

        Matrix4x4[] allMatrices = new Matrix4x4[instanceCount];

        Vector3 center = transform.position;

        for (int i = 0; i < instanceCount; i++)
        {
            float angle = (i / (float)instanceCount) * Mathf.PI * 2f;

            Vector3 pos = new Vector3(
                Mathf.Cos(angle),
                0f,
                Mathf.Sin(angle)
            ) * radius + center;

            Vector3 dirToCenter = (center - pos).normalized;

            Quaternion rot = Quaternion.LookRotation(dirToCenter, Vector3.up);

            if (randomRotationY)
            {
                rot *= Quaternion.Euler(0f, Random.Range(-10f, 10f), 0f);
            }

            float scale = Random.Range(minScale, maxScale);

            allMatrices[i] = Matrix4x4.TRS(
                pos,
                rot,
                Vector3.one * scale
            );

            // -----------------------------
            // COLLIDER CREATION (NO MESH)
            // -----------------------------
            if (generateColliders)
            {
                GameObject col = new GameObject($"MountainCollider_{i}");
                col.transform.SetParent(_colliderRoot);
                col.transform.position = pos;
                col.transform.rotation = rot;
                col.transform.localScale = Vector3.one * scale;

                BoxCollider box = col.AddComponent<BoxCollider>();

                // Approximate mountain thickness/height
                box.size = mountainMesh.bounds.size * colliderSizeMultiplier;

                // Optional: mark as static for physics optimization
                col.isStatic = true;
            }
        }

        for (int i = 0; i < instanceCount; i += BatchSize)
        {
            int len = Mathf.Min(BatchSize, instanceCount - i);

            Matrix4x4[] batch = new Matrix4x4[len];
            System.Array.Copy(allMatrices, i, batch, 0, len);

            _batches.Add(batch);
        }
    }

    void Update()
    {
        if (!mountainMesh || mountainMaterials == null)
            return;

        int subMeshCount = mountainMesh.subMeshCount;

        for (int sub = 0; sub < subMeshCount; sub++)
        {
            Material mat = mountainMaterials[sub];
            if (!mat) continue;

            foreach (var batch in _batches)
            {
                Graphics.DrawMeshInstanced(
                    mountainMesh,
                    sub,
                    mat,
                    batch,
                    batch.Length,
                    null,
                    UnityEngine.Rendering.ShadowCastingMode.Off,
                    false
                );
            }
        }
    }
}
