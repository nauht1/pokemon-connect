using Photon.Pun;
using System.Collections;
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

    private List<Vector2Int> tempPath = new List<Vector2Int>();

    private AudioManager audioManager;
    private bool cameraIsReady = false;
    private float startupTime;

    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        startupTime = Time.time;
        cameraIsReady = false;

        // Subscribe to camera ready event
        CameraController.OnCameraReady += OnCameraReady;

        // If we're not the host, we should wait longer
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            // Non-host clients will wait for OnCameraReady event
            Debug.Log("Non-host client will wait for camera setup");
        }
        else
        {
            // For host or single player, still wait a short time
            StartCoroutine(SetCameraReady(1.0f));
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe when destroyed
        CameraController.OnCameraReady -= OnCameraReady;
    }

    private void OnCameraReady()
    {
        Debug.Log("Received OnCameraReady event - GameLogic now accepting input");
        cameraIsReady = true;
    }

    private IEnumerator SetCameraReady(float delay)
    {
        yield return new WaitForSeconds(delay);
        cameraIsReady = true;
        Debug.Log("GameLogic now accepting input after delay");
    }

    private void Update()
    {
        // Wait longer for built non-host clients (3 seconds)
        float waitTime = (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) ? 3.0f : 1.0f;

        // Skip processing if camera isn't ready or we're still in startup time
        if (Time.time - startupTime < waitTime || !cameraIsReady)
        {
            return;
        }

        if (!Application.isFocused)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main == null) return;

            Vector3 mousePos3D = Input.mousePosition;

            float safeMargin = 5f; // 5-pixel safety margin
            if (mousePos3D.x < safeMargin || mousePos3D.x > Screen.width - safeMargin ||
                mousePos3D.y < safeMargin || mousePos3D.y > Screen.height - safeMargin)
            {
                return; // Skip processing clicks outside screen bounds or too close to the edge
            }
            try
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(mousePos3D);
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
            catch (System.Exception e)
            {
                Debug.LogWarning($"Exception during mouse input processing: {e.Message}");
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
                // Play connect audio
                audioManager.PlaySFX(audioManager.connect);

                boardManager.DrawPath(tempPath);
                firstTile.HideTile();
                secondTile.HideTile();

                boardManager.ReleaseTile(firstTile.gridPosition);
                boardManager.ReleaseTile(secondTile.gridPosition);

                if (GameManager.Instance.GetCurrentGameMode() == GameManager.GameMode.Endless)
                {
                    GameManager.Instance.AddScoreEndless();
                }
                else if (GameManager.Instance.GetCurrentGameMode() == GameManager.GameMode.Multiplayer)
                {
                    int playerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                    GameManager.Instance.AddScore(playerActorNumber);
                }

                CheckMoves(false);
            }
            else
            {
                // Deselected
                audioManager.PlaySFX(audioManager.wrongConnect);
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
        try
        {
            boardManager.UpdateTileGroups();
            var tileGroups = boardManager.GetTileGroups();

            if (tileGroups == null || tileGroups.Count == 0)
            {
                GameManager.Instance.ShowCongrats();
                return;
            }

            foreach (var entry in tileGroups)
            {
                var group = entry.Value;
                if (group == null) continue;

                // Make a safe copy to avoid issues if the collection changes
                List<Tile> safeCopy = new List<Tile>(group);
                int count = safeCopy.Count;

                for (int i = 0; i < count - 1; i++)
                {
                    if (safeCopy[i] == null) continue;

                    for (int j = i + 1; j < count; j++)
                    {
                        if (safeCopy[j] == null) continue;

                        if (CanConnect(safeCopy[i], safeCopy[j]))
                        {
                            if (findHint && numsOfHint > 0 && GameManager.Instance.GetCurrentGameMode() == GameManager.GameMode.Endless)
                            {
                                numsOfHint -= 1;
                                safeCopy[i].HighlightTile();
                                safeCopy[j].HighlightTile();
                            }
                            return;
                        }
                    }
                }
            }

            // If we got here with no tiles, show congrats
            GameManager.Instance.ShowCongrats();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in CheckMoves: {e.Message}\n{e.StackTrace}");
        }
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
