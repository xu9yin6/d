using UnityEngine;

namespace Goldmetal.UndeadSurvivor
{
    public class BatchAddCollider : MonoBehaviour
    {
        [ContextMenu("为所有子对象添加 BoxCollider2D 和 Rigidbody2D")]
        void AddCollidersToAllChildren()
        {
            foreach (Transform child in transform)
            {
                // 添加 Box Collider 2D
                if (child.GetComponent<Collider2D>() == null)
                {
                    child.gameObject.AddComponent<BoxCollider2D>();
                    Debug.Log($"添加 BoxCollider2D 到: {child.name}");
                }

                // 添加 Rigidbody 2D 并设为 Static
                if (child.GetComponent<Rigidbody2D>() == null)
                {
                    Rigidbody2D rb = child.gameObject.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Static;
                    Debug.Log($"添加 Rigidbody2D 到: {child.name}");
                }
            }

            Debug.Log("批量添加完成！");
        }

        [ContextMenu("移除所有子对象的 Collider 和 Rigidbody")]
        void RemoveCollidersFromAllChildren()
        {
            foreach (Transform child in transform)
            {
                Collider2D collider = child.GetComponent<Collider2D>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                    Debug.Log($"移除 Collider: {child.name}");
                }

                Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    DestroyImmediate(rb);
                    Debug.Log($"移除 Rigidbody: {child.name}");
                }
            }

            Debug.Log("批量移除完成！");
        }
    }
}