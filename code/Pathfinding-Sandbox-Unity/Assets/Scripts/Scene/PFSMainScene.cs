/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using dk.Math.Spline;
using dk.Singleton;
using dk.Tools.Debug;

using PFS.Commands.Debug;
using PFS.ProcGen.StreetScene;
using PFS.Math.AI.Wayfinding;

using SnS.UI;
using SnS.UI.Debug;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using SMath = System.Math;

namespace PFS.Scene {
    /// <summary>
    /// Initializes the main scene in this project.
    /// </summary>
    public class PFSMainScene : MonoSingleton {
        private const float _DEFAULT_GIMBAL_VELOCITY = 35.0f;
        private const float _DEFAULT_ZOOM_MAGNITUDE = 20.0f;
        private const float _DEFAULT_MAXIMUM_TILT_ANGLE = 85.0f;

        #region Inspector properties

        [SerializeField]
        protected Camera _camera = null;

        [SerializeField]
        protected GameObject _cameraGimbal = null;

        [SerializeField]
        protected bool _bresenhamPruning = false;

        [SerializeField]
        protected bool _collinearityPruning = false;

        public bool DrawPathfindingAids { get => _drawPathfindingAids; set => _drawPathfindingAids = value; }

        [SerializeField]
        protected bool _drawPathfindingAids = false;

        public List<Material> Materials { get => _materials; }

        [SerializeField]
        protected List<Material> _materials = default(List<Material>);

        public Dictionary<string, Material> MaterialsByName { get => _materialsByName; }

        private Dictionary<string, Material> _materialsByName = default(Dictionary<string,Material>);

        public GameObject NavPlane { get => _navPlane; }
        
        [SerializeField]
        protected GameObject _navPlane = null;

        [SerializeField]
        protected Player _player = null;

        public GameObject SceneRoot { get => _sceneRoot; }

        [SerializeField]
        protected GameObject _sceneRoot = null;

        #endregion

        public StreetSceneProduct StreetScene { get => _streetScene; set => _streetScene = value; }

        private StreetSceneProduct _streetScene = null;
        
        private AStar2D<int> _astar = null;
        private Vector3 _originalCameraPos = default(Vector3);
        private Vector3 _originalCameraRotation = default(Vector3);
        private Vector3 _originalGimbalPos = default(Vector3);

        private Vector3 _currentDestination = default(Vector3);
        private NavGridPathNode[] _currentPointToPointPath = Array.Empty<NavGridPathNode>();

        private PFSUIScene _uiScene = null;
        private DebugUIController _debugUIController = null;
        private DebugConsole _debugConsole = null;

        private SplineController _splineController = null;
        private float _splineVelocity = 0.05f;

        /// <summary>
		/// Start this instance.
		/// </summary>
		protected sealed override void Start() {
            base.Start();

            PFSInitializationScene initScene = Singleton.GetInstance<PFSInitializationScene>();
            initScene.DebugCommandRegistrar.RegisterCommand("genst", new GenerateStreetSceneCommand(this));
            initScene.DebugCommandRegistrar.RegisterCommand("envis", new EnableVisualizationCommand(this));
            initScene.DebugCommandRegistrar.RegisterCommand("disvis", new DisableVisualizationCommand(this));

            if (_materials != null &&
                _materials.Count > 0) {
                _materialsByName = new Dictionary<string, Material>();
                
                foreach (Material mat in _materials) {
                    _materialsByName.Add(mat.name, mat);
                }
            }

            _astar = new AStar2D<int>();

            _originalCameraPos = _camera.transform.localPosition;
            _originalCameraRotation = _camera.transform.localRotation.eulerAngles;
            _originalGimbalPos = _cameraGimbal.transform.position;

            _uiScene = Singleton.GetInstance<PFSUIScene>();
            _debugUIController = _uiScene.DebugUIController;
            _debugConsole = _debugUIController.GetView(GlobalMVCProperties.DEBUG_CONSOLE_ID) as DebugConsole;

            InvokeRepeating("UpdateInput", 0.0f, GlobalProperties.UPDATE_INTERVAL_120_FPS);
        }

        /// <summary>
        /// Updates the scene after procedural generation of a new street scene.
        /// </summary>
        public void UpdateAfterStreetSceneGen() {
            ResetTraversal();
            ResetPlayerPosition();
        }

        /// <summary>
        /// Resets the player (and the camera).
        /// </summary>
        private void ResetPlayerPosition() {
            if (_streetScene != null) {
                List<Vector3> intersectionMetadata = _streetScene.SceneMetrics.intersectionMetadata;
                Vector3 playerPos = intersectionMetadata[intersectionMetadata.Count - 1];

                _player.gameObject.transform.position = new Vector3(playerPos.x, playerPos.y + 1, playerPos.z);
                _player.gameObject.transform.LookAt(_player.gameObject.transform.position + Vector3.back);

                Vector3 playerPosDelta = _player.gameObject.transform.position - Vector3.zero;
                Vector3 gimbalPosDelta = _originalGimbalPos + playerPosDelta;

                _camera.transform.localPosition = _originalCameraPos;
                _camera.transform.localRotation = Quaternion.Euler(_originalCameraRotation);
                _cameraGimbal.transform.position = new Vector3(gimbalPosDelta.x, _originalGimbalPos.y, gimbalPosDelta.z);
                _cameraGimbal.transform.rotation = Quaternion.Euler(Vector3.zero);
            } else {
                _player.gameObject.transform.position = new Vector3(0, 1, 0);
                _player.gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
                _camera.transform.localPosition = _originalCameraPos;
                _camera.transform.localRotation = Quaternion.Euler(_originalCameraRotation);
                _cameraGimbal.transform.position = _originalGimbalPos;
                _cameraGimbal.transform.rotation = Quaternion.Euler(Vector3.zero);
            }
        }

        #region Pathfinding methods

        /// <summary>
        /// Calculates the path from the player's position to the destination defined by mouse click.
        /// </summary>
        /// <param name="ray">The Ray instance resulting from a mouse click raycast.</param>
        /// <param name="hitInfo">The RaycastHit resulting from a mouse click raycast.</param>
        private void CalculatePath(Ray ray, RaycastHit hitInfo) {
            CleanupVisualAids();

            if (hitInfo.point.y > 0.1f) return;

            // Just travel point-to-point in a straight line if we haven't generated any content.
            if (_streetScene == null) {
                TraversePointToPoint(hitInfo);
                return;
            }

            int width = (int)_navPlane.transform.localScale.x * 10; // We're using a plane, so we have to multiply by 10 to get 1m-scale values.
            int depth = (int)_navPlane.transform.localScale.z * 10; // We're using a plane, so we have to multiply by 10 to get 1m-scale values.
            Vector3 startingIndexVec = new Vector3((int)((width / 2) - _player.transform.position.x), 0.0f, (int)((depth / 2) + _player.transform.position.z));
            Vector3 destinationIndexVec = new Vector3((int)((width / 2) - hitInfo.point.x), 0.0f, (int)((depth / 2) + hitInfo.point.z));

            //Debug.Log("Indexing Start @ [" + startingIndexVec.x + "," + startingIndexVec.z + "]");
            //Debug.Log("Indexing Destination @ [" + destinationIndexVec.x + "," + destinationIndexVec.z + "]");

            ResetTraversal();
            FindPath(startingIndexVec, destinationIndexVec);
        }

        /// <summary>
        /// Recalculates the player's path when a new destination is defined while traversal is already underway.
        /// </summary>
        public void RecalculateCurrentPath() {
            CleanupVisualAids();

            int width = (int)_navPlane.transform.localScale.x * 10; // We're using a plane, so we have to multiply by 10 to get 1m-scale values.
            int depth = (int)_navPlane.transform.localScale.z * 10; // We're using a plane, so we have to multiply by 10 to get 1m-scale values.
            Vector3 startingIndexVec = new Vector3((int)((width / 2) - _player.transform.position.x), 0.0f, (int)((depth / 2) + _player.transform.position.z));

            ResetTraversal();
            FindPath(startingIndexVec, _currentDestination);
        }

        /// <summary>
        /// Finds a path from the player's current location to the mouse click.
        /// </summary>
        /// <param name="ray">The Ray instance resulting from a mouse click raycast.</param>
        /// <param name="hitInfo">The RaycastHit resulting from a mouse click raycast.</param>
        private void FindPath(Vector3 startingIndexVec, Vector3 destinationIndexVec) {
            _currentDestination = destinationIndexVec;

            // Find the fastest route to the destination
            _astar.DistanceFunction = DistanceFunction.Chebyshev;
            List<Vector2> twoDPath = _astar.Process(_streetScene.SceneMetrics.navGridWeights, startingIndexVec, destinationIndexVec);

            if (twoDPath.Count <= 1) return;

            List<Vector3> threeDPath = ConvertTo3DPath(twoDPath, 1.01f);
            if (_drawPathfindingAids) DrawPath(ConvertToWorldSpace(threeDPath), "AStarPath", "516A8C_A255_MatteTransparent", 0.3f);

            /* Try to cull extraneous points in the path. We do 
             * two passes, one with Bresenham's algorithm and one with 
             * collinearity. */
            if (_bresenhamPruning) new Bresenham3D().Process(ref threeDPath, _streetScene.SceneMetrics.navGridWeights);
            if (_drawPathfindingAids && _bresenhamPruning) DrawPath(ConvertToWorldSpace(threeDPath), "BresenhamPath", "7703F7_A101_MatteTransparent", 0.5f, 0.5f);
            if (_collinearityPruning) new Collinearity3D().Process(ref threeDPath);
            if (_drawPathfindingAids && _collinearityPruning) DrawPath(ConvertToWorldSpace(threeDPath), "CollinearPath", "FFF84B_A255_MatteTransparent");
            
            threeDPath = ConvertToWorldSpace(threeDPath);

            // Traverse the final path via SQUAD splines to smooth it out
            TraverseSplinePath(threeDPath);
        }

        /// <summary>
        /// Traverses a path derived from pathfinding algorithms and converted to splines.
        /// </summary>
        /// <param name="path">The path derived from a pathfinding algorithm.</param>
        private void TraverseSplinePath(List<Vector3> path) {
            _splineController = CreateSplines(path, _player.gameObject);

            StartCoroutine(_splineController.FollowSpline());
        }

        /// <summary>
        /// Creates pathfinding splines for smoothed path traversal, adds them to the 
        /// SplinController, and returns the SplineController.
        /// </summary>
        /// <param name="path">A list of worldspace pathing points to convert into splines for smoothed path traversal.</param>
        /// <param name="target">The target of the spline creation; could be the player or an NPC.</param>
        /// <returns>The SplineController instance populated with splines.</returns>
        public SplineController CreateSplines(List<Vector3> path, GameObject target) {
            GameObject splineRoot = GameObject.Find("PathSplines");

            if (splineRoot != null) GameObject.Destroy(splineRoot);

            splineRoot = new GameObject();
            splineRoot.name = (target == _player) ? "PathSplines" : "PathSplines" + target.name;
            splineRoot.transform.parent = _streetScene.transform;
            splineRoot.transform.position = Vector3.zero;

            int i = 1;

            foreach (Vector3 point in path) {
                GameObject spline = new GameObject();
                spline.name = i.ToString();
                spline.transform.parent = splineRoot.transform;
                spline.transform.position = point;

                i++;
            }

            SplineController splineControl = target.GetComponent<SplineController>();
            splineControl.EndCallback += HandleSplineInterpolationEnd;
            splineControl.SplineRoot = splineRoot;
            splineControl.Duration = GetPathLength(path) * _splineVelocity;
            splineControl.HideOnExecute = true;
            splineControl.WrapMode = eWrapMode.ONCE;

            return splineControl;
        }

        /// <summary>
        /// Simply traverses point-to-point resulting from a mouse-click raycast as a default when no content has been generated in the scene.
        /// </summary>
        /// <param name="hitInfo">The RaycastHit resulting from a mouse click raycast.</param>
        private void TraversePointToPoint(RaycastHit hitInfo) {
            _currentPointToPointPath = _player.Grid.GetPath(_player.transform.position, hitInfo.point);
            _currentPointToPointPath[1].Position.y = _player.transform.position.y;

            InvokeRepeating("TraversePointToPoint", 0.0f, GlobalProperties.UPDATE_INTERVAL_120_FPS);
        }

        /// <summary>
        /// Handles 'per-frame' operations for a simple point-to-point traversal resulting from a mouse-click raycast.
        /// </summary>
        private void TraversePointToPoint() {
            if (_player.transform.position != _currentPointToPointPath[1].Position) {
                var maxDistance = _player.Speed * Time.deltaTime;
                var vectorToDestination = _currentPointToPointPath[1].Position - _player.transform.position;
                var moveDistance = Mathf.Min(vectorToDestination.magnitude, maxDistance);
                var moveVector = vectorToDestination.normalized * moveDistance;

                _player.transform.position += new Vector3(moveVector.x, 0.0f, moveVector.z);
                _player.transform.LookAt(new Vector3(_currentPointToPointPath[1].Position.x, _player.transform.position.y, _currentPointToPointPath[1].Position.z));
            }
            
            if (_player.transform.position == _currentPointToPointPath[1].Position) {
                ResetPointToPointTraversal();
            }
        }

        /// <summary>
        /// Resets any and all traversals currently in motion.
        /// </summary>
        private void ResetTraversal() {
            ResetSplineTraversal();
            ResetPointToPointTraversal();
        }

        /// <summary>
        /// Resets spline traversal.
        /// </summary>
        private void ResetSplineTraversal() {
            if (_splineController != null) StopCoroutine(_splineController.FollowSpline());
        }

        /// <summary>
        /// Resets point-to-point traversal.
        /// </summary>
        private void ResetPointToPointTraversal() {
            CancelInvoke("TraversePointToPoint");
            _currentPointToPointPath = null;
        }

        /// <summary>
        /// Converts a 2D path to a 3D path.
        /// </summary>
        /// <param name="twoDPath">The 2D path to convert.</param>
        /// <param name="withY">The y-value with which to populate each Vector3 in the 3D path.</param>
        /// <returns>A List<Vector3> representing a 3D path.</returns>
        public List<Vector3> ConvertTo3DPath(List<Vector2> twoDPath, float withY=0.0f) {
            List<Vector3> threeDPath = new List<Vector3>(twoDPath.Count);

            foreach (Vector2 vector2 in twoDPath) {
                threeDPath.Add(new Vector3(vector2.x, withY, vector2.y));
            }

            return threeDPath;
        }

        /// <summary>
        /// Converts the incoming path, which corresponds to a 0-indexed data grid, into a path in world space.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The converted path.</returns>
        public List<Vector3> ConvertToWorldSpace(List<Vector3> path) {
            List<Vector3> worldPath = new List<Vector3>(path.Count);
            int width = (int)_navPlane.transform.localScale.x * 10; // We're using a plane, so we have to multiply by 10 to get 1m-scale values.
            int depth = (int)_navPlane.transform.localScale.z * 10; // We're using a plane, so we have to multiply by 10 to get 1m-scale values.

            foreach (Vector3 vector3 in path) {
                worldPath.Add(new Vector3((width / 2) - vector3.x - 0.5f, vector3.y, -(depth / 2) + vector3.z + 0.5f));
            }

            return worldPath;
        }

        /// <summary>
        /// Gets the actual length of a path, point-to-point.
        /// </summary>
        /// <param name="path">The path whose length we want to derive.</param>
        /// <returns>The length of the path.</returns>
        private float GetPathLength(List<Vector3> path) {
            float length = 0.0f;

            for (int i = 1; i < path.Count; i++) {
                length += SMath.Abs((path[i] - path[i - 1]).magnitude);
            }

            return length;
        }

        #endregion

        #region Camera controls

        /// <summary>
        /// Tilts the camera.
        /// </summary>
        /// <param name="degree">The number of degrees by which to alter the camera tilt.</param>
        private void TiltCamera(int degree=1) {
            Quaternion newRotation = _camera.transform.localRotation * Quaternion.Euler(new Vector3(degree, 0, 0));

            if (newRotation.eulerAngles.x <= _DEFAULT_MAXIMUM_TILT_ANGLE + 0.5f &&
                newRotation.eulerAngles.x >= _originalCameraRotation.x - 0.5f
            ) {
                _camera.transform.localRotation = newRotation;
            }
        }

        /// <summary>
        /// Translates the camera gimbal in the specified direction relative to the gimbal.
        /// </summary>
        /// <param name="gimbalDirection">A direction relative to the gimbal; these should come from the gimbal's transform.</param>
        private void TranslateGimbal(Vector3 gimbalDirection) {
            float gimbalVelocity = _DEFAULT_GIMBAL_VELOCITY;

            _cameraGimbal.transform.position += gimbalDirection * gimbalVelocity * Time.deltaTime * Time.timeScale;
        }

        /// <summary>
        /// Rotates the camera gimbal by a number of degrees.
        /// </summary>
        /// <param name="degree">The number of degrees to rotate the gimbal.</param>
        private void RotateGimbal(int degree=1) {
            _cameraGimbal.transform.rotation *= Quaternion.Euler(new Vector3(0, degree, 0));
        }

        /// <summary>
        /// Zooms the camera in and out.
        /// </summary>
        private void ZoomCamera() {
            float zoomMag = _DEFAULT_ZOOM_MAGNITUDE;

            Vector3 newPos = _camera.transform.localPosition + _camera.transform.forward * Input.mouseScrollDelta.y;
            Vector3 cameraPosDelta = newPos - _originalCameraPos;
            float cameraPosDeltaMag = cameraPosDelta.magnitude;

            if (cameraPosDeltaMag <= zoomMag) _camera.transform.localPosition = newPos;
        }

        #endregion

        #region Handlers and callbacks

        /// <summary>
        /// Handles the end of spline interpolation, i.e. when the animation of the character along the pathing splines has stopped.
        /// </summary>
        private void HandleSplineInterpolationEnd() {
            ResetSplineTraversal();
        }

        /// <summary>
        /// Handles input for the scene.
        /// </summary>
        private void UpdateInput() {
            if (_debugConsole == null ||
                _debugConsole.visualTarget == null || 
                _debugConsole.visualTarget.activeSelf) return;

            // Keyboard

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                TranslateGimbal(-_cameraGimbal.transform.forward);
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                TranslateGimbal(_cameraGimbal.transform.right);
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                TranslateGimbal(_cameraGimbal.transform.forward);
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                TranslateGimbal(-_cameraGimbal.transform.right);
            }

            if (Input.GetKey(KeyCode.Q)) {
                RotateGimbal();
            }

            if (Input.GetKey(KeyCode.E)) {
                RotateGimbal(-1);
            }

            if (Input.GetKey(KeyCode.Equals)) {
                TiltCamera();
            }

            if (Input.GetKey(KeyCode.Minus)) {
                TiltCamera(-1);
            }

            // Mouse & trackpad

            if (Input.mouseScrollDelta != Vector2.zero) {
                ZoomCamera();
            }

            if (Input.GetMouseButtonUp(0)) {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hitInfo)) {
                    try {
                        CalculatePath(ray, hitInfo);
                    } catch(Exception e) {
                        #if DEBUG
                        Debug.Log("An exception occurred attempted to find a path for the player: " + e.Message);
                        #endif
                    }
                }
            }
        }

        #endregion

        #region Debug visualization methods

        /// <summary>
        /// Draws a path of Vector3 derived from a pathfinding algorithm in the scene as a visualization aid for debugging.
        /// </summary>
        /// <param name="path">The path to draw.</param>
        /// <param name="rootName">The name of the root GameObject to which to attach the visuals.</param>
        /// <param name="materialName">The name of the material to apply.</param>
        private void DrawPath(List<Vector3> path, string rootName, string materialName, float scale=0.5f, float alpha=1.0f) {
            GameObject pathRoot = GameObject.Find(rootName);

            if (pathRoot != null) GameObject.Destroy(pathRoot);

            pathRoot = new GameObject();
            pathRoot.name = rootName;
            pathRoot.transform.parent = _streetScene.transform;
            pathRoot.transform.position = Vector3.zero;

            foreach (Vector3 point in path) {
                GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.name = "point_" + point.ToString();
                p.transform.parent = pathRoot.transform;
                p.transform.localScale = new Vector3(scale, scale, scale);
                p.transform.position = point;

                MeshRenderer renderer = p.GetComponent<MeshRenderer>();
                renderer.material = _materialsByName[materialName];

                if (alpha < 1.0f) renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, alpha);
            }
        }

        /// <summary>
        /// Cleans up all of the visual aids from the scene.
        /// </summary>
        private void CleanupVisualAids() {
            GameObject pathRoot = GameObject.Find("AStarPath");

            if (pathRoot != null) GameObject.Destroy(pathRoot);

            pathRoot = GameObject.Find("BresenhamPath");

            if (pathRoot != null) GameObject.Destroy(pathRoot);

            pathRoot = GameObject.Find("CollinearPath");

            if (pathRoot != null) GameObject.Destroy(pathRoot);
        }

        #endregion
    }
}
