using UnityEngine;

namespace Goldmetal.UndeadSurvivor
{
    public class BatchAddCollider : MonoBehaviour
    {
        [Header("设置")]
        [Tooltip("是否同时处理子对象的子对象（递归）")]
        public bool recursive = false;

        [Tooltip("关键词（不区分大小写）")]
        public string[] keywords = new string[] { "Wall", "墙壁" };

        [ContextMenu("为符合条件的子对象添加 BoxCollider2D")]
        void AddCollidersToMatchingChildren()
        {
            int addedCount = 0;
            int skippedCount = 0;

            if (recursive)
            {
                // 递归查找所有符合条件的子对象
                AddCollidersRecursive(transform, ref addedCount, ref skippedCount);
            }
            else
            {
                // 只查找直接子对象
                foreach (Transform child in transform)
                {
                    if (ShouldAddCollider(child.name))
                    {
                        if (child.GetComponent<Collider2D>() == null)
                        {
                            child.gameObject.AddComponent<BoxCollider2D>();
                            Debug.Log($"✅ 添加 BoxCollider2D 到: {child.name}");
                            addedCount++;
                        }
                        else
                        {
                            Debug.Log($"⏭️ {child.name} 已有 Collider，跳过");
                            skippedCount++;
                        }
                    }
                    else
                    {
                        Debug.Log($"⏭️ {child.name} 不包含关键词，跳过");
                    }
                }
            }

            Debug.Log($"批量添加完成！添加: {addedCount} 个，跳过: {skippedCount} 个");
        }

        void AddCollidersRecursive(Transform parent, ref int addedCount, ref int skippedCount)
        {
            foreach (Transform child in parent)
            {
                if (ShouldAddCollider(child.name))
                {
                    if (child.GetComponent<Collider2D>() == null)
                    {
                        child.gameObject.AddComponent<BoxCollider2D>();
                        Debug.Log($"✅ 添加 BoxCollider2D 到: {child.name}");
                        addedCount++;
                    }
                    else
                    {
                        Debug.Log($"⏭️ {child.name} 已有 Collider，跳过");
                        skippedCount++;
                    }
                }

                // 递归处理子对象的子对象
                AddCollidersRecursive(child, ref addedCount, ref skippedCount);
            }
        }

        bool ShouldAddCollider(string objectName)
        {
            string lowerName = objectName.ToLower();
            foreach (string keyword in keywords)
            {
                if (lowerName.Contains(keyword.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        [ContextMenu("移除所有子对象的 Collider")]
        void RemoveCollidersFromAllChildren()
        {
            int removedCount = 0;

            if (recursive)
            {
                RemoveCollidersRecursive(transform, ref removedCount);
            }
            else
            {
                foreach (Transform child in transform)
                {
                    Collider2D collider = child.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        DestroyImmediate(collider);
                        Debug.Log($"🗑️ 移除 Collider: {child.name}");
                        removedCount++;
                    }
                }
            }

            Debug.Log($"批量移除完成！共移除 {removedCount} 个 Collider");
        }

        void RemoveCollidersRecursive(Transform parent, ref int removedCount)
        {
            foreach (Transform child in parent)
            {
                Collider2D collider = child.GetComponent<Collider2D>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                    Debug.Log($"🗑️ 移除 Collider: {child.name}");
                    removedCount++;
                }

                RemoveCollidersRecursive(child, ref removedCount);
            }
        }

        // 可选：添加一键设置Layer的功能
        [ContextMenu("将符合条件的子对象设置为 Wall 层")]
        void SetMatchingChildrenToWallLayer()
        {
            int setCount = 0;
            int wallLayer = LayerMask.NameToLayer("Wall");

            if (wallLayer == -1)
            {
                Debug.LogError("❌ 找不到 'Wall' 层！请先在 Tags & Layers 中创建 Wall 层");
                return;
            }

            if (recursive)
            {
                SetLayerRecursive(transform, wallLayer, ref setCount);
            }
            else
            {
                foreach (Transform child in transform)
                {
                    if (ShouldAddCollider(child.name))
                    {
                        child.gameObject.layer = wallLayer;
                        Debug.Log($"🎨 设置 {child.name} 为 Wall 层");
                        setCount++;
                    }
                }
            }

            Debug.Log($"Layer设置完成！共设置 {setCount} 个对象为 Wall 层");
        }

        void SetLayerRecursive(Transform parent, int layer, ref int setCount)
        {
            foreach (Transform child in parent)
            {
                if (ShouldAddCollider(child.name))
                {
                    child.gameObject.layer = layer;
                    Debug.Log($"🎨 设置 {child.name} 为 Wall 层");
                    setCount++;
                }

                SetLayerRecursive(child, layer, ref setCount);
            }
        }
    }
}