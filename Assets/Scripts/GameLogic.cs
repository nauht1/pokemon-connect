using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    private Tile firstTile = null;
    private Tile secondTile = null;
    private BoardManager boardManager;

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
                Debug.Log("Hit");
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

            if (CanConnect(firstTile, secondTile)) // Kiểm tra xem hai tile có thể kết nối không
            {
                firstTile.HideTile();
                secondTile.HideTile();

                boardManager.ReleaseTile(firstTile.gridPosition);
                boardManager.ReleaseTile(secondTile.gridPosition);
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

    bool CanConnect(Tile tileA, Tile tileB)
    {

        Vector2Int posA = tileA.gridPosition;
        Vector2Int posB = tileB.gridPosition;

        Debug.Log($"TileID: {tileA.tileId} PosA: {posA} ");
        Debug.Log($"TileID: {tileB.tileId} PosB: {posB} ");

        if (tileA.tileId != tileB.tileId) return false;

        if (IsSameBoundary(posA, posB)) return true;

        if (CanConnectStraight(posA, posB)) return true;

        if (CanConnectOneCorner(posA, posB)) return true;

        if (CanConnectViaBoundary(posA, posB)) return true;

        if (CanConnectTwoCorners(posA, posB)) return true;

        return false;
    }

    bool IsSameBoundary(Vector2Int posA, Vector2Int posB)
    {
        int maxRow = boardManager.rows - 1;
        int maxCol = boardManager.cols - 1;

        bool sameTopEdge = posA.x == 0 && posB.x == 0;
        bool sameBottomEdge = posA.x == maxRow && posB.x == maxRow;
        bool sameLeftEdge = posA.y == 0 && posB.y == 0;
        bool sameRigtEdge = posA.y == maxCol && posB.y == maxCol;

        return sameTopEdge || sameBottomEdge || sameLeftEdge || sameRigtEdge;
    }

    // Kiểm tra kết nối trực tiếp
    bool CanConnectStraight(Vector2Int posA, Vector2Int posB)
    {
        Debug.Log("One Straight");
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
        Debug.Log("One Corner");
        Vector2Int corner1 = new Vector2Int(posA.x, posB.y);
        Vector2Int corner2 = new Vector2Int(posB.x, posA.y);
        Debug.Log($"Corner 1: {corner1}");
        Debug.Log($"Corner 2: {corner2}");
        if (!boardManager.HasTile(corner1) && CanConnectStraight(posA, corner1) && CanConnectStraight(corner1, posB))
        {
            return true;

        }

        if (!boardManager.HasTile(corner2) && CanConnectStraight(posA, corner2) && CanConnectStraight(corner2, posB))
        {
            return true;

        }

        return false;
    }

    // Kiểm tra kết nối với 2 góc vuông (bằng 1 góc trung gian)
    bool CanConnectTwoCorners(Vector2Int posA, Vector2Int posB)
    {
        for (int x = 0; x < boardManager.rows; x++)
        {
            for (int y = 0; y < boardManager.cols; y++)
            {
                Vector2Int corner = new Vector2Int(x, y);
                if (!boardManager.HasTile(corner) && CanConnectOneCorner(posA, corner) && CanConnectOneCorner(corner, posB))
                    return true;
            }
        }
        return false;
    }

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
}
