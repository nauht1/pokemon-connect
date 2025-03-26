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
        return CanConnectStraight(tileA.gridPosition, tileB.gridPosition, savePath) ||
           CanConnectOneCorner(tileA.gridPosition, tileB.gridPosition, savePath) ||
           CanConnectTwoCorners(tileA.gridPosition, tileB.gridPosition, savePath) ||
           CanConnectViaBoundary(tileA.gridPosition, tileB.gridPosition, savePath);
    }

    #region matrix approach

    // Kiểm tra kết nối trực tiếp
    bool CanConnectStraight(Vector2Int posA, Vector2Int posB, bool savePath = false)
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
            if (savePath)
            {
                tempPath.Clear();
                int step = posA.y < posB.y ? 1 : -1;
                for (int y = posA.y; y != posB.y + step; y += step)
                {
                    tempPath.Add(new Vector2Int(posA.x, y));
                }
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
            if (savePath)
            {
                tempPath.Clear();
                int step = posA.x < posB.x ? 1 : -1;
                for (int x = posA.x; x != posB.x + step; x += step)
                {
                    tempPath.Add(new Vector2Int(x, posA.y));
                }
            }
            return true;
        }

        return false;
    }

    // Kiểm tra kết nối với 1 góc vuông
    bool CanConnectOneCorner(Vector2Int posA, Vector2Int posB, bool savePath = false)
    {
        Vector2Int corner1 = new Vector2Int(posA.x, posB.y);
        Vector2Int corner2 = new Vector2Int(posB.x, posA.y);

        if (!boardManager.HasTile(corner1) && 
            CanConnectStraight(posA, corner1) &&
            CanConnectStraight(corner1, posB))
        {
            if (savePath)
            {
                tempPath.Clear();
                int step = posA.y < posB.y ? 1 : -1;
                for (int y = posA.y; y != corner1.y + step; y += step)
                    tempPath.Add(new Vector2Int(posA.x, y));
                step = corner1.x < posB.x ? 1 : -1;
                for (int x = corner1.x; x != posB.x + step; x += step)
                    tempPath.Add(new Vector2Int(x, posB.y));
            }
            return true;

        }

        if (!boardManager.HasTile(corner2) && 
            CanConnectStraight(posA, corner2) &&
            CanConnectStraight(corner2, posB))
        {
            if (savePath)
            {
                tempPath.Clear();
                int step = posA.x < posB.x ? 1 : -1;
                for (int x = posA.x; x != corner2.x + step; x += step)
                    tempPath.Add(new Vector2Int(x, posA.y));
                step = corner2.y < posB.y ? 1 : -1;
                for (int y = corner2.y; y != posB.y + step; y += step)
                    tempPath.Add(new Vector2Int(posB.x, y));
            }
            return true;

        }

        return false;
    }

    // Kiểm tra kết nối với 2 góc vuông (bằng 1 góc trung gian)
    bool CanConnectTwoCorners(Vector2Int posA, Vector2Int posB, bool savePath = false)
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
                if (savePath)
                {
                    tempPath.Clear();
                    CanConnectOneCorner(posA, corner, true); // Lưu path từ A đến corner
                    List<Vector2Int> firstHalf = new List<Vector2Int>(tempPath);
                    CanConnectOneCorner(corner, posB, true); // Lưu path từ corner đến B
                    tempPath.InsertRange(0, firstHalf.Take(firstHalf.Count - 1)); // Gộp, bỏ trùng
                }
                return true;
            }
        }

        return false;
    }


    // Kiểm tra kết nối với trường hợp biên
    bool CanConnectViaBoundary(Vector2Int posA, Vector2Int posB, bool savePath = false)
    {
        int maxRow = boardManager.rows;
        int maxCol = boardManager.cols;

        (Vector2Int boundaryPointA, Vector2Int boundaryPointB)[] boundaries = new[]
        {
            (new Vector2Int(posA.x, -1), new Vector2Int(posB.x, -1)), // Biên trái
            (new Vector2Int(posA.x, maxCol), new Vector2Int(posB.x, maxCol)), // Biên phải
            (new Vector2Int(-1, posA.y), new Vector2Int(-1, posB.y)), // Biên trên
            (new Vector2Int(maxRow, posA.y), new Vector2Int(maxRow, posB.y)) // Biên dưới
        };

        foreach (var (boundaryPointA, boundaryPointB) in boundaries)
        {
            if (CanConnectStraight(posA, boundaryPointA) && 
                CanConnectStraight(boundaryPointB, posB))
            {
                if (savePath)
                {
                    tempPath.Clear();
                    // Lưu đoạn từ posA đến biên
                    CanConnectStraight(posA, boundaryPointA, true);
                    List<Vector2Int> firstHalf = new List<Vector2Int>(tempPath);

                    // Lưu đoạn trên biên
                    List<Vector2Int> boundaryPath = new List<Vector2Int>();
                    if (boundaryPointA.x == boundaryPointB.x) // Biên trái/phải (x cố định, y thay đổi)
                    {
                        int step = posA.y < posB.y ? 1 : -1;
                        for (int y = posA.y; y != posB.y + step; y += step)
                            boundaryPath.Add(new Vector2Int(boundaryPointA.x, y));
                    }
                    else // Biên trên/dưới (y cố định, x thay đổi)
                    {
                        int step = posA.x < posB.x ? 1 : -1;
                        for (int x = posA.x; x != posB.x + step; x += step)
                            boundaryPath.Add(new Vector2Int(x, boundaryPointA.y));
                    }

                    // Lưu đoạn từ biên đến posB
                    CanConnectStraight(boundaryPointB, posB, true);
                    List<Vector2Int> secondHalf = new List<Vector2Int>(tempPath);

                    // Gộp các đoạn
                    tempPath.Clear();
                    tempPath.AddRange(firstHalf.Take(firstHalf.Count - 1));
                    tempPath.AddRange(boundaryPath);
                    tempPath.AddRange(secondHalf.Skip(1));
                }
                return true;
            }
        }

        return false;
    }

    #endregion
    
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
                    if (CanConnect(group[i], group[j]))
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
}
