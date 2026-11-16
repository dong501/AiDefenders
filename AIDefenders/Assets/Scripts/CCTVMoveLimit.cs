using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CCTVMoveLimit : MonoBehaviour
{
    [Tooltip("Prevent the drone from crossing this volume, including its next physics step.")]
    public bool hardBoundary;

    public Vector3 ClampFlightPosition(Vector3 worldPosition, float clearance = .5f)
    {
        var box = Volume as BoxCollider;
        if (box == null) return Volume.ClosestPoint(worldPosition);
        Vector3 point = box.transform.InverseTransformPoint(worldPosition) - box.center;
        Vector3 half = box.size * .5f;
        Vector3 scale = box.transform.lossyScale;
        for (int axis = 0; axis < 3; axis++)
        {
            float extent = Mathf.Max(0, half[axis] - clearance / Mathf.Max(.001f, Mathf.Abs(scale[axis])));
            point[axis] = Mathf.Clamp(point[axis], -extent, extent);
        }
        return box.transform.TransformPoint(point + box.center);
    }

    public Collider Volume
    {
        get
        {
            if (cached == null)
                cached = GetComponent<Collider>();
            return cached;
        }
    }

    Collider cached;
}
