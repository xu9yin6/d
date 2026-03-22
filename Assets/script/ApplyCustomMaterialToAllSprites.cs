using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ApplyCustomMaterialToAllSprites : MonoBehaviour
{
    [Header("材质设置")]
    [Tooltip("要应用的自定义材质")]
    public Material customMaterial;

    [Header("选项")]
    [Tooltip("是否包含子对象")]
    public bool includeChildren = true;

    [Tooltip("是否在运行时自动应用")]
    public bool applyOnStart = false;

    [Tooltip("是否保留原有材质的实例化")]
    public bool instantiateMaterial = false;

    void Start()
    {
        if (applyOnStart && customMaterial != null)
        {
            ApplyMaterialToAllSprites();
        }
    }

    [ContextMenu("应用材质到所有Sprite")]
    public void ApplyMaterialToAllSprites()
    {
        if (customMaterial == null)
        {
            Debug.LogError("请先设置自定义材质！");
            return;
        }

        // 获取所有SpriteRenderer组件
        SpriteRenderer[] spriteRenderers;

        if (includeChildren)
        {
            // 获取当前对象及其所有子对象的SpriteRenderer
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        else
        {
            // 仅获取当前对象的SpriteRenderer
            spriteRenderers = GetComponents<SpriteRenderer>();
        }

        int appliedCount = 0;

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (instantiateMaterial)
            {
                // 实例化材质，避免多个对象共享同一个材质实例
                renderer.material = new Material(customMaterial);
            }
            else
            {
                // 直接共享材质
                renderer.material = customMaterial;
            }
            appliedCount++;
        }

        Debug.Log($"已为 {appliedCount} 个SpriteRenderer应用材质: {customMaterial.name}");
    }

    [ContextMenu("恢复默认材质")]
    public void ResetMaterials()
    {
        SpriteRenderer[] spriteRenderers;

        if (includeChildren)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        else
        {
            spriteRenderers = GetComponents<SpriteRenderer>();
        }

        int resetCount = 0;

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            // 恢复为默认的Sprites-Default材质
            renderer.material = null;
            resetCount++;
        }

        Debug.Log($"已恢复 {resetCount} 个SpriteRenderer的材质");
    }
}

#if UNITY_EDITOR
// 编辑器扩展，方便批量操作
[CustomEditor(typeof(ApplyCustomMaterialToAllSprites))]
public class ApplyCustomMaterialToAllSpritesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ApplyCustomMaterialToAllSprites script = (ApplyCustomMaterialToAllSprites)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);
        
        if (GUILayout.Button("立即应用材质", GUILayout.Height(30)))
        {
            script.ApplyMaterialToAllSprites();
        }
        
        if (GUILayout.Button("恢复默认材质", GUILayout.Height(30)))
        {
            script.ResetMaterials();
        }
        
        EditorGUILayout.HelpBox(
            "提示：\n" +
            "- 点击\"立即应用材质\"按钮可以在编辑器中直接应用材质\n" +
            "- 勾选\"运行时自动应用\"会在游戏开始时自动应用\n" +
            "- 勾选\"实例化材质\"可以为每个对象创建独立的材质实例",
            MessageType.Info
        );
    }
}
#endif