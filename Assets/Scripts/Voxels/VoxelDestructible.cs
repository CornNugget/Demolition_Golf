using System.Collections.Generic;
using UnityEngine;
using VoxelSystem;

public enum VoxelShapeSource
{
    Primitive, // fill the object's bounding box (or a cylinder), as the Destructible prefabs do
    Model,     // voxelize the actual meshes of an imported art asset
}

// The object can become voxels on start or only when its health runs out.
// Destructible3D on the same object handles every hit:
// health, the ball rules and XP. Once its health is gone, each hit it passes on carves a hole here,
// and any part that is no longer connected to the ground breaks off and falls as its own chunk.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelDestructible : MonoBehaviour
{
    [Header("Model Appearance")]
    [Tooltip("Keep the original model and its UVs until Destructible3D reports that health has run out. Small chips are skipped before that point.")]
    [SerializeField] private bool voxelizeOnBreak = false;

    [Tooltip("Material for the inside of the object (cut faces) and its debris. On a Model the outside keeps the art's own materials and texture colours. Empty = the object's current material")]
    [SerializeField] private Material voxelMaterial;

    [Header("Voxels")]
    [Tooltip("Primitive fills the object's box or cylinder shape. Model voxelizes the meshes of an imported art asset (child meshes included); tick Read/Write Enabled in the model's import settings")]
    [SerializeField] private VoxelShapeSource shapeSource = VoxelShapeSource.Model;
    [Tooltip("Primitive only: edge length of one voxel in metres")]
    [SerializeField] private float voxelSize = 0.25f;
    [Tooltip("Model only: voxels along the model's longest side, so every model gets the same detail whatever its size. Higher looks smoother but loads slower in WebGL")]
    [Range(4, 48)]
    [SerializeField] private int modelDetail = 16;

    [Header("Carving")]
    [Tooltip("Hole radius a regular ball carves in each material, in metres; stone and diamond balls carve bigger holes. " +
             "Tuned for primitives about 3 m across: on a Model the holes and chips shrink or grow with its size")]
    [SerializeField] private float glassHoleRadius = 1.2f;
    [SerializeField] private float dryWallHoleRadius = 0.95f;
    [SerializeField] private float woodHoleRadius = 0.7f;
    [SerializeField] private float steelHoleRadius = 0.45f;
    [Tooltip("Largest hole any hit can carve, in metres")]
    [SerializeField] private float maxHoleRadius = 2.5f;
    [Tooltip("Ball speed that carves a full-size hole; slower hits carve smaller ones")]
    [SerializeField] private float fullHoleSpeed = 5f;
    [Tooltip("How much of its speed the ball keeps after carving through (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float keepSpeed = 0.8f;

    [Header("Chipping (hits before health reaches 0)")]
    [Tooltip("Radius of the chip a Regular ball knocks off on a hit that doesn't break the object, in metres; Stone and Diamond balls chip more. 0 = no chipping")]
    [Min(0f)]
    [SerializeField] private float chipSize = 0.4f;
    [Tooltip("Most of the object (0-1) that chips can remove before it breaks, so it stays recognisable")]
    [Range(0f, 1f)]
    [SerializeField] private float maxChipped = 0.15f;

    [Header("Breaking Apart")]
    [Tooltip("When less than this fraction of the object is left standing, the rest crumbles and the object is gone (uncovers switches)")]
    [Range(0f, 1f)]
    [SerializeField] private float crumbleBelow = 0.2f;
    [Tooltip("Loose pieces with fewer voxels than this turn into debris instead of falling chunks")]
    [SerializeField] private int minChunkVoxels = 8;
    [Tooltip("Mass of one voxel in a falling chunk")]
    [SerializeField] private float massPerVoxel = 0.05f;

    [Header("Debris")]
    [Tooltip("Fraction of carved voxels that fly off as debris cubes (0-1); the rest vanish")]
    [Range(0f, 1f)]
    [SerializeField] private float debrisKept = 0.35f;
    [SerializeField] private int maxDebrisPerHit = 40;
    [Tooltip("Speed debris is thrown away from the hit")]
    [SerializeField] private float burstSpeed = 3f;
    [Tooltip("Seconds before debris and fallen chunks disappear")]
    [Min(0.1f)]
    [SerializeField] private float debrisLifetime = 13f;

    // Size of object the carving and chip sizes above are tuned for, in metres
    private const float TunedSize = 3f;

    private Destructible3D rules;
    // Multiplies the carving and chip sizes: 1 for primitives, the model's size / TunedSize for models
    private float sizeScale = 1f;
    private VoxelGrid grid;
    private MeshFilter meshFilter;
    // Solid boxes covering the voxels. A MeshCollider would only be the visible surface,
    // and a fast ball could slip inside it and pass through.
    private readonly List<BoxCollider> boxColliders = new List<BoxCollider>();
    // Inside material, also used for debris
    private Material material;
    // The voxel mesh's materials: the inside material, then the art's own materials (Model only)
    private Material[] slotMaterials;
    private int startCount;
    private bool isChunk;
    private ObstacleMaterialType chunkType = ObstacleMaterialType.Wood;
    private bool crumbled;

#if UNITY_EDITOR
    // Runs in the editor when the component is added: it needs a Destructible3D for health, the ball rules
    // and XP, so add one if it's missing. Not a RequireComponent, because broken-off chunks must not get one
    private void Reset()
    {
        if (GetComponent<Destructible3D>() == null) UnityEditor.Undo.AddComponent<Destructible3D>(gameObject);
    }

    // Runs in the editor whenever this object is loaded or edited: Model mode has to read the model's
    // triangles, so turn on Read/Write for any model it uses that doesn't have it, wherever the model lives
    private void OnValidate()
    {
        if (Application.isPlaying || shapeSource != VoxelShapeSource.Model) return;

        foreach (MeshFilter part in GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh mesh = part.sharedMesh;
            if (mesh == null || mesh.isReadable) continue;
            string path = UnityEditor.AssetDatabase.GetAssetPath(mesh);
            if (!(UnityEditor.AssetImporter.GetAtPath(path) is UnityEditor.ModelImporter model) || model.isReadable) continue;

            // Reimporting isn't allowed during OnValidate, so do it straight after
            model.isReadable = true;
            UnityEditor.EditorApplication.delayCall += model.SaveAndReimport;
        }
    }
#endif

    private void Start()
    {
        // Chunks are set up by the object they broke off from
        if (grid != null) return;

        rules = GetComponent<Destructible3D>();
        meshFilter = GetComponent<MeshFilter>();
        material = GetComponent<MeshRenderer>().sharedMaterial;

        if (voxelizeOnBreak == false)
        {
            BuildFromShape();
        }
    }

    private void BuildFromShape()
    {
        // The inside material shows wherever the art's surface has been cut away
        if (voxelMaterial != null) material = voxelMaterial;

        var artMaterials = new List<Material>();
        if (shapeSource == VoxelShapeSource.Model)
        {
            if (!FillFromModel(artMaterials)) return;
            if (material == null && artMaterials.Count > 0) material = artMaterials[0];
        }
        else
        {
            FillPrimitive();
        }
        startCount = grid.SolidCount;

        // Slot 0 is the inside material, then the art's own materials for the painted surface voxels
        artMaterials.Insert(0, material);
        slotMaterials = artMaterials.ToArray();
        GetComponent<MeshRenderer>().sharedMaterials = slotMaterials;

        // The grid now carries the object's size, so the scale goes back to 1
        transform.localScale = Vector3.one;
        foreach (Collider old in GetComponents<Collider>()) Destroy(old);

        // Ensures standing object stays where it was placed; only broken-off chunks move
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null) body.isKinematic = true;

        Rebuild();
    }

    private void FillPrimitive()
    {
        Mesh source = meshFilter.sharedMesh;
        Vector3 size = Vector3.Scale(source.bounds.size, transform.localScale);
        Vector3 center = Vector3.Scale(source.bounds.center, transform.localScale);
        bool round = GetComponent<CapsuleCollider>() != null || source.name.Contains("Cylinder");

        int nx = Mathf.Max(1, Mathf.RoundToInt(size.x / voxelSize));
        int ny = Mathf.Max(1, Mathf.RoundToInt(size.y / voxelSize));
        int nz = Mathf.Max(1, Mathf.RoundToInt(size.z / voxelSize));
        var cell = new Vector3(size.x / nx, size.y / ny, size.z / nz);
        grid = new VoxelGrid(nx, ny, nz, cell, center - size / 2f);

        for (int z = 0; z < nz; z++)
        for (int y = 0; y < ny; y++)
        for (int x = 0; x < nx; x++)
        {
            float dx = (x + 0.5f) / nx - 0.5f;
            float dz = (z + 0.5f) / nz - 0.5f;
            if (!round || dx * dx + dz * dz <= 0.25f) grid.Set(x, y, z, true);
        }
    }

    // Voxelizes every mesh on this object and its children: marks each voxel the surface passes through
    // (triangle-box test from unity-voxel, Assets/ThirdParty/unity-voxel), fills whatever is enclosed,
    // then hides the original art. Returns false if nothing could be voxelized.
    private bool FillFromModel(List<Material> artMaterials)
    {
        // Put every part of the model into this object's own space, keeping its current scale.
        // Each triangle remembers its UVs and which of the art's materials it uses (slot 1 and up; 0 = none)
        Matrix4x4 toLocal = Matrix4x4.Scale(transform.localScale) * transform.worldToLocalMatrix;
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();
        var triangleSlots = new List<int>();
        foreach (MeshFilter part in GetComponentsInChildren<MeshFilter>())
        {
            Mesh mesh = part.sharedMesh;
            if (mesh == null) continue;
            if (!mesh.isReadable)
            {
                Debug.LogError($"{name}: can't voxelize '{mesh.name}'. Tick Read/Write Enabled in the model's import settings.", this);
                continue;
            }

            Matrix4x4 partToLocal = toLocal * part.transform.localToWorldMatrix;
            int first = vertices.Count;
            Vector3[] partVertices = mesh.vertices;
            Vector2[] partUVs = mesh.uv;
            for (int v = 0; v < partVertices.Length; v++)
            {
                vertices.Add(partToLocal.MultiplyPoint3x4(partVertices[v]));
                uvs.Add(v < partUVs.Length ? partUVs[v] : Vector2.zero);
            }

            Renderer art = part.GetComponent<Renderer>();
            Material[] partMaterials = art != null ? art.sharedMaterials : new Material[0];
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                Material artMaterial = sub < partMaterials.Length ? partMaterials[sub] : null;
                int slot = 0;
                if (artMaterial != null)
                {
                    if (!artMaterials.Contains(artMaterial)) artMaterials.Add(artMaterial);
                    slot = artMaterials.IndexOf(artMaterial) + 1;
                }
                int[] subTriangles = mesh.GetTriangles(sub);
                foreach (int index in subTriangles) triangles.Add(first + index);
                for (int t = 0; t < subTriangles.Length / 3; t++) triangleSlots.Add(slot);
            }
        }
        if (triangles.Count == 0)
        {
            Debug.LogWarning($"{name}: Shape Source is Model, but no readable meshes were found on it or its children.", this);
            return false;
        }

        var bounds = new Bounds(vertices[triangles[0]], Vector3.zero);
        foreach (int index in triangles) bounds.Encapsulate(vertices[index]);

        // Cube voxels sized so the longest side gets Model Detail of them, with the grid centred on the model
        Vector3 size = bounds.size;
        float longest = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
        float unit = longest / modelDetail;
        int nx = Mathf.Max(1, Mathf.CeilToInt(size.x / unit - 0.001f));
        int ny = Mathf.Max(1, Mathf.CeilToInt(size.y / unit - 0.001f));
        int nz = Mathf.Max(1, Mathf.CeilToInt(size.z / unit - 0.001f));
        grid = new VoxelGrid(nx, ny, nz, Vector3.one * unit, bounds.center - new Vector3(nx, ny, nz) * (unit / 2f));
        sizeScale = longest / TunedSize;

        // Surface: every voxel a triangle passes through, using the unity-voxel triangle-box test.
        // Each one is painted with the texture colour of the triangle's closest point, so it keeps the art's look
        var cellBox = new Bounds(Vector3.zero, Vector3.one * unit);
        Vector3Int last = new Vector3Int(nx - 1, ny - 1, nz - 1);
        for (int i = 0; i < triangles.Count; i += 3)
        {
            int ia = triangles[i], ib = triangles[i + 1], ic = triangles[i + 2];
            int slot = triangleSlots[i / 3];
            var tri = new CPUVoxelizer.Triangle(vertices[ia], vertices[ib], vertices[ic], Vector3.forward);
            Vector3Int min = Vector3Int.Min(Vector3Int.Max(grid.LocalToCell(tri.bounds.min), Vector3Int.zero), last);
            Vector3Int max = Vector3Int.Min(Vector3Int.Max(grid.LocalToCell(tri.bounds.max), Vector3Int.zero), last);
            for (int z = min.z; z <= max.z; z++)
            for (int y = min.y; y <= max.y; y++)
            for (int x = min.x; x <= max.x; x++)
            {
                var cell = new Vector3Int(x, y, z);
                if (grid.Get(cell)) continue;
                cellBox.center = grid.LocalCenter(cell);
                if (!CPUVoxelizer.Intersects(tri, cellBox)) continue;
                grid.Set(cell, true);
                if (slot > 0)
                {
                    grid.Paint(cell, slot, UVClosestTo(cellBox.center, vertices[ia], vertices[ib], vertices[ic], uvs[ia], uvs[ib], uvs[ic]));
                }
            }
        }
        // Inside: whatever the outside can't reach. Works for models made of overlapping or open parts
        grid.FillEnclosed();

        if (grid.SolidCount == 0)
        {
            Debug.LogWarning($"{name}: the model produced no voxels.", this);
            return false;
        }

        // The voxel mesh replaces the art: hide the model's own renderers and colliders
        MeshRenderer ownRenderer = GetComponent<MeshRenderer>();
        foreach (Renderer art in GetComponentsInChildren<Renderer>())
        {
            if (art != ownRenderer) art.enabled = false;
        }
        foreach (Collider artCollider in GetComponentsInChildren<Collider>())
        {
            if (artCollider.gameObject != gameObject) artCollider.enabled = false;
        }
        return true;
    }

    // Texture coordinate of the point on triangle abc nearest to p (close enough for picking one colour per voxel)
    private static Vector2 UVClosestTo(Vector3 p, Vector3 a, Vector3 b, Vector3 c, Vector2 uvA, Vector2 uvB, Vector2 uvC)
    {
        Vector3 ab = b - a, ac = c - a, ap = p - a;
        float abab = Vector3.Dot(ab, ab), abac = Vector3.Dot(ab, ac), acac = Vector3.Dot(ac, ac);
        float denominator = abab * acac - abac * abac;
        if (denominator <= 1e-12f) return uvA;

        float apab = Vector3.Dot(ap, ab), apac = Vector3.Dot(ap, ac);
        float towardB = Mathf.Clamp01((acac * apab - abac * apac) / denominator);
        float towardC = Mathf.Clamp01((abab * apac - abac * apab) / denominator);
        float total = towardB + towardC;
        if (total > 1f)
        {
            towardB /= total;
            towardC /= total;
        }
        return uvA + (uvB - uvA) * towardB + (uvC - uvA) * towardC;
    }

    private void Rebuild()
    {
        meshFilter.sharedMesh = VoxelMesher.Build(grid, meshFilter.sharedMesh != null && meshFilter.sharedMesh.name == "Voxels" ? meshFilter.sharedMesh : null, slotMaterials.Length);

        // Reuse the existing box colliders, add more if needed, and switch off any left over
        List<Bounds> boxes = VoxelMesher.Boxes(grid);
        for (int i = 0; i < boxes.Count; i++)
        {
            if (i == boxColliders.Count) boxColliders.Add(gameObject.AddComponent<BoxCollider>());
            boxColliders[i].center = boxes[i].center;
            boxColliders[i].size = boxes[i].size;
            boxColliders[i].enabled = true;
        }
        for (int i = boxes.Count; i < boxColliders.Count; i++) boxColliders[i].enabled = false;
    }

    // Called by Destructible3D on the hit that takes its health to 0, and on every powered hit after that.
    // Destructible3D has already checked the ball rules and paid the XP.
    public void TakeHit(BallMaterialController ball, Collision collision)
    {
        if (crumbled) return;

        if (grid == null)
        {
            BuildFromShape();
            // Couldn't be voxelized (the reason is already in the Console), so leave the object as it is
            if (grid == null) return;
        }

        ObstacleMaterialType type = ObstacleType;
        Vector3 incoming = ball.LastVelocity;

        float speedFactor = Mathf.Clamp(incoming.magnitude / fullHoleSpeed, 0.5f, 1.5f);
        float radius = Mathf.Min(HoleRadius(type) * Mathf.Sqrt(ball.Damage) * speedFactor, maxHoleRadius) * sizeScale;
        // On a small model the scaled hole could miss every voxel; always take at least one
        if (shapeSource == VoxelShapeSource.Model) radius = Mathf.Max(radius, LargestVoxelSide);
        Vector3 hitPoint = collision.GetContact(0).point;

        int before = grid.SolidCount;
        // Centre the hole slightly inside the object so the ball punches through rather than scooping the surface
        Carve(hitPoint + incoming.normalized * radius * 0.5f, radius, hitPoint, incoming);
        if (grid.SolidCount == before) return;

        BreakOffLoosePieces(incoming);

        if (grid.SolidCount < startCount * crumbleBelow) Crumble(incoming);
        else
        {
            Rebuild();
            Rigidbody body = GetComponent<Rigidbody>();
            if (isChunk && body != null) body.mass = Mathf.Max(0.2f, grid.SolidCount * massPerVoxel);
        }

        // Physics already bounced the ball, so give its speed back to carry it through the hole
        if (collision.rigidbody != null) collision.rigidbody.linearVelocity = incoming * keepSpeed;
    }

    // A fallen chunk has no Destructible3D, so it checks the ball rules itself. Like the broken object it
    // came from, it has no health and pays no XP: every powered hit carves it, and steel needs Stone or Diamond
    private void OnCollisionEnter(Collision collision)
    {
        if (!isChunk || crumbled) return;

        BallMaterialController ball = collision.collider.GetComponentInParent<BallMaterialController>();
        if (ball == null) return;
        if (ObstacleType == ObstacleMaterialType.Steel && ball.currentMaterial == BallMaterialType.Regular) return;

        PlayerController player = ball.GetComponent<PlayerController>();
        if (player != null)
        {
            if (!player.powered) return;
            player.powered = false;
        }
        TakeHit(ball, collision);
    }

    // Called by Destructible3D on a hit that lowers health without breaking the object:
    // knocks a small chip off at the impact point to show the damage. No XP granted
    public void Chip(BallMaterialController ball, Collision collision)
    {
        if (isChunk || crumbled || grid == null || chipSize <= 0f) return;
        if (startCount - grid.SolidCount >= startCount * maxChipped) return;

        // At least one voxel across, so even a small chip is visible
        float radius = Mathf.Max(chipSize * Mathf.Sqrt(ball.Damage) * sizeScale, LargestVoxelSide);
        Vector3 hitPoint = collision.GetContact(0).point;
        Vector3 incoming = ball.LastVelocity;

        int before = grid.SolidCount;
        // Centred on the surface, so it takes a bite out of the face rather than punching inward
        Carve(hitPoint, radius, hitPoint, incoming);
        if (grid.SolidCount == before) return;

        BreakOffLoosePieces(incoming * 0.2f);
        Rebuild();
    }

    // Chunks have no Destructible3D, so they remember the material of the object they broke off from
    private ObstacleMaterialType ObstacleType => rules != null ? rules.ObstacleType : chunkType;

    private float LargestVoxelSide => Mathf.Max(grid.VoxelSize.x, Mathf.Max(grid.VoxelSize.y, grid.VoxelSize.z));

    private float HoleRadius(ObstacleMaterialType type)
    {
        switch (type)
        {
            case ObstacleMaterialType.Glass: return glassHoleRadius;
            case ObstacleMaterialType.DryWall: return dryWallHoleRadius;
            case ObstacleMaterialType.Steel: return steelHoleRadius;
            default: return woodHoleRadius;
        }
    }

    private void Carve(Vector3 worldCenter, float radius, Vector3 hitPoint, Vector3 incoming)
    {
        Vector3 local = transform.InverseTransformPoint(worldCenter);
        Vector3Int min = grid.LocalToCell(local - Vector3.one * radius);
        Vector3Int max = grid.LocalToCell(local + Vector3.one * radius);
        int debris = 0;

        for (int z = min.z; z <= max.z; z++)
        for (int y = min.y; y <= max.y; y++)
        for (int x = min.x; x <= max.x; x++)
        {
            var cell = new Vector3Int(x, y, z);
            if (!grid.Get(cell) || (grid.LocalCenter(cell) - local).sqrMagnitude > radius * radius) continue;

            grid.Set(cell, false);
            if (debris < maxDebrisPerHit && Random.value < debrisKept)
            {
                Vector3 position = transform.TransformPoint(grid.LocalCenter(cell));
                Vector3 velocity = (position - hitPoint).normalized * burstSpeed + incoming * 0.3f;
                VoxelDebris.Spawn(position, transform.rotation, grid.VoxelSize, material, velocity, debrisLifetime);
                debris++;
            }
        }
    }

    private void BreakOffLoosePieces(Vector3 incoming)
    {
        List<List<Vector3Int>> islands = grid.FindIslands();
        if (islands.Count == 0) return;

        var staying = new HashSet<List<Vector3Int>>();
        if (isChunk)
        {
            // A fallen chunk isn't standing on anything: its biggest piece stays as the chunk, the rest split off
            List<Vector3Int> biggest = islands[0];
            foreach (List<Vector3Int> island in islands)
            {
                if (island.Count > biggest.Count) biggest = island;
            }
            staying.Add(biggest);
        }
        else
        {
            // Pieces touching the bottom layer rest on the ground and stay; everything else falls.
            // If the whole bottom has been knocked out, nothing is holding the object up, so all of it falls
            foreach (List<Vector3Int> island in islands)
            {
                if (island.Exists(cell => cell.y == 0)) staying.Add(island);
            }
        }

        int debris = 0;
        foreach (List<Vector3Int> island in islands)
        {
            if (staying.Contains(island)) continue;

            foreach (Vector3Int cell in island) grid.Set(cell, false);

            if (island.Count >= minChunkVoxels)
            {
                SpawnChunk(island, incoming);
                continue;
            }
            foreach (Vector3Int cell in island)
            {
                if (debris++ >= maxDebrisPerHit) break;
                VoxelDebris.Spawn(transform.TransformPoint(grid.LocalCenter(cell)), transform.rotation, grid.VoxelSize, material, incoming * 0.2f, debrisLifetime);
            }
        }
    }

    private void SpawnChunk(List<Vector3Int> island, Vector3 incoming)
    {
        Vector3Int min = island[0], max = island[0];
        foreach (Vector3Int cell in island)
        {
            min = Vector3Int.Min(min, cell);
            max = Vector3Int.Max(max, cell);
        }

        var chunkGrid = new VoxelGrid(max.x - min.x + 1, max.y - min.y + 1, max.z - min.z + 1, grid.VoxelSize,
            grid.Origin + Vector3.Scale(min, grid.VoxelSize));
        foreach (Vector3Int cell in island)
        {
            chunkGrid.Set(cell - min, true);
            int slot = grid.PaintOf(cell, out Vector2 uv);
            if (slot > 0) chunkGrid.Paint(cell - min, slot, uv);
        }

        var chunk = new GameObject(name + " Chunk");
        // Loose chunks go on the Voxel layer, which the camera ignores so they don't keep pushing it around.
        // Standing objects keep their own layer and block the camera like walls, so it can't get inside a box
        int voxelLayer = LayerMask.NameToLayer("Voxel");
        chunk.layer = voxelLayer >= 0 ? voxelLayer : gameObject.layer;
        chunk.transform.SetParent(transform.parent, false);
        chunk.transform.SetPositionAndRotation(transform.position, transform.rotation);

        VoxelDestructible piece = chunk.AddComponent<VoxelDestructible>();
        // Same Inspector settings (hole sizes, debris and so on) as the object it broke off from
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this), piece);
        piece.isChunk = true;
        piece.grid = chunkGrid;
        piece.startCount = island.Count;
        piece.chunkType = ObstacleType;
        piece.sizeScale = sizeScale;
        piece.meshFilter = chunk.GetComponent<MeshFilter>();
        piece.material = material;
        piece.slotMaterials = slotMaterials;
        chunk.GetComponent<MeshRenderer>().sharedMaterials = slotMaterials;
        // Builds the chunk's mesh and its box colliders
        piece.Rebuild();

        // A chunk breaking off a falling chunk keeps falling with it
        Rigidbody own = GetComponent<Rigidbody>();
        Rigidbody body = chunk.AddComponent<Rigidbody>();
        body.mass = Mathf.Max(0.2f, island.Count * massPerVoxel);
        body.linearVelocity = (isChunk && own != null ? own.linearVelocity : Vector3.zero) + incoming * 0.25f;

        Destroy(chunk, debrisLifetime);
    }

    private void Crumble(Vector3 incoming)
    {
        crumbled = true;
        int debris = 0;
        for (int z = 0; z < grid.SizeZ; z++)
        for (int y = 0; y < grid.SizeY; y++)
        for (int x = 0; x < grid.SizeX; x++)
        {
            var cell = new Vector3Int(x, y, z);
            if (!grid.Get(cell) || debris >= maxDebrisPerHit * 2 || Random.value > debrisKept) continue;
            VoxelDebris.Spawn(transform.TransformPoint(grid.LocalCenter(cell)), transform.rotation, grid.VoxelSize, material, incoming * 0.2f, debrisLifetime);
            debris++;
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
