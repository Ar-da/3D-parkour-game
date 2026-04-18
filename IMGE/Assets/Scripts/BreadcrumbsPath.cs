using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BreadcrumbsPath : MonoBehaviour
{
    [Header("Marker Configuration")]
    [SerializeField] private GameObject[] markers;
    [SerializeField] private Vector3 inactivePosition;
    [SerializeField] private float markerDistance = 5.0f;
    [SerializeField] private int skipAFewMarkers = 2;
    [Header("Agent Configuration")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;

    private NavMeshPath _currentPath;
    private NavMeshAgent _navAgent;
    private NavMeshHit playerNavMeshPos;
    private NavMeshHit targetNavMeshPos;

    // Start is called before the first frame update
    private void Start()
    {
        _currentPath = new NavMeshPath();
        playerNavMeshPos = new NavMeshHit();
        targetNavMeshPos = new NavMeshHit();
    }

    // Update is called once per frame
    private void Update()
    {
        // ...
        if (!NavMesh.SamplePosition(player.position, out playerNavMeshPos, 5.0f, NavMesh.AllAreas)) return;
        if (!NavMesh.SamplePosition(target.position, out targetNavMeshPos, 5.0f, NavMesh.AllAreas)) return;
        bool foundPath = NavMesh.CalculatePath(playerNavMeshPos.position, targetNavMeshPos.position, NavMesh.AllAreas, _currentPath);
        
        if (!foundPath || _currentPath.corners == null || _currentPath.corners.Length < 2) return;
        for (var i = 0; i < _currentPath.corners.Length - 1; ++i)
        {
            Debug.DrawLine(_currentPath.corners[i], _currentPath.corners[i + 1], Color.red);
        }
        UpdateMarkers(_currentPath.corners);
    }

    private void UpdateMarkers(Vector3[] path)
    {
        var potentialMarkerPositions = new List<Vector3>();
        var rest = 0.0f;
        var from = Vector3.zero;
        var to = Vector3.zero;

        // ...
        for (var i = 0; i < path.Length - 1; ++i)
        {
            from = path[i];
            to = path[i+1];
            var length = (to - from).magnitude;
            var remainingDistance = length - rest;
            while (remainingDistance > markerDistance)
            {
                remainingDistance -= markerDistance;
                potentialMarkerPositions.Add(from + (to - from).normalized * (length - remainingDistance));
            }
            rest = remainingDistance;
        }

        for (int i = 0; i < markers.Length; i++)
        {
            if (potentialMarkerPositions.Count > i + skipAFewMarkers)
            {
                markers[i].transform.position = SnapToNavMesh(potentialMarkerPositions[i + skipAFewMarkers], 2f, 0.05f);
            }
            else
            {
                markers[i].transform.position = inactivePosition;
            }
        }
        
        // ...
    }
    
    private Vector3 SnapToNavMesh(Vector3 worldPos, float maxDistance = 2f, float yOffset = 0.05f)
    {
        if (NavMesh.SamplePosition(worldPos, out var hit, maxDistance, NavMesh.AllAreas))
        {
            var p = hit.position;
            p.y += yOffset;
            return p;
        }

        // fallback: unverändert
        worldPos.y += yOffset;
        return worldPos;
    }
    
}
