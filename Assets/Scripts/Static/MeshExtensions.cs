using UnityEngine;

public static class MeshExtensions
{
    public static Mesh Duplicate(this Mesh self)
    {
        return new Mesh
        {
            vertices = self.vertices,
            uv = self.uv,
            uv2 = self.uv2,
            uv3 = self.uv3,
            uv4 = self.uv4,
            triangles = self.triangles,
            bindposes = self.bindposes,
            boneWeights = self.boneWeights,
            bounds = self.bounds,
            colors = self.colors,
            colors32 = self.colors32,
            normals = self.normals,
            subMeshCount = self.subMeshCount,
            tangents = self.tangents
        };
    }
}
