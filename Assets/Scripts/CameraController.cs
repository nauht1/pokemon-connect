using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public BoardManager boardManager;
    public float padding = 1.5f;
    
    // Event to notify when the camera is ready
    public static event Action OnCameraReady;
    
    private bool hasNotifiedReady = false;

    private void Start()
    {
        if (boardManager != null)
        {
            CenterCameraOnBoard();
        }
        else
        {
            // Try to find BoardManager if not assigned
            boardManager = FindObjectOfType<BoardManager>();
            if (boardManager != null)
            {
                CenterCameraOnBoard();
            }
        }
    }
    
    private void LateUpdate()
    {
        if (!hasNotifiedReady && boardManager != null && 
            boardManager.rows > 0 && boardManager.cols > 0)
        {
            // Wait for a few frames for everything to be rendered
            Invoke("NotifyCameraReady", 2.0f);
            hasNotifiedReady = true;
        }
    }
    
    private void NotifyCameraReady()
    {
        OnCameraReady?.Invoke();
    }

    public void CenterCameraOnBoard()
    {
        if (boardManager == null) return;
        if (boardManager.rows <= 0 || boardManager.cols <= 0) return;

        // Calculate the position
        float centerX = (boardManager.cols - 1) * boardManager.tileSize / 2f;
        float centerY = -(boardManager.rows - 1) * boardManager.tileSize / 2f;
        
        // Position the camera
        transform.position = new Vector3(centerX, centerY, -10f);
        
        // Set the orthographic size based on the board dimensions
        float boardHeight = boardManager.rows * boardManager.tileSize;
        float boardWidth = boardManager.cols * boardManager.tileSize;
        Camera.main.orthographicSize = Mathf.Max(boardHeight, boardWidth) / (boardManager.tileSize * 2) * padding;
        
        Debug.Log($"Camera centered at {centerX}, {centerY} with size {Camera.main.orthographicSize}");
    }
}
