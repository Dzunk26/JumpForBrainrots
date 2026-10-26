using UnityEngine;

// duong mui ten cau vong la 1 tam plane keo gian tu diem start (con cua player) den target
// di thang, xuyen qua vat can; muon chinh cao thap thi di chuyen 2 diem start/target trong scene
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TutorialPathArrow : MonoBehaviour {
    private static readonly int LengthId = Shader.PropertyToID("_Length");
    private static readonly int WidthId = Shader.PropertyToID("_Width");

    private static Mesh planeMesh;

    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private float width = 1.6f;
    [SerializeField] private float hideDistance = 1.5f;
    [SerializeField] private bool isFaceCamera = true;
    [SerializeField] private Camera targetCamera;

    private Transform from;
    private Transform to;
    private MaterialPropertyBlock propertyBlock;

    public bool IsShowing => to != null;

    private void Reset() {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Awake() {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = GetPlaneMesh();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        propertyBlock = new MaterialPropertyBlock();
        meshRenderer.enabled = false;
    }

    public void Show(Transform startPoint, Transform target) {
        from = startPoint;
        to = target;
    }

    public void Hide() {
        to = null;
        meshRenderer.enabled = false;
    }

    private void LateUpdate() {
        if (from == null || to == null) return;

        Vector3 start = from.position;
        Vector3 direction = to.position - start;
        float length = direction.magnitude;

        if (length < hideDistance) {
            meshRenderer.enabled = false;
            return;
        }

        direction /= length;

        // mesh dai 1 theo truc Z -> scale Z = chieu dai, scale X = be rong
        transform.SetPositionAndRotation(start, Quaternion.LookRotation(direction, GetPlaneNormal(start + direction * length * 0.5f, direction)));
        transform.localScale = new Vector3(width, 1f, length);

        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(LengthId, length);
        propertyBlock.SetFloat(WidthId, width);
        meshRenderer.SetPropertyBlock(propertyBlock);
        meshRenderer.enabled = true;
    }

    // mat plane (truc Y local) quay ve camera, chi xoay quanh truc doc duong nen dau/cuoi van dinh o start/target
    // tat isFaceCamera thi plane nam ngang nhu cu
    private Vector3 GetPlaneNormal(Vector3 center, Vector3 direction) {
        if (isFaceCamera) {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera != null) {
                Vector3 toCamera = Vector3.ProjectOnPlane(targetCamera.transform.position - center, direction);
                if (toCamera.sqrMagnitude > 0.0001f) return toCamera.normalized;
            }
        }

        // huong gan nhu thang dung thi LookRotation voi up bi suy bien -> doi truc up
        return Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
    }

    // plane 1x1 nam tren mat XZ, tu z = 0 den z = 1, uv.x chay doc chieu dai, uv.y chay ngang
    private static Mesh GetPlaneMesh() {
        if (planeMesh != null) return planeMesh;

        planeMesh = new Mesh { name = "Tutorial Path Plane" };
        planeMesh.vertices = new[] {
            new Vector3(-0.5f, 0f, 0f),
            new Vector3(0.5f, 0f, 0f),
            new Vector3(-0.5f, 0f, 1f),
            new Vector3(0.5f, 0f, 1f),
        };
        planeMesh.uv = new[] {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
        };
        planeMesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        planeMesh.RecalculateNormals();
        planeMesh.RecalculateBounds();

        return planeMesh;
    }
}
