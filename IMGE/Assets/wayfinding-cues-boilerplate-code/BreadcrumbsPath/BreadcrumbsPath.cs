using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
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

    // Start is called before the first frame update
    private void Start()
    {
        _currentPath = new NavMeshPath();
    }

    // Update is called once per frame
    private void Update()
    {
        // ...

        UpdateMarkers(_currentPath.corners);
    }

    private void UpdateMarkers(Vector3[] path)
    {
        if (path.Length < 2)
        {
            return;
        }

        var potentialMarkerPositions = new List<Vector3>();
        var rest = 0.0f;
        var from = Vector3.zero;
        var to = Vector3.zero;

        // ...
        
        // ...
    }
}