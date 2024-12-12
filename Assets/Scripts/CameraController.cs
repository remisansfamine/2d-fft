using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector3[] _wayPoints;
    [SerializeField] private int _waypointIndex = 0;

    [SerializeField] private float _speed = 20.0f;

    private void Start()
    {
        transform.position = _wayPoints[_waypointIndex];
    }

    public void GoLeft()
    {
        _waypointIndex = Mathf.Clamp(_waypointIndex - 1, 0, _wayPoints.Length - 1);
    }

    public void GoRight()
    {
        _waypointIndex = Mathf.Clamp(_waypointIndex + 1, 0, _wayPoints.Length - 1);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _wayPoints[_waypointIndex], (Vector3.Distance(transform.position, _wayPoints[_waypointIndex]) + 1.0f) * _speed * Time.deltaTime);
    }

}
