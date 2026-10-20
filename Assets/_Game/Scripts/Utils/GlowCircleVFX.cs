using UnityEngine;

// doi mau vong tron sang theo tung instance (MaterialPropertyBlock) de dung chung 1 material
// tu sinh mesh tru ho (ban kinh 1, cao 1) cho tuong sang, chinh kich thuoc bang scale cua object Wall
[ExecuteAlways]
public class GlowCircleVFX : MonoBehaviour {
    private const int WALL_SEGMENTS = 64;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");

    private static Mesh wallMesh;

    [SerializeField] private Renderer[] renderers;
    [SerializeField] private MeshFilter wallMeshFilter;
    [SerializeField, ColorUsage(true, true)] private Color color = new Color(0.2f, 1.6f, 0.9f, 1f);
    [SerializeField, Range(0f, 2f)] private float alpha = 1f;

    private MaterialPropertyBlock propertyBlock;

    private void OnEnable() {
        if (wallMeshFilter != null) wallMeshFilter.sharedMesh = GetWallMesh();
        Apply();
    }

    private void OnValidate() {
        Apply();
    }

    public void SetColor(Color newColor) {
        color = newColor;
        Apply();
    }

    public void SetAlpha(float newAlpha) {
        alpha = newAlpha;
        Apply();
    }

    public Color GetColor() {
        return color;
    }

    private void Apply() {
        if (renderers == null) return;
        propertyBlock ??= new MaterialPropertyBlock();

        for (int i = 0; i < renderers.Length; i++) {
            if (renderers[i] == null) continue;
            renderers[i].GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(ColorId, color);
            propertyBlock.SetFloat(AlphaId, alpha);
            renderers[i].SetPropertyBlock(propertyBlock);
        }
    }

    private static Mesh GetWallMesh() {
        if (wallMesh != null) return wallMesh;

        // tru ho khong nap, nhan doi dinh o duong noi de uv.x chay lien 0..1
        int ringCount = WALL_SEGMENTS + 1;
        Vector3[] vertices = new Vector3[ringCount * 2];
        Vector3[] normals = new Vector3[ringCount * 2];
        Vector2[] uvs = new Vector2[ringCount * 2];
        int[] triangles = new int[WALL_SEGMENTS * 6];

        for (int i = 0; i < ringCount; i++) {
            float u = (float)i / WALL_SEGMENTS;
            float angle = u * Mathf.PI * 2f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            vertices[i] = direction;
            vertices[i + ringCount] = direction + Vector3.up;
            normals[i] = direction;
            normals[i + ringCount] = direction;
            uvs[i] = new Vector2(u, 0f);
            uvs[i + ringCount] = new Vector2(u, 1f);
        }

        for (int i = 0; i < WALL_SEGMENTS; i++) {
            int t = i * 6;
            triangles[t] = i;
            triangles[t + 1] = i + ringCount;
            triangles[t + 2] = i + 1;
            triangles[t + 3] = i + 1;
            triangles[t + 4] = i + ringCount;
            triangles[t + 5] = i + ringCount + 1;
        }

        wallMesh = new Mesh {
            name = "GlowCircle Wall",
            hideFlags = HideFlags.DontSave,
            vertices = vertices,
            normals = normals,
            uv = uvs,
            triangles = triangles
        };
        wallMesh.RecalculateBounds();

        return wallMesh;
    }
}
