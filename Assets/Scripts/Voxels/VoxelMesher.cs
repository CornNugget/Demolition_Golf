using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Builds a mesh from a voxel grid (only faces that are exposed to air) and box colliders for loose chunks.
public static class VoxelMesher
{
    private static readonly Vector3Int[] FaceDirections =
    {
        Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down, new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
    };

    // Corners of each face on a unit cube, wound so Unity renders the outside
    private static readonly Vector3[][] FaceCorners =
    {
        new[] { new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1) },
        new[] { new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0) },
        new[] { new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0) },
        new[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1) },
        new[] { new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1), new Vector3(0, 0, 1) },
        new[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) },
    };

    private static readonly Vector2[] FaceUVs = { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };

    public static Mesh Build(VoxelGrid grid, Mesh mesh)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        for (int z = 0; z < grid.SizeZ; z++)
        for (int y = 0; y < grid.SizeY; y++)
        for (int x = 0; x < grid.SizeX; x++)
        {
            if (!grid.Get(x, y, z)) continue;

            var cell = new Vector3Int(x, y, z);
            Vector3 corner = grid.Origin + Vector3.Scale(cell, grid.VoxelSize);
            for (int face = 0; face < 6; face++)
            {
                if (grid.Get(cell + FaceDirections[face])) continue;

                int first = vertices.Count;
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(corner + Vector3.Scale(FaceCorners[face][i], grid.VoxelSize));
                    normals.Add(FaceDirections[face]);
                    uvs.Add(FaceUVs[i]);
                }
                triangles.Add(first); triangles.Add(first + 1); triangles.Add(first + 2);
                triangles.Add(first); triangles.Add(first + 2); triangles.Add(first + 3);
            }
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = "Voxels", indexFormat = IndexFormat.UInt32 };
            mesh.MarkDynamic();
        }
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    // Covers the solid voxels with as few boxes as possible, for the colliders of falling chunks
    public static List<Bounds> Boxes(VoxelGrid grid)
    {
        var boxes = new List<Bounds>();
        var used = new bool[grid.SizeX, grid.SizeY, grid.SizeZ];
        bool Free(int x, int y, int z) => grid.Get(x, y, z) && !used[x, y, z];

        for (int z = 0; z < grid.SizeZ; z++)
        for (int y = 0; y < grid.SizeY; y++)
        for (int x = 0; x < grid.SizeX; x++)
        {
            if (!Free(x, y, z)) continue;

            int width = 1;
            while (Free(x + width, y, z)) width++;

            int height = 1;
            while (RowFree(x, y + height, z, width)) height++;

            int depth = 1;
            while (SlabFree(x, y, z + depth, width, height)) depth++;

            for (int dz = 0; dz < depth; dz++)
            for (int dy = 0; dy < height; dy++)
            for (int dx = 0; dx < width; dx++)
                used[x + dx, y + dy, z + dz] = true;

            Vector3 size = Vector3.Scale(new Vector3(width, height, depth), grid.VoxelSize);
            Vector3 min = grid.Origin + Vector3.Scale(new Vector3(x, y, z), grid.VoxelSize);
            boxes.Add(new Bounds(min + size / 2f, size));
        }
        return boxes;

        bool RowFree(int x0, int y0, int z0, int width)
        {
            for (int dx = 0; dx < width; dx++)
                if (!Free(x0 + dx, y0, z0)) return false;
            return true;
        }

        bool SlabFree(int x0, int y0, int z0, int width, int height)
        {
            for (int dy = 0; dy < height; dy++)
                if (!RowFree(x0, y0 + dy, z0, width)) return false;
            return true;
        }
    }
}
