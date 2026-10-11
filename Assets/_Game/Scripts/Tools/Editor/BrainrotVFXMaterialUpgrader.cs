using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEngine;

// cac pack VFX (Hovl, Epic Toon FX, Sci-Fi Arsenal, Voxel Arsenal) dung shader built-in "Particles/Standard Unlit"
// URP khong ve duoc shader nay (hien hong / sai mau) nen chuyen sang shader particle cua URP
// dung chinh upgrader cua URP, giong Window > Rendering > Render Pipeline Converter > Material Upgrade
public static class BrainrotVFXMaterialUpgrader {
    private static string[] PARTICLE_SHADERS = new string[] { "Particles/Standard Unlit", "Particles/Standard Surface", "Particles/VertexLit Blended" };
    private static string STANDARD_SHADER = "Standard";

    private static List<MaterialUpgrader> upgraders;

    // chuyen material cua 1 hieu ung (sua thang vao material cua pack), tra ve so material da chuyen
    public static int UpgradeEffect(GameObject effect) {
        int upgradedCount = 0;

        foreach (Renderer renderer in effect.GetComponentsInChildren<Renderer>(true)) {
            foreach (Material material in renderer.sharedMaterials) {
                if (UpgradeMaterial(material)) upgradedCount++;
            }

            if (renderer is ParticleSystemRenderer particleRenderer && UpgradeMaterial(particleRenderer.trailMaterial)) {
                upgradedCount++;
            }
        }

        return upgradedCount;
    }

    // chuyen tat ca material trong cac folder pack, de xem demo scene cua pack cung dung mau
    public static int UpgradeFolders(string[] folders) {
        int upgradedCount = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:Material", folders)) {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (UpgradeMaterial(material)) upgradedCount++;
        }

        AssetDatabase.SaveAssets();
        return upgradedCount;
    }

    public static bool IsNeedUpgrade(Material material) {
        if (material == null || material.shader == null) return false;

        string shaderName = material.shader.name;
        if (shaderName == STANDARD_SHADER) return true;

        foreach (string particleShader in PARTICLE_SHADERS) {
            if (shaderName == particleShader) return true;
        }

        return false;
    }

    private static bool UpgradeMaterial(Material material) {
        if (!IsNeedUpgrade(material)) return false;

        MaterialUpgrader.Upgrade(material, GetUpgraders(), MaterialUpgrader.UpgradeFlags.None);
        EditorUtility.SetDirty(material);
        return true;
    }

    private static List<MaterialUpgrader> GetUpgraders() {
        if (upgraders != null) return upgraders;

        upgraders = new List<MaterialUpgrader>();
        foreach (string particleShader in PARTICLE_SHADERS) {
            upgraders.Add(new ParticleUpgrader(particleShader));
        }

        upgraders.Add(new StandardUpgrader(STANDARD_SHADER));
        return upgraders;
    }
}
