using UnityEngine;

public static class Helpers
{
    public static Vector2 GetUV(RaycastHit hit)
    {
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (!meshCollider || !meshCollider.sharedMesh)
            return Vector2.zero;

        Mesh mesh = meshCollider.sharedMesh;

        int triangleIndex = hit.triangleIndex;

        if (triangleIndex < 0)
            return Vector2.zero;

        int[] triangles = mesh.triangles;
        Vector2[] uvs = mesh.uv;
        Vector3[] vertices = mesh.vertices;

        int i0 = triangles[triangleIndex * 3];
        int i1 = triangles[triangleIndex * 3 + 1];
        int i2 = triangles[triangleIndex * 3 + 2];

        Vector3 localHit =
            hit.collider.transform.InverseTransformPoint(hit.point);

        Vector3 barycentric = Barycentric(
            localHit,
            vertices[i0],
            vertices[i1],
            vertices[i2]
        );

        return
            uvs[i0] * barycentric.x +
            uvs[i1] * barycentric.y +
            uvs[i2] * barycentric.z;
    }

    private static Vector3 Barycentric(
        Vector3 p,
        Vector3 a,
        Vector3 b,
        Vector3 c)
    {
        Vector3 v0 = b - a;
        Vector3 v1 = c - a;
        Vector3 v2 = p - a;

        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);

        float denominator = d00 * d11 - d01 * d01;

        if (Mathf.Abs(denominator) < Mathf.Epsilon)
            return Vector3.zero;

        float v = (d11 * d20 - d01 * d21) / denominator;
        float w = (d00 * d21 - d01 * d20) / denominator;
        float u = 1f - v - w;

        return new Vector3(u, v, w);
    }
}