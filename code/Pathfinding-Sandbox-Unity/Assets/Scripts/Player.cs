/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using dk.Singleton;

using PFS.Scene;

using UnityEngine;

/// <summary>
/// Player script.
/// </summary>
public class Player : MonoBehaviour {
    public NavGrid Grid { get => _grid; set => _grid = value; }
    
    [SerializeField]
    private NavGrid _grid;

    public float Speed { get => _speed; }

    [SerializeField]
    private float _speed = 20.0f;

    PFSMainScene _mainScene = null;

    /// <summary>
    /// Starts this instance.
    /// </summary>
    private void Start() {
        _mainScene = Singleton.GetInstance<PFSMainScene>();
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
}
