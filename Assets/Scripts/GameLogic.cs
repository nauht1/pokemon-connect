using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class GameLogic : MonoBehaviour
{
    private Tile firstTile = null;
    private Tile secondTile = null;
    private BoardManager boardManager;

    public int numsOfHint = 5;
    public int pointsPerPair = 10;

    private List<Vector2Int> tempPath = new List<Vector2Int>();

    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Tile clickedTile = hit.collider.GetComponent<Tile>();
                if (clickedTile != null) 
                {
                    HandleTileClick(clickedTile);
                }
            }
        }
    }

    void HandleTileClick(Tile clickedTile)
    {
        if (firstTile == null) // Nếu chưa có tile được chọn
        {
            firstTile = clickedTile;
            firstTile.SelectTile();
        }
        else if (secondTile == null && clickedTile != firstTile) // Nếu đã có 1 tile được chọn
        {
            secondTile = clickedTile;
            secondTile.SelectTile();

            if (CanConnect(firstTile, secondTile, true)) // Kiểm tra xem hai tile có thể kết nối không
            {
                boardManager.DrawPath(tempPath);
                firstTile.HideTile();
                secondTile.HideTile();

                boardManager.ReleaseTile(firstTile.gridPosition);
                boardManager.ReleaseTile(secondTile.gridPosition);
                GameManager.Instance.AddScore(pointsPerPair);

                CheckMoves(false);
            }
            else
            {
                // Deselected
                firstTile.SelectTile(); 
                secondTile.SelectTile();
            }

            firstTile = null;
            secondTile = null;
        }
    }

    bool CanConnect(Tile tileA, Tile tileB, bool savePath = false)
    {
        tempPath.Clear();

        if (tileA.tileId != tileB.tileId) return false;
        return CanConnectBFS(tileA.gridPosition, tileB.gridPosition, savePath) || 
            CanConnectBFS(tileB.gridPosition, tileA.gridPosition, savePath);
    }

    #region matrix approach, now use for check available moves and hints

    // Kiểm tra kết nối trực tiếp
    bool CanConnectStraight(Vector2Int posA, Vector2Int posB)
    {
        // Case cùng hàng
        if (posA.x == posB.x)
        {
            int minY = Mathf.Min(posA.y, posB.y);
            int maxY = Mathf.Max(posA.y, posB.y);

            for (int y = minY + 1; y < maxY; y++)
            {
                if (boardManager.HasTile(new Vector2Int(posA.x, y))) return false;
            }
            return true;
        }
        
        if (posA.y == posB.y) // Case cùng cột
        {
            int minX = Mathf.Min(posA.x, posB.x);
            int maxX = Mathf.Max(posA.x, posB.x);
             
            for (int x = minX + 1; x < maxX; x++)
            {
                if (boardManager.HasTile(new Vector2Int(x, posA.y))) return false;
            }
            return true;
        }

        return false;
    }

    // Kiểm tra kết nối với 1 góc vuông
    bool CanConnectOneCorner(Vector2Int posA, Vector2Int posB)
    {
        Vector2Int corner1 = new Vector2Int(posA.x, posB.y);
        Vector2Int corner2 = new Vector2Int(posB.x, posA.y);

        if (!boardManager.HasTile(corner1) && 
            CanConnectStraight(posA, corner1) &&
            CanConnectStraight(corner1, posB))
        {
            return true;

        }

        if (!boardManager.HasTile(corner2) && 
            CanConnectStraight(posA, corner2) &&
            CanConnectStraight(corner2, posB))
        {
            return true;

        }

        return false;
    }

    // Kiểm tra kết nối với 2 góc vuông (bằng 1 góc trung gian)
    bool CanConnectTwoCorners(Vector2Int posA, Vector2Int posB)
    {
        List<Vector2Int> potentialCorners = new List<Vector2Int>();

        // Cùng hàng
        for (int y = 0; y < boardManager.cols; y++)
        {
            Vector2Int corner = new Vector2Int(posA.x, y);
            if (!boardManager.HasTile(corner))
            {
                potentialCorners.Add(corner);
            }
        }

        // Cùng cột
        for (int x = 0; x < boardManager.rows; x++)
        {
            Vector2Int corner = new Vector2Int(x, posA.y);
            if (!boardManager.HasTile(corner))
            {
                potentialCorners.Add(corner);
            }
        }

        foreach (var corner in potentialCorners)
        {
            if (CanConnectOneCorner(posA, corner) &&
                CanConnectOneCorner(corner, posB))
            {
                return true;
            }
        }

        return false;
    }


    // Kiểm tra kết nối với trường hợp biên
    bool CanConnectViaBoundary(Vector2Int posA, Vector2Int posB)
    {
        int maxRow = boardManager.rows;
        int maxCol = boardManager.cols;

        bool canGoLeft = CanConnectStraight(posA, new Vector2Int(posA.x, -1)) && 
            CanConnectStraight(new Vector2Int(posB.x, -1), posB);

        bool canGoRight = CanConnectStraight(posA, new Vector2Int(posA.x, maxCol)) &&
            CanConnectStraight(new Vector2Int(posB.x, maxCol), posB);

        bool canGoTop = CanConnectStraight(posA, new Vector2Int(-1, posA.y)) &&
            CanConnectStraight(new Vector2Int(-1, posB.y), posB);

        bool canGoBottom = CanConnectStraight(posA, new Vector2Int(maxRow, posA.y)) &&
            CanConnectStraight(new Vector2Int(maxRow, posB.y), posB);

        return canGoLeft || canGoRight || canGoTop || canGoBottom;
    }

    #endregion
    
    bool CheckConnection(Tile tileA, Tile tileB)
    {
        Vector2Int posA = tileA.gridPosition;
        Vector2Int posB = tileB.gridPosition;

        if (tileA.tileId != tileB.tileId) return false;

        if (CanConnectStraight(posA, posB)) return true;

        if (CanConnectOneCorner(posA, posB)) return true;

        if (CanConnectViaBoundary(posA, posB)) return true;

        if (CanConnectTwoCorners(posA, posB)) return true;

        return false;
    }

    // Sử dụng để kiểm tra available move và show hint
    void CheckMoves(bool findHint)
    {
        boardManager.UpdateTileGroups();
        var tileGroups = boardManager.GetTileGroups();

        foreach (var group in tileGroups.Values)
        {
            int count = group.Count;
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (CheckConnection(group[i], group[j]))
                    {
                        if (findHint && numsOfHint > 0)
                        {
                            numsOfHint -= 1;
                            group[i].GetComponent<Tile>().HighlightTile();
                            group[j].GetComponent<Tile>().HighlightTile();
                        }

                        return;
                    }
                }
            }
        }

        if (tileGroups.Count == 0)
        {
            GameManager.Instance.ShowCongrats();
        }

        Debug.Log("No more move available");
    }

    public void ShowHint()
    {
        if (numsOfHint > 0)
        {
            CheckMoves(true);
        }
        else
        {
            Debug.Log("No more hints available");
        }
    }

    public void ResetHint()
    {
        foreach (var group in boardManager.GetTileGroups().Values)
        {
            foreach (var tile in group)
            {
                tile.ResetHighlight();
            }
        }
    }

    bool CanConnectBFS(Vector2Int start, Vector2Int end, bool savePath = false)
    {
        Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        // <currentPos, path, prevDir, turnCount>
        Queue<(Vector2Int, List<Vector2Int>, int, int)> queue = new Queue<(Vector2Int, List<Vector2Int>, int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        if (start == end) return true;

        // Thêm phần tử đầu tiên vào queue
        queue.Enqueue((start, new List<Vector2Int> { start }, -1, 0));
        visited.Add(start);

        while (queue.Count > 0)
        {
            // Pop khỏi hàng queue và duyệt
            var (currentPos, path, prevDir, turnCount) = queue.Dequeue();

            if (turnCount > 2) continue;

            if (currentPos == end)
            {
                if (savePath) tempPath = path;
                return true;
            }

            // Duyệt 4 hướng xung quanh vertex
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int nextPos = currentPos + directions[i];

                // Kiểm tra tile có hợp lệ ?
                if (!boardManager.IsValidPosition(nextPos))
                    continue;

                if (nextPos != end && boardManager.HasTile(nextPos)) continue;

                // Nếu hướng di chuyển không đổi thì giữ nguyên, ngược lại tăng 1
                int newTurnCount = (prevDir == -1 || prevDir == i) ? turnCount : turnCount + 1;
                if (newTurnCount > 2) continue;

                if (!visited.Contains(nextPos))
                {
                    // Tạo path mới
                    List<Vector2Int> newPath = new List<Vector2Int>(path) { nextPos };
                    queue.Enqueue((nextPos, newPath, i, newTurnCount));
                    visited.Add(nextPos);
                }
            }
        }

        return false;
    }
}
