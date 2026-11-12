using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace StandaloneTerrainFootprints
{
    /// <summary>Independent URP decals. Does not depend on or modify GlobalSnow.</summary>
    [DisallowMultipleComponent]
    public sealed class TerrainFootprintEmitter : MonoBehaviour
    {
        [Header("Required: native Unity Terrain and decal assets")]
        public Terrain targetTerrain;
        public DecalProjector projectorPrefab;
        public Material leftMaterial;
        public Material rightMaterial;

        [Header("Walking source (empty = this transform)")]
        public Transform followTarget;
        [Tooltip("Assign the player's controller to suppress footprints while airborne.")]
        public CharacterController characterController;
        public bool automaticSteps = true;
        [Min(0.05f)] public float stepDistance = 0.65f;
        [Min(0)] public float lateralOffset = 0.10f;
        [Min(0.1f)] public float teleportDistance = 3f;

        [Header("Ground test")]
        public LayerMask groundLayers = ~0;
        [Tooltip("Distance downward from the walking source. Use about 0.4 for feet/root, 2 for a camera.")]
        [Min(0.05f)] public float maxGroundDistance = 0.5f;
        [Min(0.01f)] public float rayStartLift = 0.25f;
        [Range(0, 80)] public float maxSlope = 50f;
        [Tooltip("-1 = every painted area; otherwise the zero-based snow TerrainLayer index.")]
        public int snowLayerIndex = -1;
        [Range(0, 1)] public float minimumSnowWeight = 0.5f;

        [Header("Appearance and bounded pool")]
        public Vector2 footprintSize = new Vector2(0.16f, 0.32f);
        [Min(0.02f)] public float projectionDepth = 0.16f;
        [Range(1, 128)] public int capacity = 48;
        [Min(0.1f)] public float lifetime = 35f;
        [Range(0, 1)] public float fadePortion = 0.25f;
        [Min(1)] public float drawDistance = 35f;

        sealed class Slot
        {
            public DecalProjector projector;
            public float born;
        }
        Slot[] slots;
        GameObject poolRoot;
        Vector3 previousPosition;
        float distanceSinceStep;
        int nextSlot;
        bool nextLeft = true;
        bool previousGrounded;
        bool ready;

        public int TotalStamped { get; private set; }
        public int ActiveCount
        {
            get
            {
                int count = 0;
                if (slots != null)
                    foreach (Slot slot in slots)
                        if (slot?.projector != null && slot.projector.gameObject.activeSelf) count++;
                return count;
            }
        }
        public int AllocatedCount
        {
            get
            {
                int count = 0;
                if (slots != null) foreach (Slot slot in slots) if (slot != null) count++;
                return count;
            }
        }
        Transform Source => followTarget != null ? followTarget : transform;

        void OnEnable()
        {
            if (!Application.isPlaying) return;
            if (targetTerrain == null || targetTerrain.terrainData == null ||
                targetTerrain.GetComponent<TerrainCollider>() == null || projectorPrefab == null ||
                leftMaterial == null || rightMaterial == null)
            {
                Debug.LogError("Footprints: assign Terrain (with TerrainCollider), projector prefab, and both decal materials.", this);
                return;
            }
            slots = new Slot[Mathf.Clamp(capacity, 1, 128)];
            poolRoot = new GameObject(name + " [Footprint pool]");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(poolRoot, gameObject.scene);
            previousPosition = Source.position;
            distanceSinceStep = 0;
            nextSlot = 0;
            nextLeft = true;
            previousGrounded = false;
            TotalStamped = 0;
            ready = true;
        }

        void LateUpdate()
        {
            if (!ready) return;
            foreach (Slot slot in slots)
            {
                if (slot?.projector == null || !slot.projector.gameObject.activeSelf) continue;
                float age = Time.time - slot.born;
                if (age >= lifetime) { slot.projector.gameObject.SetActive(false); continue; }
                float fadeTime = lifetime * fadePortion;
                slot.projector.fadeFactor = fadeTime > 0 ? Mathf.Clamp01((lifetime - age) / fadeTime) : 1;
            }

            Vector3 current = Source.position;
            Vector3 delta = current - previousPosition;
            previousPosition = current;
            bool grounded = TryGround(current, out _) &&
                (characterController == null || characterController.isGrounded);
            if (!automaticSteps || !grounded || delta.magnitude > teleportDistance)
            {
                distanceSinceStep = 0;
                previousGrounded = grounded;
                return;
            }
            if (!previousGrounded) { previousGrounded = true; distanceSinceStep = 0; return; }
            delta.y = 0;
            distanceSinceStep += delta.magnitude;
            if (distanceSinceStep < Mathf.Max(0.05f, stepDistance)) return;
            // At most one step per frame; never draw a trail across a teleport or long frame.
            distanceSinceStep = 0;
            Vector3 forward = delta.sqrMagnitude > 0.00001f ? delta.normalized : Source.forward;
            if (TryStamp(current, forward, nextLeft)) nextLeft = !nextLeft;
        }

        bool TryGround(Vector3 point, out RaycastHit hit)
        {
            if (!Physics.Raycast(point + Vector3.up * rayStartLift, Vector3.down, out hit,
                    rayStartLift + maxGroundDistance, groundLayers, QueryTriggerInteraction.Ignore)) return false;
            // Nearest hit must be this Terrain: do not stamp through roofs, props or other terrains.
            if (hit.collider != targetTerrain.GetComponent<TerrainCollider>()) return false;
            if (Vector3.Angle(hit.normal, Vector3.up) > maxSlope) return false;
            if (snowLayerIndex < 0) return true;
            TerrainData data = targetTerrain.terrainData;
            if (snowLayerIndex >= data.alphamapLayers) return false;
            Vector3 local = targetTerrain.transform.InverseTransformPoint(hit.point);
            int x = Mathf.Clamp(Mathf.FloorToInt(local.x / data.size.x * data.alphamapWidth), 0, data.alphamapWidth - 1);
            int z = Mathf.Clamp(Mathf.FloorToInt(local.z / data.size.z * data.alphamapHeight), 0, data.alphamapHeight - 1);
            return data.GetAlphamaps(x, z, 1, 1)[0, 0, snowLayerIndex] >= minimumSnowWeight;
        }

        /// <summary>Call with a near-ground position; use left=true/false for animation foot contacts.</summary>
        public bool StampAt(Vector3 footPosition, Vector3 forward, bool left)
        {
            if (!ready || !TryGround(footPosition, out RaycastHit hit)) return false;
            Vector3 tangent = Vector3.ProjectOnPlane(forward, hit.normal);
            if (tangent.sqrMagnitude < 0.001f) tangent = Vector3.ProjectOnPlane(Vector3.forward, hit.normal);
            Slot slot = slots[nextSlot];
            if (slot == null)
            {
                DecalProjector instance = Instantiate(projectorPrefab, poolRoot.transform);
                slot = new Slot { projector = instance };
                slots[nextSlot] = slot;
            }
            nextSlot = (nextSlot + 1) % slots.Length;
            slot.projector.gameObject.SetActive(false);
            slot.projector.transform.SetPositionAndRotation(hit.point + hit.normal * 0.005f,
                Quaternion.LookRotation(-hit.normal, tangent.normalized));
            slot.projector.transform.localScale = Vector3.one;
            slot.projector.material = left ? leftMaterial : rightMaterial;
            slot.projector.size = new Vector3(Mathf.Max(0.02f, footprintSize.x), Mathf.Max(0.02f, footprintSize.y), projectionDepth);
            slot.projector.pivot = Vector3.zero;
            slot.projector.drawDistance = drawDistance;
            slot.projector.fadeFactor = 1;
            slot.born = Time.time;
            slot.projector.gameObject.SetActive(true);
            TotalStamped++;
            return true;
        }

        bool TryStamp(Vector3 center, Vector3 forward, bool left)
        {
            forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            return StampAt(center + right * (left ? -lateralOffset : lateralOffset), forward, left);
        }

        [ContextMenu("Stamp Test Pair (Play Mode)")]
        public void StampTestPair()
        {
            if (!ready) { Debug.LogWarning("Footprints: enter Play Mode with all required references assigned.", this); return; }
            if (!TryStamp(Source.position, Source.forward, true)) Debug.LogWarning("Footprints: no eligible Terrain under source. Check ray distance, snow layer and source height.", this);
            TryStamp(Source.position + Vector3.ProjectOnPlane(Source.forward, Vector3.up).normalized * 0.3f, Source.forward, false);
        }

        [ContextMenu("Clear Footprints")]
        public void ClearFootprints()
        {
            if (slots == null) return;
            foreach (Slot slot in slots) if (slot?.projector != null) slot.projector.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            ready = false;
            if (poolRoot != null)
            {
                poolRoot.SetActive(false);
                if (Application.isPlaying) Destroy(poolRoot); else DestroyImmediate(poolRoot);
            }
            slots = null;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Source.position + Vector3.up * rayStartLift, Source.position - Vector3.up * maxGroundDistance);
        }
    }
}
