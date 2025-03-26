using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    public void DrawLine(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startWorldPos);
        lineRenderer.SetPosition(1, endWorldPos);
    }
}
