using UnityEngine;

public class CameraController : MonoBehaviour
{
    public BoardManager boardManager;
    public float padding = 1.5f;

    private void Start()
    {
        CenterCameraOnBoard();
    }

    void CenterCameraOnBoard()
    {
        if (boardManager == null) return;

        // Lấy kích thước Board
        int rows = boardManager.rows;
        int cols = boardManager.cols;
        float tileSize = boardManager.tileSize;

        // Tính vị trí trung tâm của Board
        float centerX = (cols - 1) * tileSize / 2f;
        float centerY = -(rows - 1) * tileSize / 2f;

        transform.position = new Vector3(centerX, centerY, -10f);

        // Cập nhật kích thước Camera
        float boardHeight = rows * tileSize;
        float boardWidth = cols * tileSize;
        Camera.main.orthographicSize = Mathf.Max(boardHeight, boardWidth) / (tileSize * 2) * padding;
    }
}
