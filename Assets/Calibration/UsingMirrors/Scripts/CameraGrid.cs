using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraGrid : MonoBehaviour
{

    Camera _camera;

    public int focalLength = 5;
    public int size = 10;

    private void OnDrawGizmos()
    {
        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }
        Gizmos.color = Color.gray;
        for (int x = 0; x <= size - 1; ++x)
        {
            Vector3 from = _camera.ViewportToWorldPoint(new Vector3(x / (float)(size - 1), 0, focalLength));
            Vector3 to = _camera.ViewportToWorldPoint(new Vector3(x / (float)(size - 1), 1, focalLength));
            Gizmos.DrawLine(from, to);
        }
        for (int y = 0; y <= size - 1; ++y)
        {
            Vector3 from = _camera.ViewportToWorldPoint(new Vector3(0, y / (float)(size - 1), focalLength));
            Vector3 to = _camera.ViewportToWorldPoint(new Vector3(1, y / (float)(size - 1), focalLength));
            Gizmos.DrawLine(from, to);
        }
        Gizmos.color = Color.blue;
        {
            Vector3 from = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0, focalLength));
            Vector3 to = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1, focalLength));
            Gizmos.DrawLine(from, to);
        }
        {
            Vector3 from = _camera.ViewportToWorldPoint(new Vector3(0,0.5f, focalLength));
            Vector3 to = _camera.ViewportToWorldPoint(new Vector3(1,0.5f, focalLength));
            Gizmos.DrawLine(from, to);
        }
        Gizmos.color = Color.white;
        {
            Vector3 from = transform.TransformPoint(new Vector3(0, -1, focalLength));
            Vector3 to = transform.TransformPoint(new Vector3(0, 1,focalLength));
            Gizmos.DrawLine(from, to);
        }
        {
            Vector3 from = transform.TransformPoint(new Vector3(-1, 0, focalLength));
            Vector3 to = transform.TransformPoint(new Vector3(1, 0, focalLength));
            Gizmos.DrawLine(from, to);
        }
    }
}
