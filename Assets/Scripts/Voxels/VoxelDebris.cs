using System.Collections.Generic;
using UnityEngine;

// Small loose voxel cubes thrown off by hits. Cubes are reused, and the oldest are recycled
// once the cap is reached, so heavy destruction never piles up unlimited physics objects.
public class VoxelDebris : MonoBehaviour
{
    private const int MaxActive = 250;

    private struct Piece
    {
        public GameObject cube;
        public Rigidbody body;
        public float expiresAt;
    }

    private static VoxelDebris instance;

    private readonly Queue<Piece> active = new Queue<Piece>();
    private readonly Stack<Piece> spare = new Stack<Piece>();

    public static void Spawn(Vector3 position, Quaternion rotation, Vector3 size, Material material, Vector3 velocity, float lifetime)
    {
        if (instance == null)
        {
            instance = new GameObject("VoxelDebris").AddComponent<VoxelDebris>();
        }
        instance.SpawnPiece(position, rotation, size, material, velocity, lifetime);
    }

    private void SpawnPiece(Vector3 position, Quaternion rotation, Vector3 size, Material material, Vector3 velocity, float lifetime)
    {
        Piece piece;
        if (active.Count >= MaxActive)
        {
            piece = active.Dequeue();
        }
        else if (spare.Count > 0)
        {
            piece = spare.Pop();
        }
        else
        {
            piece = new Piece { cube = GameObject.CreatePrimitive(PrimitiveType.Cube) };
            piece.cube.name = "Voxel Debris";
            piece.cube.transform.SetParent(transform, false);
            piece.body = piece.cube.AddComponent<Rigidbody>();
            piece.body.mass = 0.05f;
        }

        piece.cube.transform.SetPositionAndRotation(position, rotation);
        piece.cube.transform.localScale = size;
        piece.cube.GetComponent<MeshRenderer>().sharedMaterial = material;
        piece.cube.SetActive(true);
        piece.body.linearVelocity = velocity;
        piece.body.angularVelocity = Random.insideUnitSphere * 5f;
        piece.expiresAt = Time.time + lifetime;
        active.Enqueue(piece);
    }

    private void Update()
    {
        // Pieces are queued oldest first, so only the front ever needs checking
        while (active.Count > 0 && active.Peek().expiresAt <= Time.time)
        {
            Piece piece = active.Dequeue();
            piece.cube.SetActive(false);
            spare.Push(piece);
        }
    }
}
