using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AutoSetChildLayer : MonoBehaviour
{
    [Header("Layer设置规则")]
    [Tooltip("是否在Start时自动执行")]
    public bool autoSetOnStart = true;

    [Tooltip("是否在编辑器模式下也自动执行")]
    public bool autoSetInEditor = true;

    [Header("关键词映射")]
    public LayerRule[] rules = new LayerRule[]
    {
        new LayerRule { keyword = "Wall", targetLayer = "Wall" },
        new LayerRule { keyword = "Ground", targetLayer = "Ground" },
        new LayerRule { keyword = "Platform", targetLayer = "Ground" },
        new LayerRule { keyword = "Floor", targetLayer = "Ground" }
    };

    [System.Serializable]
    public class LayerRule
    {
        public string keyword;
        public string targetLayer;
    }

    void Start()
    {
        if (autoSetOnStart)
        {
            SetChildLayers();
        }
    }

    [ContextMenu("手动设置子对象Layer")]
    public void SetChildLayers()
    {
        int count = 0;
        foreach (Transform child in transform)
        {
            if (SetLayerByName(child.gameObject))
            {
                count++;
            }
        }

        Debug.Log($"[{gameObject.name}] 完成！共设置了 {count} 个子对象的Layer");
    }

    private bool SetLayerByName(GameObject obj)
    {
        string objName = obj.name.ToLower();

        foreach (var rule in rules)
        {
            if (objName.Contains(rule.keyword.ToLower()))
            {
                int layerId = LayerMask.NameToLayer(rule.targetLayer);
                if (layerId != -1)
                {
                    obj.layer = layerId;
                    Debug.Log($"设置 {obj.name} → {rule.targetLayer} 层");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"找不到 '{rule.targetLayer}' 层，请先在Tags & Layers中创建");
                    return false;
                }
            }
        }

        return false;
    }

    // 编辑器模式下自动更新（可选）
#if UNITY_EDITOR
    void OnValidate()
    {
        if (autoSetInEditor && !Application.isPlaying)
        {
            // 延迟执行，避免频繁调用
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    SetChildLayers();
                }
            };
        }
    }
#endif
}