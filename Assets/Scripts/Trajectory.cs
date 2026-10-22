using UnityEngine;

public class Trajectory : MonoBehaviour
{
    private LineRenderer lineRenderer;

    public int numPoints = 30;          // Number of dots/segments in the line
    public float timeStep = 0.1f;       // Time interval between points

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Draw(Vector3 origin, Vector3 startVelocity)
    {
        lineRenderer.positionCount = numPoints;
        Vector3[] points = new Vector3[numPoints];

        for (int i = 0; i < numPoints; i++)
        {
            float t = i * timeStep;
            // Calculate the point along the trajectory
            Vector3 point = origin + startVelocity * t + 0.5f * Physics.gravity * t * t;
            points[i] = point;

            // Don't draw if trajectory is under the map
            if (point.y < -0.5f)
            {
                lineRenderer.positionCount = i + 1;
                break;
            }
        }

        lineRenderer.SetPositions(points);
    }

    public void Hide()
    {
        // set number of points to zero to remove the trajectory
        lineRenderer.positionCount = 0;
    }
}
