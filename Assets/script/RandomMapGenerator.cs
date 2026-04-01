using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMapGenerator : MonoBehaviour
{
    [Header("地图设置")]
    public int mapWidth = 20;      // 地图宽度（格子数）
    public int mapHeight = 15;     // 地图高度（格子数）
    public float tileSize = 1f;    // 每个格子的大小（单位：Unity单位）

    [Header("瓦片素材")]
    public List<Sprite> tileSprites = new List<Sprite>();  // 地面瓦片
    public List<Sprite> outerWallSprites = new List<Sprite>();  // 外层围墙瓦片（新增）
    public List<Sprite> innerWallSprites = new List<Sprite>();  // 内层围墙瓦片（新增）

    [Header("生成设置")]
    public Transform tileContainer;   // 存放生成瓦片的父物体（可选）
    public bool clearExisting = true; // 生成前清除现有瓦片
    public bool randomizeRotation = false;  // 是否随机旋转
    public bool randomizeFlip = false;      // 是否随机翻转

    [Header("围墙设置")]
    public bool generateWalls = true;       // 是否生成围墙
    public int wallThickness = 2;           // 围墙总厚度（推荐设置为2，用于内外两层）
    public bool generateBorderOnly = true;  // true=只生成边界围墙，false=填充整个外围区域

    [Header("自定义围墙位置")]
    public List<CustomWallPosition> customWallPositions = new List<CustomWallPosition>();  // 自定义围墙位置（支持指定层数）
    public bool useCustomPositions = false;  // 是否使用自定义位置

    // 新增：自定义围墙位置数据结构
    [System.Serializable]
    public class CustomWallPosition
    {
        public Vector2Int position;  // 围墙位置
        public int layer = 1;        // 围墙层数：1=外层，2=内层，3=混合等
        public bool useCustomSprite = false;  // 是否使用自定义sprite
        public List<Sprite> customSprites = new List<Sprite>();  // 自定义sprite

        public CustomWallPosition(int x, int y, int layer = 1)
        {
            this.position = new Vector2Int(x, y);
            this.layer = layer;
        }
    }

    private List<GameObject> generatedTiles = new List<GameObject>();

    void Start()
    {
        GenerateMap();
    }

    [ContextMenu("生成地图")]
    public void GenerateMap()
    {
        // 检查是否有地面瓦片素材
        if (tileSprites == null || tileSprites.Count == 0)
        {
            Debug.LogError("没有设置任何地面瓦片素材！请在Inspector中将Sprite拖拽到 Tile Sprites 列表中");
            return;
        }

        // 如果启用了围墙，检查是否有围墙瓦片
        if (generateWalls)
        {
            if ((outerWallSprites == null || outerWallSprites.Count == 0) &&
                (innerWallSprites == null || innerWallSprites.Count == 0))
            {
                Debug.LogError("启用了围墙但没有设置围墙瓦片！请至少设置外层或内层围墙瓦片");
                return;
            }
        }

        // 清除现有地图
        if (clearExisting)
        {
            ClearMap();
        }

        // 生成地图瓦片
        GenerateTiles();

        Debug.Log($"地图生成完成！地面瓦片：{tileSprites.Count}种，外层围墙：{outerWallSprites.Count}种，内层围墙：{innerWallSprites.Count}种");
    }

    // 获取围墙的层数（0=不是围墙，1=外层，2=内层，3=两者都是）
    int GetWallLayer(int x, int y)
    {
        if (!generateWalls) return 0;

        // 使用自定义位置
        if (useCustomPositions && customWallPositions != null)
        {
            foreach (CustomWallPosition customPos in customWallPositions)
            {
                if (customPos.position.x == x && customPos.position.y == y)
                {
                    return customPos.layer;
                }
            }
            return 0;
        }

        // 使用边界围墙
        if (generateBorderOnly)
        {
            // 判断是外层还是内层
            bool isOuter = false;
            bool isInner = false;

            for (int thickness = 0; thickness < wallThickness; thickness++)
            {
                if (x == thickness || x >= mapWidth - thickness - 1 ||
                    y == thickness || y >= mapHeight - thickness - 1)
                {
                    if (thickness == 0)
                        isOuter = true;
                    else if (thickness == 1)
                        isInner = true;
                }
            }

            if (isOuter && isInner) return 3;  // 既是外层又是内层（厚度为1时）
            if (isOuter) return 1;
            if (isInner) return 2;
            return 0;
        }
        else
        {
            // 填充整个外围区域
            bool isOuter = false;
            bool isInner = false;

            for (int thickness = 0; thickness < wallThickness; thickness++)
            {
                if (x < thickness || x >= mapWidth - thickness ||
                    y < thickness || y >= mapHeight - thickness)
                {
                    if (thickness == 0)
                        isOuter = true;
                    else if (thickness == 1)
                        isInner = true;
                }
            }

            if (isOuter && isInner) return 3;
            if (isOuter) return 1;
            if (isInner) return 2;
            return 0;
        }
    }

    // 获取围墙的sprite
    Sprite GetWallSprite(int x, int y, int layer)
    {
        // 检查自定义位置是否有特殊sprite
        if (useCustomPositions && customWallPositions != null)
        {
            foreach (CustomWallPosition customPos in customWallPositions)
            {
                if (customPos.position.x == x && customPos.position.y == y &&
                    customPos.useCustomSprite && customPos.customSprites.Count > 0)
                {
                    return customPos.customSprites[Random.Range(0, customPos.customSprites.Count)];
                }
            }
        }

        // 根据层数选择sprite
        if (layer == 1)  // 外层
        {
            if (outerWallSprites != null && outerWallSprites.Count > 0)
            {
                return outerWallSprites[Random.Range(0, outerWallSprites.Count)];
            }
            else if (innerWallSprites != null && innerWallSprites.Count > 0)
            {
                // 如果没有外层，使用内层作为备用
                return innerWallSprites[Random.Range(0, innerWallSprites.Count)];
            }
        }
        else if (layer == 2)  // 内层
        {
            if (innerWallSprites != null && innerWallSprites.Count > 0)
            {
                return innerWallSprites[Random.Range(0, innerWallSprites.Count)];
            }
            else if (outerWallSprites != null && outerWallSprites.Count > 0)
            {
                // 如果没有内层，使用外层作为备用
                return outerWallSprites[Random.Range(0, outerWallSprites.Count)];
            }
        }
        else if (layer == 3)  // 既是外层又是内层（厚度为1时）
        {
            // 随机选择外层或内层
            if (outerWallSprites != null && outerWallSprites.Count > 0 &&
                innerWallSprites != null && innerWallSprites.Count > 0)
            {
                bool useOuter = Random.value > 0.5f;
                return useOuter ? outerWallSprites[Random.Range(0, outerWallSprites.Count)]
                                : innerWallSprites[Random.Range(0, innerWallSprites.Count)];
            }
            else if (outerWallSprites != null && outerWallSprites.Count > 0)
            {
                return outerWallSprites[Random.Range(0, outerWallSprites.Count)];
            }
            else if (innerWallSprites != null && innerWallSprites.Count > 0)
            {
                return innerWallSprites[Random.Range(0, innerWallSprites.Count)];
            }
        }

        return null;
    }

    void GenerateTiles()
    {
        // 确定容器
        Transform container = tileContainer != null ? tileContainer : transform;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int wallLayer = GetWallLayer(x, y);
                Sprite selectedSprite;
                bool isWall = wallLayer != 0;

                if (isWall)
                {
                    selectedSprite = GetWallSprite(x, y, wallLayer);
                    if (selectedSprite == null)
                    {
                        // 如果没有围墙瓦片，使用地面瓦片作为fallback
                        selectedSprite = tileSprites[Random.Range(0, tileSprites.Count)];
                        isWall = false;
                    }
                }
                else
                {
                    selectedSprite = tileSprites[Random.Range(0, tileSprites.Count)];
                }

                // 创建GameObject
                string wallType = "";
                if (isWall)
                {
                    if (wallLayer == 1) wallType = "_OuterWall";
                    else if (wallLayer == 2) wallType = "_InnerWall";
                    else if (wallLayer == 3) wallType = "_MixedWall";
                }

                GameObject tile = new GameObject($"Tile_{x}_{y}{wallType}");
                tile.transform.SetParent(container);
                tile.transform.position = new Vector3(x * tileSize, y * tileSize, 0);
                tile.transform.localScale = Vector3.one;

                // 添加SpriteRenderer组件
                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = selectedSprite;

                // 添加围墙标签组件（可选）
                if (isWall)
                {
                    WallTag wallTag = tile.AddComponent<WallTag>();
                    wallTag.wallLayer = wallLayer;  // 记录围墙层数
                }

                // 随机旋转（可选）
                if (randomizeRotation && !isWall)
                {
                    int rotation = Random.Range(0, 4) * 90;
                    tile.transform.rotation = Quaternion.Euler(0, 0, rotation);
                }
                else if (randomizeRotation && isWall && wallLayer == 2)  // 内层围墙可以旋转
                {
                    int rotation = Random.Range(0, 4) * 90;
                    tile.transform.rotation = Quaternion.Euler(0, 0, rotation);
                }

                // 随机翻转（可选）
                if (randomizeFlip && !isWall)
                {
                    renderer.flipX = Random.value > 0.5f;
                    renderer.flipY = Random.value > 0.5f;
                }
                else if (randomizeFlip && isWall && wallLayer == 2)
                {
                    renderer.flipX = Random.value > 0.5f;
                    renderer.flipY = Random.value > 0.5f;
                }

                // 保存以便后续清除
                generatedTiles.Add(tile);
            }
        }

        // 调整容器位置使其居中
        Vector3 centerOffset = new Vector3(-mapWidth * tileSize / 2f, -mapHeight * tileSize / 2f, 0);
        container.position += centerOffset;
    }

    [ContextMenu("清除地图")]
    public void ClearMap()
    {
        foreach (GameObject tile in generatedTiles)
        {
            if (tile != null)
            {
                if (Application.isPlaying)
                    Destroy(tile);
                else
                    DestroyImmediate(tile);
            }
        }
        generatedTiles.Clear();

        if (tileContainer != null)
        {
            for (int i = tileContainer.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(tileContainer.GetChild(i).gameObject);
                else
                    DestroyImmediate(tileContainer.GetChild(i).gameObject);
            }
        }
    }

    [ContextMenu("重新生成地图")]
    public void RegenerateMap()
    {
        ClearMap();
        GenerateMap();
    }

    // 新增：添加自定义围墙位置（指定层数）
    public void AddCustomWallPosition(int x, int y, int layer = 1)
    {
        customWallPositions.Add(new CustomWallPosition(x, y, layer));
    }

    // 新增：添加矩形围墙区域（支持指定层数）
    public void AddWallRectangle(int startX, int startY, int width, int height, int layer = 1, bool fillBorderOnly = true)
    {
        if (fillBorderOnly)
        {
            // 只添加边界
            for (int x = startX; x < startX + width; x++)
            {
                customWallPositions.Add(new CustomWallPosition(x, startY, layer));  // 底边
                customWallPositions.Add(new CustomWallPosition(x, startY + height - 1, layer));  // 顶边
            }

            for (int y = startY + 1; y < startY + height - 1; y++)
            {
                customWallPositions.Add(new CustomWallPosition(startX, y, layer));  // 左边
                customWallPositions.Add(new CustomWallPosition(startX + width - 1, y, layer));  // 右边
            }
        }
        else
        {
            // 填充整个区域
            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    customWallPositions.Add(new CustomWallPosition(x, y, layer));
                }
            }
        }

        Debug.Log($"已添加矩形围墙区域：({startX},{startY}) 到 ({startX + width - 1},{startY + height - 1})，层数：{layer}");
    }

    // 新增：添加双层围墙（自动生成内外层）
    public void AddDoubleLayerWall(int startX, int startY, int width, int height)
    {
        // 外层（层数1）
        AddWallRectangle(startX, startY, width, height, 1, true);
        // 内层（层数2）
        AddWallRectangle(startX + 1, startY + 1, width - 2, height - 2, 2, true);

        Debug.Log($"已添加双层围墙区域：({startX},{startY}) 到 ({startX + width - 1},{startY + height - 1})");
    }

    // 新增：添加示例双层围墙
    [ContextMenu("添加示例双层围墙")]
    public void AddExampleDoubleLayerWall()
    {
        customWallPositions.Clear();
        AddDoubleLayerWall(0, 0, mapWidth, mapHeight);
        useCustomPositions = true;
        Debug.Log($"已添加示例双层围墙，地图大小：{mapWidth}x{mapHeight}");
    }

    // 新增：清除所有自定义围墙位置
    [ContextMenu("清除自定义围墙位置")]
    public void ClearCustomWallPositions()
    {
        customWallPositions.Clear();
        Debug.Log("已清除所有自定义围墙位置");
    }

    // 获取指定位置的瓦片
    public GameObject GetTileAt(int x, int y)
    {
        int index = y * mapWidth + x;
        if (index >= 0 && index < generatedTiles.Count)
        {
            return generatedTiles[index];
        }
        return null;
    }

    // 获取世界坐标对应的瓦片
    public GameObject GetTileAtWorldPosition(Vector3 worldPos)
    {
        Transform container = tileContainer != null ? tileContainer : transform;
        Vector3 localPos = worldPos - container.position;

        int x = Mathf.FloorToInt(localPos.x / tileSize);
        int y = Mathf.FloorToInt(localPos.y / tileSize);

        if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
        {
            return GetTileAt(x, y);
        }
        return null;
    }

    // 编辑器辅助
    private void OnValidate()
    {
        if (tileSprites != null && tileSprites.Count == 0)
        {
            Debug.Log("请将地面瓦片Sprite拖拽到 Tile Sprites 列表中");
        }

        if (generateWalls)
        {
            if ((outerWallSprites == null || outerWallSprites.Count == 0) &&
                (innerWallSprites == null || innerWallSprites.Count == 0))
            {
                Debug.Log("请至少设置外层或内层围墙瓦片");
            }
        }
    }
}

// 围墙标签组件
public class WallTag : MonoBehaviour
{
    public int wallLayer = 1;  // 1=外层, 2=内层, 3=混合

    public bool IsOuterWall() { return wallLayer == 1 || wallLayer == 3; }
    public bool IsInnerWall() { return wallLayer == 2 || wallLayer == 3; }
}