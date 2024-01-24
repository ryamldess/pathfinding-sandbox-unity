/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using dk.Singleton;
using dk.Math.Spline;

using System.Collections;
using System.Collections.Generic;

using PFS.Math.AI.Wayfinding;
using PFS.Scene;

using UnityEngine;

/// <summary>
/// NPC script.
/// </summary>
public class NPC : MonoBehaviour {
    public NavGrid Grid { get => _grid; set => _grid = value; }

    public float Speed { get => _speed; }

    [SerializeField]
    protected float _speed = 20.0f;

    [SerializeField]
    private NavGrid _grid;

    [SerializeField]
    protected Player _player = null;

    private AStar2D<int> _astar = null;
    private Vector3 _currentDestination = default(Vector3);
    private PFSMainScene _mainScene = null;
    private SplineController _splineController = null;

    /// <summary>
    /// Starts this instance.
    /// </summary>
    private void Start() {
        _astar = new AStar2D<int>();
        _splineController = GetComponent<SplineController>();

        StartCoroutine("WaitForMainScene");
    }

    private IEnumerator WaitForMainScene() {
        yield return new WaitForSeconds(0.5f + UnityEngine.Random.Range(0.0f, 0.5f));

        _mainScene = Singleton.GetInstance<PFSMainScene>();
        InvokeRepeating("ChasePlayer", 0.0f, 0.5f);
    }

    /// <summary>
    /// Handles triggers for this object.
    /// </summary>
    /// <param name="other">The Collider component of the other GameObject.</param>
    private void OnTriggerEnter(Collider other) {
        if (other is BoxCollider) {
            //Debug.Log("Colliding with building.");
            if (_mainScene != null) _mainScene.RecalculateCurrentPath();
        }
    }

    private void ChasePlayer() {
        if (_mainScene.StreetScene == null) return;
        if (transform.position == _player.transform.position) return;

        int width = (int)_mainScene.NavPlane.transform.localScale.x * 10; // We're using a plane, so we have to multiply by 10 to get 1m-scale values.
        int depth = (int)_mainScene.NavPlane.transform.localScale.z * 10; // We're using a plane, so we have to multiply by 10 to get 1m-scale values.
        Vector3 startingIndexVec = new Vector3((int)((width / 2) - transform.position.x), 0.0f, (int)((depth / 2) + transform.position.z));
        Vector3 destinationIndexVec = new Vector3((int)((width / 2) - _player.transform.position.x), 0.0f, (int)((depth / 2) + _player.transform.position.z));

        FindPath(startingIndexVec, destinationIndexVec);
    }

    private void FindPath(Vector3 startingIndexVec, Vector3 destinationIndexVec) {
        if (_splineController != null) StopCoroutine(_splineController.FollowSpline());
        _currentDestination = destinationIndexVec;

        // Find the fastest route to the destination
        _astar.DistanceFunction = DistanceFunction.Chebyshev;
        List<Vector2> twoDPath = _astar.Process(_mainScene.StreetScene.SceneMetrics.navGridWeights, startingIndexVec, destinationIndexVec);

        if (twoDPath.Count <= 1) return;

        List<Vector3> threeDPath = _mainScene.ConvertTo3DPath(twoDPath, 1.01f);

        /* Try to cull extraneous points in the path. We do 
         * two passes, one with Bresenham's algorithm and one with 
         * collinearity. */
        new Collinearity3D().Process(ref threeDPath);

        threeDPath = _mainScene.ConvertToWorldSpace(threeDPath);

        // Traverse the final path via SQUAD splines to smooth it out
        TraverseSplinePath(threeDPath);
    }

    private void TraverseSplinePath(List<Vector3> path) {
        _splineController = _mainScene.CreateSplines(path, gameObject);

        StartCoroutine(_splineController.FollowSpline());
    }
}
