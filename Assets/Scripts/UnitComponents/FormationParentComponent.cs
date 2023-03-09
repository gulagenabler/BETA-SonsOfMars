using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FormationParentComponent : MonoBehaviour {
    public MovementComponent.FormationType formationType;
    public Vector3 size;
    MovementComponent movementComponent;
    const float SQRDistanceToFlip = 25; // 5 * 5
    bool flip = false;

    private void Start() {
        movementComponent = GetComponent<MovementComponent>();
        movementComponent.OnReachedDestinationEvent += OnReachedDestination;
    }

    private void OnReachedDestination()
    {
        transform.rotation = movementComponent.GetMoveTarget().rotation;
        //int childCount = transform.childCount;
        //try
        //{
        //    for (int i = 0; i < childCount; ++i)
        //    {
        //        Transform child = transform.GetChild(i);
        //        if (child)
        //            child.SetParent(null, true);
        //    }
        //}
        //catch (System.Exception e) { Debug.Log(e); }
        //Destroy(this, 10f);
    }

    bool finalUpdate = false;
    private void FixedUpdate()
    {
        if (finalUpdate) return;
        if (movementComponent == null || movementComponent.GetMoveTarget() == null) return;
        {
            if ((transform.position - movementComponent.GetMoveTarget().position).sqrMagnitude < SQRDistanceToFlip) {
                flip = true;
                finalUpdate = true;
                Vector3 avgPos = Vector3.zero;
                foreach (Transform child in transform)
                {
                    avgPos += child.position;
                }
                avgPos = avgPos / transform.childCount;
                UpdateFormation(formationType, avgPos);
            }
        }
        //if (movementComponent == null || movementComponent.GetMoveTarget() == null) return;
        //if (Input.GetKeyDown(KeyCode.Alpha0))
        //{
        //    transform.SetPositionAndRotation(movementComponent.GetMoveTarget().position, movementComponent.GetMoveTarget().rotation);
        //
        //    Debug.Break();
        //}
        //
        //    foreach (Transform c in transform)
        //{
        //    ulong id = ulong.Parse(c.name.Split()[2]);
        //    UnitBase u = UnitBase.units[id];
        //    Debug.DrawLine(u.GetComponent<MovementComponent>().GetMoveTarget().position, u.transform.position, Color.red);
        //}
    }

    Vector3 centerPos;
    public void UpdateFormation(MovementComponent.FormationType newFormation, Vector3 avgPos)
    {
        formationType = newFormation;
        centerPos = avgPos;
        UpdateFormation();
    }

    private void NoFormation() {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.localPosition = Vector3.zero;
            child.localEulerAngles = Vector3.zero;
            child.localScale = Vector3.one;
        }
    }

    private void SwapSibling(Transform a, Transform b, bool updateTransform = false) {
        int aIndex = a.GetSiblingIndex();
        a.SetSiblingIndex(b.GetSiblingIndex());
        b.SetSiblingIndex(aIndex);
        if (updateTransform) {
            Vector3 localPosition = a.localPosition;
            Quaternion localRotation = a.localRotation;
            Vector3 localScale = a.localScale;
            a.localPosition = b.localPosition;
            a.localRotation = b.localRotation;
            a.localScale = b.localScale;
            b.localPosition = localPosition;
            b.localRotation = localRotation;
            b.localScale = localScale;
        }
    }


    // Direction vectors
    static int[] dRow = { -1, 0, 1, 0 };
    static int[] dCol = { 0, 1, 0, -1 };

    void ProcessBFS(Vector2Int node, Vector2Int max, int count, ref HashSet<Transform> executedTransforms)
    {
        try
        {
            // Do something with the node
            int i = Mathf.Clamp(node.x + node.y * max.y, 0, count - 1);
            if (!executedTransforms.Contains(transform.GetChild(i)))
            {
                float closestDistanceSqr = float.PositiveInfinity;
                UnitBase closestUnitBase = null;
                foreach (Transform c in transform)
                {
                    if (!executedTransforms.Contains(c))
                    {
                        string[] parts = c.name.Split();
                        UnitBase u = UnitBase.units[ulong.Parse(parts[2])];
                        float compareSqr = (u.transform.position - c.transform.position).sqrMagnitude;
                        if (compareSqr > closestDistanceSqr)
                        {
                            closestDistanceSqr = compareSqr;
                            closestUnitBase = u;
                        }
                    }
                }

                if (closestUnitBase && closestUnitBase.TryGetComponent(out MovementComponent closestMovementComp))
                {
                    SwapSibling(transform.GetChild(i), closestMovementComp.GetMoveTarget(), true);
                }

                executedTransforms.Add(transform.GetChild(i));

            }
        }
        catch (UnityEngine.UnityException e)
        {
            Debug.LogException(e);
        }
    }

    void BFS(Vector2Int index, Vector2Int max, int count)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        HashSet<Transform> executedTransforms = new HashSet<Transform>();
        queue.Enqueue(index);

        while (queue.Count > 0)
        {
            Vector2Int node = queue.Dequeue();
            visited.Add(node);

            ProcessBFS(index, max, count, ref executedTransforms);

            // Go to the adjacent cells
            for (int i = 0; i < 4; i++)
            {
                int adjx = Mathf.Clamp(node.x + dRow[i], 0, max.x);
                int adjy = Mathf.Clamp(node.y + dCol[i], 0, max.y);

                Vector2Int adjNode = new Vector2Int(adjx, adjy);
                Transform adjTransform = null;
                int adjIndex = adjx + adjy + max.y;
                if (adjIndex >= 0 && adjIndex < count)
                {
                    adjTransform = transform.GetChild(adjIndex);
                }
                //Transform adjTransform = transform.GetChild(adjNode.x + adjNode.y * max.y);
                if (!visited.Contains(adjNode) && adjTransform != null)
                {
                    queue.Enqueue(adjNode);
                    visited.Add(adjNode);
                }
            }
        }
    }

    private Vector3 GetCenterOfChildren(Transform parentTransform)
    {
        // Initialize center position and child count
        Vector3 centerPosition = Vector3.zero;
        int childCount = 0;

        // Iterate through all child objects
        foreach (Transform child in parentTransform)
        {
            // Add the child's position to the center position
            centerPosition += child.position;
            childCount++;
        }

        // Divide the center position by the number of child objects to get the average position
        centerPosition /= childCount;

        return centerPosition;
    }

    public void MoveParentToCenter(Transform parentTransform)
    {
        // Get the center position of the child objects
        Vector3 centerPosition = GetCenterOfChildren(parentTransform);

        // Calculate the offset between the parent's position and the center position
        Vector3 offset = centerPosition - parentTransform.position;

        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;

        // Move all the child objects by the offset
        foreach (Transform child in parentTransform)
        {
            child.position -= offset;

            if (min.x > child.localPosition.x)
                min.x = child.localPosition.x;
            if (min.y > child.localPosition.y)
                min.y = child.localPosition.y;
            if (min.z > child.localPosition.z)
                min.z = child.localPosition.z;

            if (max.x < child.localPosition.x)
                max.x = child.localPosition.x;
            if (max.y < child.localPosition.y)
                max.y = child.localPosition.y;
            if (max.z < child.localPosition.z)
                max.z = child.localPosition.z;
        }
        size = max - min;
    }

    private void LineFormation()
    {
        int count = transform.childCount;
        // Set the number of rows
        int N = 1;
        if (count > 7)
        {
            N = 2;
        }
        if (count > 15)
        {
            N = 3;
        }
        if (count > 30)
        {
            N = 4;
        }

        int M = (count - 1) / N;

        int middleX = M / 2;
        int middleY = N / 2;

        int lastRow = (count - 1) / N;

        for (int i = 0; i < count; i++)
        {
            // Setting the position
            float _x = ((int)i % N);
            float _z = ((int)i / N);

            Transform child = transform.GetChild(i);
            if (flip == true) {
                child.localPosition = new Vector3(_z, 0, -_x);
            } else {
                child.localPosition = new Vector3(_x, 0, -_z);
            }
            child.localEulerAngles = Vector3.zero;
            child.localScale = Vector3.one;
        }

        BFS(new Vector2Int(middleX, middleY), new Vector2Int(M, N), count);
        MoveParentToCenter(transform);
        transform.position = centerPos;
    }

    private void UpdateFormation()
    {
        switch (formationType)
        {
            case MovementComponent.FormationType.NoFormation:
                NoFormation();
                break;
            case MovementComponent.FormationType.LineFormation:
                LineFormation();
                break;
            default:
                break;
        }
    }

    private void OnTransformChildrenChanged()
    {
        if (transform.childCount == 0)
        {
            Destroy(this);
        } else
        {
            UpdateFormation();
        }
    }

    private void OnDestroy()
    {
        if (gameObject)
            Destroy(this.gameObject);
    }
}