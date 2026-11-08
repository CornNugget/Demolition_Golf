using System.Collections.Generic;
using UnityEngine;

// A 3D grid of solid or empty voxels in an object's local space.
public class VoxelGrid
{
    private static readonly Vector3Int[] Neighbours =
    {
        Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down, new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
    };

    public readonly int SizeX, SizeY, SizeZ;
    public readonly Vector3 VoxelSize;
    // Local position of the outer corner of voxel (0, 0, 0)
    public readonly Vector3 Origin;
    public int SolidCount { get; private set; }

    private readonly bool[] solid;

    public VoxelGrid(int sizeX, int sizeY, int sizeZ, Vector3 voxelSize, Vector3 origin)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        SizeZ = sizeZ;
        VoxelSize = voxelSize;
        Origin = origin;
        solid = new bool[sizeX * sizeY * sizeZ];
    }

    private int Index(int x, int y, int z) => (z * SizeY + y) * SizeX + x;

    public bool InBounds(int x, int y, int z) => x >= 0 && y >= 0 && z >= 0 && x < SizeX && y < SizeY && z < SizeZ;

    public bool Get(int x, int y, int z) => InBounds(x, y, z) && solid[Index(x, y, z)];

    public bool Get(Vector3Int cell) => Get(cell.x, cell.y, cell.z);

    public void Set(int x, int y, int z, bool value)
    {
        if (!InBounds(x, y, z)) return;
        int i = Index(x, y, z);
        if (solid[i] == value) return;
        solid[i] = value;
        SolidCount += value ? 1 : -1;
    }

    public void Set(Vector3Int cell, bool value) => Set(cell.x, cell.y, cell.z, value);

    public Vector3 LocalCenter(Vector3Int cell) => Origin + Vector3.Scale(cell + new Vector3(0.5f, 0.5f, 0.5f), VoxelSize);

    public Vector3Int LocalToCell(Vector3 local)
    {
        Vector3 scaled = local - Origin;
        return new Vector3Int(
            Mathf.FloorToInt(scaled.x / VoxelSize.x),
            Mathf.FloorToInt(scaled.y / VoxelSize.y),
            Mathf.FloorToInt(scaled.z / VoxelSize.z));
    }

    // Groups the solid voxels into pieces that touch each other face to face
    public List<List<Vector3Int>> FindIslands()
    {
        var islands = new List<List<Vector3Int>>();
        var visited = new bool[solid.Length];
        var stack = new Stack<Vector3Int>();

        for (int z = 0; z < SizeZ; z++)
        for (int y = 0; y < SizeY; y++)
        for (int x = 0; x < SizeX; x++)
        {
            int start = Index(x, y, z);
            if (!solid[start] || visited[start]) continue;

            var island = new List<Vector3Int>();
            visited[start] = true;
            stack.Push(new Vector3Int(x, y, z));
            while (stack.Count > 0)
            {
                Vector3Int cell = stack.Pop();
                island.Add(cell);
                foreach (Vector3Int step in Neighbours)
                {
                    Vector3Int next = cell + step;
                    if (!InBounds(next.x, next.y, next.z)) continue;
                    int i = Index(next.x, next.y, next.z);
                    if (!solid[i] || visited[i]) continue;
                    visited[i] = true;
                    stack.Push(next);
                }
            }
            islands.Add(island);
        }
        return islands;
    }
}
