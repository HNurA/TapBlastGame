using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    public int rows = 10;
    public int cols = 10;
    public GameObject blockPrefab;
    public float fallSpeed = 15f;
    private Block[,] grid;
    private bool isAnimating = false;
    private HashSet<Block> blocksBeingDestroyed = new HashSet<Block>();
    private Queue<List<Block>> destructionQueue = new Queue<List<Block>>();

    [System.Serializable]
    public class ColorSpriteSet
    {
        public string colorName;
        public Sprite[] sprites = new Sprite[4];
    }

    public ColorSpriteSet[] colorSets;

    void Start()
    {
        GenerateBoard();
    }

    public void RestartGame()
    {
        // Clear existing blocks
        if (grid != null)
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (grid[y, x] != null)
                    {
                        Destroy(grid[y, x].gameObject);
                    }
                }
            }
        }

        // Clear all queues and sets
        blocksBeingDestroyed.Clear();
        destructionQueue.Clear();
        isAnimating = false;

        // Generate new board
        GenerateBoard();
    }

    void UpdateAllBlockAppearances()
    {
        bool[,] processed = new bool[rows, cols];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (!processed[y, x] && grid[y, x] != null && !blocksBeingDestroyed.Contains(grid[y, x]))
                {
                    List<Block> group = GetMatchingGroup(grid[y, x]);
                    foreach (Block b in group)
                    {
                        if (b != null && !blocksBeingDestroyed.Contains(b))
                        {
                            int bX = Mathf.RoundToInt(b.transform.position.x);
                            int bY = Mathf.RoundToInt(-b.transform.position.y);
                            processed[bY, bX] = true;
                            b.UpdateAppearance(group.Count);
                        }
                    }
                }
            }
        }
    }

    void GenerateBoard()
    {
        grid = new Block[rows, cols];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                CreateBlockAt(x, y, true);
            }
        }
        
        UpdateAllBlockAppearances();
    }

    void CreateBlockAt(int x, int y, bool immediate)
    {
        Vector2 spawnPosition = immediate ? new Vector2(x, -y) : new Vector2(x, 0);
        GameObject newBlock = Instantiate(blockPrefab, spawnPosition, Quaternion.identity);
        Block block = newBlock.GetComponent<Block>();

        int randomColor = Random.Range(0, colorSets.Length);
        block.SetupBlock(randomColor, colorSets[randomColor].sprites);

        grid[y, x] = block;
        
        if (!immediate)
        {
            Vector2 targetPosition = new Vector2(x, -y);
            StartCoroutine(MoveBlockToPosition(block, targetPosition));
        }
    }

    public void OnBlockClicked(Block block)
    {
        if (block == null || blocksBeingDestroyed.Contains(block)) return;

        List<Block> group = GetMatchingGroup(block);
        
        if (group.Count >= 2)
        {
            foreach (Block b in group)
            {
                blocksBeingDestroyed.Add(b);
            }
            
            // Add score for the matched group
            GameManager.Instance.AddScore(group.Count);
            
            destructionQueue.Enqueue(new List<Block>(group));
            
            if (!isAnimating)
            {
                StartCoroutine(ProcessDestructionQueue());
            }
        }
    }

    private IEnumerator ProcessDestructionQueue()
    {
        isAnimating = true;

        while (destructionQueue.Count > 0)
        {
            List<Block> blocksToDestroy = destructionQueue.Dequeue();
            
            foreach (Block b in blocksToDestroy)
            {
                if (b != null)
                {
                    int x = Mathf.RoundToInt(b.transform.position.x);
                    int y = Mathf.RoundToInt(-b.transform.position.y);
                    grid[y, x] = null;
                    Destroy(b.gameObject);
                }
            }

            yield return StartCoroutine(ApplyGravity());
            yield return StartCoroutine(FillEmptySpaces());
            yield return StartCoroutine(WaitForBlockMovements());
            UpdateAllBlockAppearances();
        }

        yield return StartCoroutine(CheckAndHandleDeadlock());
        blocksBeingDestroyed.Clear();
        isAnimating = false;
    }

    private IEnumerator WaitForBlockMovements()
    {
        bool isMoving;
        do
        {
            isMoving = false;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (grid[y, x] != null)
                    {
                        Vector2 targetPos = new Vector2(x, -y);
                        Vector2 currentPos = grid[y, x].transform.position;
                        if (Vector2.Distance(currentPos, targetPos) > 0.01f)
                        {
                            isMoving = true;
                            break;
                        }
                    }
                }
                if (isMoving) break;
            }
            yield return new WaitForSeconds(0.05f);
        } while (isMoving);
    }

    IEnumerator ApplyGravity()
    {
        bool moved;
        do
        {
            moved = false;
            for (int x = 0; x < cols; x++)
            {
                for (int y = rows - 1; y > 0; y--)
                {
                    if (grid[y, x] == null && grid[y - 1, x] != null)
                    {
                        Block block = grid[y - 1, x];
                        grid[y, x] = block;
                        grid[y - 1, x] = null;

                        Vector2 targetPosition = new Vector2(x, -y);
                        StartCoroutine(MoveBlockToPosition(block, targetPosition));
                        moved = true;
                    }
                }
            }
            if (moved)
            {
                yield return new WaitForSeconds(0.1f);
            }
        } while (moved);
    }

    IEnumerator MoveBlockToPosition(Block block, Vector2 targetPosition)
    {
        if (block == null) yield break;
        
        Transform blockTransform = block.transform;
        while (block != null && Vector2.Distance(blockTransform.position, targetPosition) > 0.01f)
        {
            blockTransform.position = Vector2.MoveTowards(
                blockTransform.position,
                targetPosition,
                fallSpeed * Time.deltaTime
            );
            yield return null;
        }
        if (block != null)
        {
            blockTransform.position = targetPosition;
        }
    }

    IEnumerator FillEmptySpaces()
    {
        for (int x = 0; x < cols; x++)
        {
            int emptySpaces = 0;
            for (int y = 0; y < rows; y++)
            {
                if (grid[y, x] == null)
                {
                    CreateBlockAt(x, y, false);
                    emptySpaces++;
                }
            }
            if (emptySpaces > 0)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    List<Block> GetMatchingGroup(Block startBlock)
    {
        List<Block> matchingGroup = new List<Block>();
        if (startBlock == null || blocksBeingDestroyed.Contains(startBlock)) return matchingGroup;

        bool[,] visited = new bool[rows, cols];
        int startX = Mathf.RoundToInt(startBlock.transform.position.x);
        int startY = Mathf.RoundToInt(-startBlock.transform.position.y);
        int targetColor = startBlock.colorID;

        FindMatchingBlocks(startX, startY, targetColor, matchingGroup, visited);

        return matchingGroup;
    }

    void FindMatchingBlocks(int x, int y, int color, List<Block> group, bool[,] visited)
    {
        if (x < 0 || x >= cols || y < 0 || y >= rows || visited[y, x])
            return;

        Block block = grid[y, x];
        if (block == null || block.colorID != color || blocksBeingDestroyed.Contains(block))
            return;

        visited[y, x] = true;
        group.Add(block);

        FindMatchingBlocks(x + 1, y, color, group, visited);
        FindMatchingBlocks(x - 1, y, color, group, visited);
        FindMatchingBlocks(x, y + 1, color, group, visited);
        FindMatchingBlocks(x, y - 1, color, group, visited);
    }

    private bool IsDeadlocked()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (grid[y, x] != null && !blocksBeingDestroyed.Contains(grid[y, x]))
                {
                    List<Block> group = GetMatchingGroup(grid[y, x]);
                    if (group.Count >= 2)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private void ShuffleBoard()
    {
        // First, collect all blocks and their colors
        List<(int colorID, Sprite[] sprites)> blocks = new List<(int, Sprite[])>();
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (grid[y, x] != null && !blocksBeingDestroyed.Contains(grid[y, x]))
                {
                    Block block = grid[y, x];
                    blocks.Add((block.colorID, colorSets[block.colorID].sprites));
                }
            }
        }

        // Create a valid configuration
        bool[,] processed = new bool[rows, cols];
        List<(int colorID, Sprite[] sprites)> shuffledBlocks = new List<(int, Sprite[])>(blocks);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (!processed[y, x] && grid[y, x] != null)
                {
                    // Try to create at least one valid group
                    if (x < cols - 1 && y < rows - 1 && shuffledBlocks.Count >= 4)
                    {
                        // Create a 2x2 group of the same color
                        int randomIndex = Random.Range(0, shuffledBlocks.Count);
                        var selectedColor = shuffledBlocks[randomIndex];
                        
                        UpdateBlockColor(x, y, selectedColor.colorID, selectedColor.sprites);
                        UpdateBlockColor(x + 1, y, selectedColor.colorID, selectedColor.sprites);
                        UpdateBlockColor(x, y + 1, selectedColor.colorID, selectedColor.sprites);
                        UpdateBlockColor(x + 1, y + 1, selectedColor.colorID, selectedColor.sprites);

                        processed[y, x] = processed[y, x + 1] = processed[y + 1, x] = processed[y + 1, x + 1] = true;
                        
                        // Remove used blocks from the pool
                        shuffledBlocks.RemoveAt(randomIndex);
                        shuffledBlocks.RemoveAt(Random.Range(0, shuffledBlocks.Count));
                        shuffledBlocks.RemoveAt(Random.Range(0, shuffledBlocks.Count));
                        shuffledBlocks.RemoveAt(Random.Range(0, shuffledBlocks.Count));
                    }
                    else if (!processed[y, x] && shuffledBlocks.Count > 0)
                    {
                        // Fill remaining spaces randomly
                        int randomIndex = Random.Range(0, shuffledBlocks.Count);
                        var selectedColor = shuffledBlocks[randomIndex];
                        UpdateBlockColor(x, y, selectedColor.colorID, selectedColor.sprites);
                        processed[y, x] = true;
                        shuffledBlocks.RemoveAt(randomIndex);
                    }
                }
            }
        }

        UpdateAllBlockAppearances();
    }

    private void UpdateBlockColor(int x, int y, int colorID, Sprite[] sprites)
    {
        if (grid[y, x] != null)
        {
            grid[y, x].SetupBlock(colorID, sprites);
        }
    }

    private IEnumerator CheckAndHandleDeadlock()
    {
        if (IsDeadlocked())
        {
            // Optional: Add visual/audio feedback that a shuffle is happening
            yield return new WaitForSeconds(0.5f);
            ShuffleBoard();
            yield return new WaitForSeconds(0.5f);
        }
    }
}