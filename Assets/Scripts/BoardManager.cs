using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class BoardManager : MonoBehaviourPunCallbacks
{
    public int rows { get; private set; }
    public int cols { get; private set; }
    public float tileSize = 2f;
    public GameObject tilePrefab;
    private GameLogic gameLogic;
    private LineRenderer lineRenderer;

    [Tooltip("Time for displaying lines connection")]
    public float delayTime = 0.5f;

    public List<Sprite> tileSprites;

    private List<int> availableIDs = new List<int>();
    private Dictionary<Vector2Int, Tile> tileDict = new Dictionary<Vector2Int, Tile>();
    private Dictionary<int, List<Tile>> tileGroups = new Dictionary<int, List<Tile>>();

    private void Start()
    {
        gameLogic = FindObjectOfType<GameLogic>();
        GenerateAvailableIDs();

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.useWorldSpace = true; // Vẽ bằng tọa độ thế giới
    }

    public void SetBoardSize(int newRows, int newCols)
    {
        rows = newRows;
        cols = newCols;
    }

    // Khởi tạo Board
    public void GenerateBoard()
    {
        // Xóa bảng cũ nếu có
        foreach (var tile in tileDict.Values)
        {
            Destroy(tile.gameObject);
        }
        tileDict.Clear();
        tileGroups.Clear();
        availableIDs.Clear();

        GenerateAvailableIDs();

        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector2Int gridPos = new Vector2Int(row, col);

                // Do trục Oxy trong Engine nghịch với cách duyệt matrix thông thường
                Vector3 position = new Vector3(col * tileSize, -row * tileSize, 0); 
                GameObject gameObject = Instantiate(tilePrefab, position, Quaternion.identity, transform);

                // Khởi tạo tile
                Tile tile = gameObject.GetComponent<Tile>();

                int tileID = availableIDs[index];
                tile.SetTile(tileID, tileSprites[tileID]);
                tile.gridPosition = gridPos;
                tileDict[gridPos] = tile; // Save to dictionary

                index++;
            }
        }
    }

    // Khởi tạo danh sách ID cho tile
    void GenerateAvailableIDs()
    {
        int totalTiles = rows * cols;
        int pairs = totalTiles / 4; // Chia 4 để ra 2 cặp

        for (int i = 0; i < pairs; i++)
        {
            availableIDs.Add(i);
            availableIDs.Add(i);
            availableIDs.Add(i);
            availableIDs.Add(i);
        }

        availableIDs.Shuffle();
    }

    // Check xem tile có tồn tại với Key là Position ?
    public bool HasTile(Vector2Int pos)
    {
        return tileDict.ContainsKey(pos);
    }

    public void ReleaseTile(Vector2Int pos)
    {
        try
        {
            if (tileDict.ContainsKey(pos))
            {
                if (tileDict[pos] != null && tileDict[pos].gameObject != null)
                {
                    Destroy(tileDict[pos].gameObject);
                }
                tileDict.Remove(pos);
            }
            if (PhotonNetwork.IsConnected && photonView != null)
            {
                photonView.RPC("RPC_ReleaseTile", RpcTarget.Others, pos.x, pos.y);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error releasing tile at {pos}: {e.Message}\n{e.StackTrace}");
        }
    }

    public List<Tile> GetRemainingTiles()
    {
        return new List<Tile>(tileDict.Values);
    }

    public Dictionary<int, List<Tile>> GetTileGroups()
    {
        return tileGroups;
    }

    public void UpdateTileGroups()
    {
        tileGroups.Clear();

        // Create a copy of dictionary values
        List<Tile> tileValues = new List<Tile>(tileDict.Values);

        foreach (var tile in tileValues)
        {
            // Check if tile is valid
            if (tile == null) continue;

            if (!tileGroups.ContainsKey(tile.tileId))
            {
                tileGroups[tile.tileId] = new List<Tile>();
            }
            tileGroups[tile.tileId].Add(tile);
        }
    }

    public void ShuffleTiles()
    {

        // Reset Hint trước khi Shuffle
        gameLogic.ResetHint();

        List<Tile> remainingTiles = new List<Tile>(tileDict.Values);

        if (remainingTiles.Count <= 1) return;

        // Lấy vị trí cũ của các tiles và shuffle
        List<Vector2Int> position = new List<Vector2Int>(tileDict.Keys);
        position.Shuffle();

        Dictionary<Vector2Int, Tile> newTileDict = new Dictionary<Vector2Int, Tile>();

        // Cập nhật vị trí mới cho tiles
        for (int i = 0; i < remainingTiles.Count; i++)
        {
            Vector2Int newPos = position[i];
            Tile tile = remainingTiles[i];

            tile.transform.position = new Vector3(newPos.y * tileSize, -newPos.x * tileSize, 0);
            tile.gridPosition = newPos;

            newTileDict[newPos] = tile;
        }

        tileDict = newTileDict;
        Debug.Log("Tile shuffled");
    }

    // Hàm chuyển đổi Vector2 sang World Position
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.y * tileSize, -gridPos.x * tileSize, 0);
    }

    // Vẽ line giữa các điểm trong path
    public void DrawPath(List<Vector2Int> path)
    {

        if (lineRenderer == null || path.Count < 2) return;

        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            Debug.Log("Drawing Path: " + string.Join(" -> ", path));

            lineRenderer.SetPosition(i, GridToWorldPosition(path[i]));
        }

        StartCoroutine(HideLineAfterDelay(delayTime));
    }

    private IEnumerator HideLineAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        lineRenderer.positionCount = 0; // Hide line
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < rows && pos.y >= 0 && pos.y < cols;
    }

    public void SendBoardToClients()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (tileDict.Count == 0)
        {
            Debug.LogError("tileDict is empty! Board has not been generated yet.");
            return;
        }

        Debug.Log($"Sending board to clients. Rows: {rows}, Cols: {cols}, Tile count: {tileDict.Count}");

        // Gửi kích thước board
        photonView.RPC("RPC_SetBoardSize", RpcTarget.Others, 10, 16);

        // Gửi danh sách tile
        List<Vector2Int> positions = new List<Vector2Int>(tileDict.Keys);
        int[] posX = new int[positions.Count];
        int[] posY = new int[positions.Count];
        int[] tileIDs = new int[positions.Count];

        for (int i = 0; i < positions.Count; i++)
        {
            posX[i] = positions[i].x;
            posY[i] = positions[i].y;
            tileIDs[i] = tileDict[positions[i]].tileId;
        }

        photonView.RPC("RPC_SyncBoard", RpcTarget.Others, posX, posY, tileIDs);
    }

    [PunRPC]
    public void RPC_SetBoardSize(int newRows, int newCols) {
        rows = newRows;
        cols = newCols;
    }

    [PunRPC]
    public void RPC_SyncBoard(int[] posX, int[] posY, int[] tileIDs)
    {
        // Xóa board cũ nếu có
        foreach (var tile in tileDict.Values)
        {
            Destroy(tile.gameObject);
        }
        tileDict.Clear();
        tileGroups.Clear();

        // Tái tạo board từ dữ liệu nhận được
        for (int i = 0; i < posX.Length; i++)
        {
            Vector2Int gridPos = new Vector2Int(posX[i], posY[i]);
            Vector3 position = new Vector3(gridPos.y * tileSize, -gridPos.x * tileSize, 0);
            GameObject gameObject = Instantiate(tilePrefab, position, Quaternion.identity, transform);

            Tile tile = gameObject.GetComponent<Tile>();
            int tileID = tileIDs[i];
            tile.SetTile(tileID, tileSprites[tileID]);
            tile.gridPosition = gridPos;
            tileDict[gridPos] = tile;
        }

        // Cập nhật camera sau khi board được tạo
        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            cameraController.CenterCameraOnBoard();
        }
        Debug.Log("Board synced on client");
    }

    [PunRPC]
    public void RPC_ReleaseTile(int x, int y)
    {
        try
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (tileDict.ContainsKey(pos))
            {
                // Add null check before destroying
                if (tileDict[pos] != null && tileDict[pos].gameObject != null)
                {
                    Destroy(tileDict[pos].gameObject);
                }
                tileDict.Remove(pos);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in RPC_ReleaseTile: {e.Message}\n{e.StackTrace}");
        }
    }
}
