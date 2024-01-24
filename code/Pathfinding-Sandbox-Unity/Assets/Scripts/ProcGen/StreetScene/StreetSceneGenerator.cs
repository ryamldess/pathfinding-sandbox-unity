/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using dk.Singleton;
using dk.Tools.Debug;

using SnS.ProcGen;

using System;
using System.Collections.Generic;
using System.Linq;

using PFS.Math;
using PFS.Scene;

using UnityEngine;
using Unity.AI.Navigation;

using SMath = System.Math;

namespace PFS.ProcGen.StreetScene {
    /// <summary>
    /// Generates a street scene for the game.
    /// </summary>
    public class StreetSceneGenerator : MonoSingleton, IGenerator<StreetSceneProduct, StreetSceneGeneratorParameters> {
        private const int _MINIMUM_BUILDING_DIMENSTION = 20;

        #region Inspector properties

        [SerializeField]
        protected bool _generateBuildings = true;
        [SerializeField]
        protected bool _combineStreetSceneMeshes = true;
        [SerializeField]
        protected bool _addStandardNavigation = false;
        [SerializeField]
        protected bool _drawBlockMetaData = false;
        [SerializeField]
        protected bool _drawNavGridData = false;
        [SerializeField]
        protected bool _drawStreetPlanes = false;
        [SerializeField]
        protected bool _drawStreetSplines = false;

        #endregion

        public bool DrawBlockMetaData { get => _drawBlockMetaData; set => _drawBlockMetaData = value; }
        public bool DrawNavGridData { get => _drawNavGridData; set => _drawNavGridData = value; }
        public bool DrawStreetPlanes { get => _drawStreetPlanes; set => _drawStreetPlanes = value; }
        public bool DrawStreetSplineVisuals { get => _drawStreetSplines; set => _drawStreetSplines = value; }

        private StreetSceneProduct _generatedStreetScene = default(StreetSceneProduct);
        private PFSMainScene _mainScene = null;
        private StreetSceneGeneratorParameters _parameters = default(StreetSceneGeneratorParameters);

        #region Monobehaviours

        /// <summary>
        /// Starts this instance.
        /// </summary>
        protected sealed override void Start() {
            base.Start();
        }

        #endregion

        /// <summary>
        /// Generates a List of StreetSceneProduct instances according to the parameters in a StreetSceneGeneratorParameters object.
        /// </summary>
        /// <param name="parameters">The parameters of the generation.</param>
        /// <returns>A list of StreetSceneProduct instances.</returns>
        public List<StreetSceneProduct> Generate(StreetSceneGeneratorParameters parameters) {
            _mainScene = Singleton.GetInstance<PFSMainScene>() as PFSMainScene;
            _parameters = parameters;

            List<StreetSceneProduct> generatedScenes = GenerateMainProduct();

            GenerateStreetsAndNavGrid();
            GenerateBuildings();
            CombineStreetSceneMeshes();
            AddStandardNavigation();

            return generatedScenes;
        }

        #region Street scene procgen methods

        /// <summary>
        /// Generates the main street scene product.
        /// </summary>
        /// <returns>A list of StreetSceneProducts.</returns>
        protected List<StreetSceneProduct> GenerateMainProduct() {
            GameObject parentObject = new GameObject(_parameters.name);
            StreetSceneProduct stubbedStreetScene = parentObject.AddComponent<StreetSceneProduct>();
            stubbedStreetScene.transform.position = Vector3.zero;

            if (_parameters.parent != null) stubbedStreetScene.transform.parent = _parameters.parent.transform;

            _generatedStreetScene = stubbedStreetScene;

            return new List<StreetSceneProduct>(new StreetSceneProduct[] { _generatedStreetScene });
        }

        /// <summary>
        /// Generates GameObjects and data objects for streets, intersections, blocks and a navigation grid.
        /// </summary>
        protected void GenerateStreetsAndNavGrid() {
            ManhattanStreets streetData = _parameters.manhattanStreets;
            int streetWidth = _parameters.manhattanStreets.streetWidth;
            int width = (int)_mainScene.NavPlane.transform.localScale.x * 10; // We're using a plane, so we have to multiply by 10 to get 1m-scale values.
            int depth = (int)_mainScene.NavPlane.transform.localScale.z * 10; // We're using a plane, so we have to multiply by 10 to get 1m-scale values.

            float northSouthXDivisor = width / (streetData.northSouthStreets + 1);
            float eastWestZDivisor = depth / (streetData.eastWestStreets + 1);
            int maxStreetXVariation = (int)(((northSouthXDivisor - streetWidth) / 2) - (streetWidth / 2));
            int maxStreetZVariation = (int)(((eastWestZDivisor - streetWidth) / 2) - (streetWidth / 2));

            int[] northSouthXPositions = new int[streetData.northSouthStreets];
            int[] eastWestZPositions = new int[streetData.eastWestStreets];
            
            int i = 0;

            // Generate street splines with slight psuedo-random variation, clamped to the dimensions of the NavPlane.

            for (i = 0; i < northSouthXPositions.Length; i++) {
                northSouthXPositions[i] = 
                    (int)(
                        (northSouthXDivisor * (i + 1)) + 
                        UnityEngine.Random.Range(-maxStreetXVariation, maxStreetXVariation + 1)
                    );
            }

            for (i = 0; i < eastWestZPositions.Length; i++) {
                eastWestZPositions[i] =
                    (int)(
                        (eastWestZDivisor * (i + 1)) +
                        UnityEngine.Random.Range(-maxStreetZVariation, maxStreetZVariation + 1)
                    );
            }

            _generatedStreetScene.SceneMetrics.navPlaneWidth = width;
            _generatedStreetScene.SceneMetrics.navPlaneDepth = depth;
            _generatedStreetScene.SceneMetrics.eastWestZPositions = eastWestZPositions;
            _generatedStreetScene.SceneMetrics.northSouthXPositions = northSouthXPositions;

            if (_drawStreetSplines) DrawStreetSplines();

            GenerateStreetPlanes(); // Generate the actual street geometry
            GenerateBlockMetadata(); // Generate metadata for street intersections for building gen
            CreateAndPopulateNavGrid(); // Populate navigational grid
        }

        /// <summary>
        /// Generates planes for all of the streets we defined with simple X and Z values and the streetWidth defined in our parameters.
        /// </summary>
        private void GenerateStreetPlanes() {
            StreetSceneMetrics metrics = _generatedStreetScene.SceneMetrics;
            int width = _generatedStreetScene.SceneMetrics.navPlaneWidth;
            int depth = _generatedStreetScene.SceneMetrics.navPlaneDepth;

            var streetPlanes = new GameObject();
            streetPlanes.name = "StreetPlanes";
            streetPlanes.transform.parent = _generatedStreetScene.transform;
            streetPlanes.transform.position = Vector3.zero;

            int i = 0;

            for (i = 0; i < metrics.northSouthXPositions.Length; i++) {
                var streetDepth = (float)depth / 10.0f;

                GameObject streetPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                streetPlane.name = "streetPlane_NS_" + (i + 1);
                // We're using a plane, so we have to divide by 10 to get 1m-scale values.
                streetPlane.transform.localScale = new Vector3((float)_parameters.manhattanStreets.streetWidth / 10.0f, 1.0f, streetDepth);
                streetPlane.transform.parent = streetPlanes.transform;
                //streetPlane.transform.position = new Vector3((width / 2) - metrics.northSouthXPositions[i], 0.01f, 0.0f);
                streetPlane.transform.position = new Vector3((width / 2) - metrics.northSouthXPositions[i], 0.01f, 0.0f);

                if (_drawStreetPlanes) {
                    MeshRenderer renderer = streetPlane.transform.GetComponent<MeshRenderer>();
                    if (renderer != null) renderer.material = _mainScene.MaterialsByName["7703F7_A101_MatteTransparent"];
                }
            }

            for (i = 0; i < metrics.eastWestZPositions.Length; i++) {
                var streetWidth = (float)width / 10.0f;

                GameObject streetPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                streetPlane.name = "streetPlane_EW_" + (i + 1);
                // We're using a plane, so we have to divide by 10 to get 1m-scale values.S
                streetPlane.transform.localScale = new Vector3(streetWidth, 1.0f, (float)_parameters.manhattanStreets.streetWidth / 10.0f);
                streetPlane.transform.parent = streetPlanes.transform;
                streetPlane.transform.position = new Vector3(0.0f, 0.01f, -(depth / 2) + metrics.eastWestZPositions[i]);

                if (_drawStreetPlanes) {
                    MeshRenderer renderer = streetPlane.transform.GetComponent<MeshRenderer>();
                    if (renderer != null) renderer.material = _mainScene.MaterialsByName["7703F7_A101_MatteTransparent"];
                }
            }

            _generatedStreetScene.StreetPlanes = streetPlanes;
        }

        /// <summary>
        /// Generates metadata for all blocks derived from the intersection data defined earlier.
        /// </summary>
        private void GenerateBlockMetadata() {
            int width = _generatedStreetScene.SceneMetrics.navPlaneWidth;
            int depth = _generatedStreetScene.SceneMetrics.navPlaneDepth;
            int streetWidth = _parameters.manhattanStreets.streetWidth;
            float halfWidth = streetWidth / 2;
            StreetSceneMetrics metrics = _generatedStreetScene.SceneMetrics;
            List<BlockData> blockMetadata = new List<BlockData>();
            List<Vector3> intersectionMetadata = new List<Vector3>();

            float northEdge = -(depth / 2);
            float southEdge = (depth / 2);
            float eastEdge = -(width / 2);
            float westEdge = (width / 2);

            for (int i = 0; i < metrics.northSouthXPositions.Length; i++) {
                for (int j = 0; j < metrics.eastWestZPositions.Length; j++) {
                    // Intersection metadata
                    
                    var X = westEdge - metrics.northSouthXPositions[i];
                    var Z = northEdge + metrics.eastWestZPositions[j];

                    intersectionMetadata.Add(new Vector3(X, 0.01f, Z));

                    /* Block metadata
                     * We are calculating this from intersection positions, which are in between the blocks; 
                     * So for each intersection, we need to evaluate which blocks we need to generate. We 
                     * choose the block NW of us as the default, since the top left block is in the NW corner, 
                     * and we proceed through the data in a way that corresponds to traveling from N-S and W-E:
                     * 
                     * - Every intersection has to generate a NW block
                     * - The last intersection on each row from N-S (eastern most) also needs to generate a NE block
                     * - The last intersection in each column from E-W (southern most) also needs to generate a SW block
                     * - The last intersection in both rows and columns (sotuheast most) also needs to generate a SE block
                     * */

                    // NW Block
                    var nwBlock = new BlockData();
                    var prevNSXPos = (i == 0) ? 0 : metrics.northSouthXPositions[i - 1];
                    var prevEWZPos = (j == 0) ? 0 : metrics.eastWestZPositions[j - 1];
                    var clampedStreetXOffset = (i == 0) ? 0 : halfWidth;
                    var clampedStreetZOffset = (j == 0) ? 0 : halfWidth;

                    nwBlock.nwCorner = new Vector3(westEdge - prevNSXPos - clampedStreetXOffset, 0, northEdge + prevEWZPos + clampedStreetZOffset);
                    nwBlock.neCorner = new Vector3(X + halfWidth, 0, northEdge + prevEWZPos + clampedStreetZOffset);
                    nwBlock.seCorner = new Vector3(X + halfWidth, 0, Z - halfWidth);
                    nwBlock.swCorner = new Vector3(westEdge - prevNSXPos - clampedStreetXOffset, 0, Z - halfWidth);

                    blockMetadata.Add(nwBlock);

                    // NE Block for intersections on the eastern edge
                    if (i == metrics.northSouthXPositions.Length - 1) {
                        var neBlock = new BlockData();

                        neBlock.nwCorner = new Vector3(X - halfWidth, 0, northEdge + prevEWZPos + clampedStreetZOffset);
                        neBlock.neCorner = new Vector3(eastEdge, 0, northEdge + prevEWZPos + clampedStreetZOffset);
                        neBlock.seCorner = new Vector3(eastEdge, 0, Z - halfWidth);
                        neBlock.swCorner = new Vector3(X - halfWidth, 0, Z - halfWidth);

                        blockMetadata.Add(neBlock);
                    }

                    // SW Block for intersections on the southern edge
                    if (j == metrics.eastWestZPositions.Length - 1) {
                        var swBlock = new BlockData();

                        swBlock.nwCorner = new Vector3(westEdge - prevNSXPos - clampedStreetXOffset, 0, Z + halfWidth);
                        swBlock.neCorner = new Vector3(X + halfWidth, 0, Z + halfWidth);
                        swBlock.seCorner = new Vector3(X + halfWidth, 0, southEdge);
                        swBlock.swCorner = new Vector3(westEdge - prevNSXPos - clampedStreetXOffset, 0, southEdge);

                        blockMetadata.Add(swBlock);
                    }

                    // SE Block for the southeast most (and last processed) intersection
                    if (i == metrics.northSouthXPositions.Length - 1 && 
                        j == metrics.eastWestZPositions.Length - 1) {
                        var seBlock = new BlockData();

                        seBlock.nwCorner = new Vector3(X - halfWidth, 0, Z + halfWidth);
                        seBlock.neCorner = new Vector3(eastEdge, 0, Z + halfWidth);
                        seBlock.seCorner = new Vector3(eastEdge, 0, southEdge);
                        seBlock.swCorner = new Vector3(X - halfWidth, 0, southEdge);

                        blockMetadata.Add(seBlock);
                    }
                }
            }

            _generatedStreetScene.SceneMetrics.blockMetadata = blockMetadata;
            _generatedStreetScene.SceneMetrics.intersectionMetadata = intersectionMetadata;

            if (_drawBlockMetaData) DrawBlockAndIntersectionMetadata();
        }

        /// <summary>
        /// Creates the nav grid and populates it with weights for pathfinding.
        /// </summary>
        private void CreateAndPopulateNavGrid() {
            StreetSceneMetrics metrics = _generatedStreetScene.SceneMetrics;
            int navGridWidth = metrics.navPlaneWidth;
            int navGridDepth = metrics.navPlaneDepth;
            int streetWidth = _parameters.manhattanStreets.streetWidth;
            CartesianDataGrid2D<int> weightedGrid = new CartesianDataGrid2D<int>(navGridWidth, navGridDepth);
            int i = 0;
            int j = 0;
            int k = 0;

            // Populate navigable areas

            if (_drawNavGridData) CreateGridDataParent();

            for (i = 0; i < metrics.northSouthXPositions.Length; i++) {
                for (j = 0; j < streetWidth; j++) {
                    for (k = 0; k < navGridDepth; k++) {
                        var xIndex = metrics.northSouthXPositions[i] + (streetWidth / 2) - j - 1;
                        weightedGrid.Data[xIndex, k] = (j > 0 && j < streetWidth - 1) ? 1 : 2;

                        if (_drawNavGridData) IndicateNavigableSquare(xIndex, k, true);
                    }
                }
            }

            for (i = 0; i < metrics.eastWestZPositions.Length; i++) {
                for (j = 0; j < streetWidth; j++) {
                    for (k = 0; k < navGridWidth; k++) {
                        var zIndex = metrics.eastWestZPositions[i] + (streetWidth / 2) - j - 1;
                        weightedGrid.Data[k, zIndex] = (j > 0 && j < streetWidth - 1) ? 1 : 2;

                        if (_drawNavGridData) IndicateNavigableSquare(k, zIndex, true);
                    }
                }
            }

            /* Populate non-navigable areas; in this case, since only
             * our streets are navigable, we are simply looking for any cell
             * that still has a value of zero and giving it a prohibitively 
             * high weight.
             */

            for (i = 0; i < weightedGrid.Data.GetLength(0); i++) {
                for (j = 0; j < weightedGrid.Data.GetLength(1); j++) {
                    if (weightedGrid.Data[i, j] == 0) {
                        weightedGrid.Data[i, j] = Int16.MaxValue;

                        if (_drawNavGridData) IndicateNavigableSquare(i, j);
                    }
                }
            }

            _generatedStreetScene.SceneMetrics.navGridWeights = weightedGrid;
        }

        /// <summary>
        /// Generates buildings for the street scene that line up with the defined blocks and streets.
        /// </summary>
        protected void GenerateBuildings() {
            if (!_generateBuildings) return;

            StreetSceneMetrics metrics = _generatedStreetScene.SceneMetrics;
            List<BlockData> blocks = metrics.blockMetadata;
            int mapWidth = metrics.navPlaneWidth;
            int mapDepth = metrics.navPlaneDepth;
            int streetWidth = _parameters.manhattanStreets.streetWidth;
            int buildingSizeXThreshold = SMath.Max((mapWidth - (streetWidth * metrics.northSouthXPositions.Length)) / ((metrics.northSouthXPositions.Length + 1) * 2), _MINIMUM_BUILDING_DIMENSTION);
            int buildingSizeZThreshold = SMath.Max((mapDepth - (streetWidth * metrics.eastWestZPositions.Length)) / ((metrics.eastWestZPositions.Length + 1) * 2), _MINIMUM_BUILDING_DIMENSTION);
            int blockCount = 0;

            var buildings = new GameObject();
            buildings.name = "Buildings";
            buildings.transform.parent = _generatedStreetScene.transform;
            buildings.transform.position = Vector3.zero;

            foreach (BlockData block in blocks) {
                var blockWidth = block.nwCorner.x - block.neCorner.x;
                var blockDepth = block.swCorner.z - block.nwCorner.z;
                var numBuildingsX = (int)SMath.Round(blockWidth / buildingSizeXThreshold);
                var numBuildingsZ = (int)SMath.Round(blockDepth / buildingSizeZThreshold);
                var numBuildings = 1;

                if (numBuildingsX < 1) numBuildingsX = 1;
                if (numBuildingsZ < 1) numBuildingsZ = 1;

                for (int i = 0; i < numBuildingsX; i++) {
                    for (int j = 0; j < numBuildingsZ; j++) {
                        int height = UnityEngine.Random.Range(2, 8);
                        height *= 3;

                        var buildingWidth = blockWidth / numBuildingsX;
                        var buildingDepth = blockDepth / numBuildingsZ;
                        var X = block.nwCorner.x - (buildingWidth * (i + 1)) + (buildingWidth / 2);
                        var Z = block.nwCorner.z + (buildingDepth * (j + 1)) - (buildingDepth / 2);
                        var buildingPos = new Vector3(X, height / 2, Z);

                        int materialIndex = UnityEngine.Random.Range(2, 7);

                        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        building.name = "Block_" + blockCount + "_Building_(" + i + "," + j + ")";
                        building.transform.parent = buildings.transform;
                        building.transform.localScale = new Vector3(buildingWidth, height, buildingDepth);
                        building.transform.position = buildingPos;

                        MeshRenderer renderer = building.transform.GetComponent<MeshRenderer>();
                        renderer.material = _mainScene.Materials.ElementAt(materialIndex);
                        renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0.8f);
                    }
                }

                blockCount++;
            }

            _generatedStreetScene.Buildings = buildings;
        }

        #endregion

        #region Finishing & post-processing methods

        /// <summary>
        /// Combines a number of generated meshes into a single mesh.
        /// </summary>
        protected void CombineStreetSceneMeshes() {
            if (!_combineStreetSceneMeshes) return;

            CombineMeshes(_generatedStreetScene.StreetPlanes, GameObject.Find("streetPlane_NS_1"));
            CleanupAfterMeshCombine();
        }

        /// <summary>
        /// Combines all of the MeshFilter objects in children of a GameObject into a single mesh.
        /// </summary>
        /// <param name="parentMeshObject">The parent GameObject to target.</param>
        /// <param name="combineObject">The child GameObject on which to consolidate all of the meshes.</param>
        private void CombineMeshes(GameObject parentMeshObject, GameObject combineObject) {
            MeshFilter[] meshFilters = parentMeshObject.GetComponentsInChildren<MeshFilter>();

            if (meshFilters.Length < 2) return;

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;

            while (i < meshFilters.Length) {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);

                i++;
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine);
            mesh.Optimize();

            combineObject.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
            combineObject.transform.gameObject.SetActive(true);
            combineObject.transform.localScale = Vector3.one;
            combineObject.transform.position = Vector3.zero;
            combineObject.SetActive(true);
        }

        /// <summary>
        /// Cleans up orphaned objects following the mesh combine operation.
        /// </summary>
        private void CleanupAfterMeshCombine() {
            //
        }

        /// <summary>
        /// Adds standard Unity NavMesh navigation to the generated geometry.
        /// </summary>
        protected void AddStandardNavigation() {
            if (!_addStandardNavigation) return;

            NavMeshSurface navMeshSurface = _mainScene.NavPlane.AddComponent<NavMeshSurface>();
            navMeshSurface.BuildNavMesh();
        }

        #endregion

        #region Debug visualization methods

        /// <summary>
        /// Draws splines down the center of all of the streets defined in data.
        /// </summary>
        private void DrawStreetSplines() {
            StreetSceneMetrics metrics = _generatedStreetScene.SceneMetrics;
            int width = _generatedStreetScene.SceneMetrics.navPlaneWidth;
            int depth = _generatedStreetScene.SceneMetrics.navPlaneDepth;

            var streetSplines = new GameObject();
            streetSplines.name = "StreetSplines";
            streetSplines.transform.parent = _generatedStreetScene.transform;
            streetSplines.transform.position = Vector3.zero;

            int i = 0;

            for (i = 0; i < metrics.northSouthXPositions.Length; i++) {
                var streetSpline = new GameObject();
                streetSpline.name = "NS_spline_" + i.ToString();
                streetSpline.transform.parent = streetSplines.transform;
                var startX = (width / 2) - metrics.northSouthXPositions[i];
                var startZ = -(depth / 2);

                var lr = streetSpline.AddComponent<LineRenderer>();
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.SetPosition(0, new Vector3(startX, 0.3f, startZ));
                lr.SetPosition(1, new Vector3(startX, 0.3f, startZ + depth));
            }

            for (i = 0; i < metrics.eastWestZPositions.Length; i++) {
                var streetSpline = new GameObject();
                streetSpline.name = "NS_spline_" + i.ToString();
                streetSpline.transform.parent = streetSplines.transform;
                var startX = width / 2;
                var startZ = -(depth / 2) + metrics.eastWestZPositions[i];

                var lr = streetSpline.AddComponent<LineRenderer>();
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.SetPosition(0, new Vector3(startX, 0.3f, startZ));
                lr.SetPosition(1, new Vector3(startX - width, 0.3f, startZ));
            }

            _generatedStreetScene.VisualAids.Add(streetSplines);
        }

        /// <summary>
        /// Draws visual aids designating the positions of all intersections and blocks.
        /// </summary>
        private void DrawBlockAndIntersectionMetadata() {
            List<Vector3> intersections = _generatedStreetScene.SceneMetrics.intersectionMetadata;
            List<BlockData> blocks = _generatedStreetScene.SceneMetrics.blockMetadata;

            var blockVisuals = new GameObject();
            blockVisuals.name = "BlockVisuals";
            blockVisuals.transform.parent = _generatedStreetScene.transform;
            blockVisuals.transform.position = Vector3.zero;

            var intersectionVisuals = new GameObject();
            intersectionVisuals.name = "IntersectionVisuals";
            intersectionVisuals.transform.parent = _generatedStreetScene.transform;
            intersectionVisuals.transform.position = Vector3.zero;

            int i = 0;

            foreach (Vector3 intersection in intersections) {
                var intersectionVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                intersectionVis.name = "intersection_" + i + "_" + intersection.ToString();
                intersectionVis.transform.parent = intersectionVisuals.transform;
                intersectionVis.transform.position = new Vector3(intersection.x, 0.3f, intersection.z);

                MeshRenderer renderer = intersectionVis.GetComponent<MeshRenderer>();
                renderer.material = _mainScene.MaterialsByName["03F7A9_Matte"];

                i++;
            }

            i = 0;

            foreach (BlockData block in blocks) {
                var nwVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                nwVis.name = "block_" + i + "_nw_" + block.nwCorner.ToString();
                nwVis.transform.parent = blockVisuals.transform;
                nwVis.transform.position = new Vector3(block.nwCorner.x, 0.3f, block.nwCorner.z);

                var neVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                neVis.name = "block_" + i + "_ne_" + block.neCorner.ToString();
                neVis.transform.parent = blockVisuals.transform;
                neVis.transform.position = new Vector3(block.neCorner.x, 0.3f, block.neCorner.z);

                var seVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                seVis.name = "block_" + i + "_se_" + block.seCorner.ToString();
                seVis.transform.parent = blockVisuals.transform;
                seVis.transform.position = new Vector3(block.seCorner.x, 0.3f, block.seCorner.z);

                var swVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                swVis.name = "block_" + i + "_sw_" + block.swCorner.ToString();
                swVis.transform.parent = blockVisuals.transform;
                swVis.transform.position = new Vector3(block.swCorner.x, 0.3f, block.swCorner.z);

                MeshRenderer renderer = nwVis.GetComponent<MeshRenderer>();
                renderer.material = _mainScene.MaterialsByName["03F7A9_Matte"];
                renderer = neVis.GetComponent<MeshRenderer>();
                renderer.material = _mainScene.MaterialsByName["03F7A9_Matte"];
                renderer = seVis.GetComponent<MeshRenderer>();
                renderer.material = _mainScene.MaterialsByName["03F7A9_Matte"];
                renderer = swVis.GetComponent<MeshRenderer>();
                renderer.material = _mainScene.MaterialsByName["03F7A9_Matte"];

                i++;
            }

            _generatedStreetScene.VisualAids.Add(blockVisuals);
            _generatedStreetScene.VisualAids.Add(intersectionVisuals);
        }

        /// <summary>
        /// Grids a parent GameObject to which to attach visuals for individual grid cells.
        /// </summary>
        private void CreateGridDataParent() {
            var gridVisuals = new GameObject();
            gridVisuals.name = "GridDataVisuals";
            gridVisuals.transform.parent = _generatedStreetScene.transform;
            gridVisuals.transform.position = Vector3.zero;

            _generatedStreetScene.VisualAids.Add(gridVisuals);
        }

        /// <summary>
        /// Places a marker to indicate an individual grid square and whether or not it is navigable.
        /// </summary>
        /// <param name="x">The x index of the grid square in data.</param>
        /// <param name="z">The z index of the grid square in data.</param>
        /// <param name="canPass">Whether or not the square is passable (navigable).</param>
        private void IndicateNavigableSquare(int x, int z, bool canPass=false) {
            StreetSceneMetrics metrics = _generatedStreetScene.SceneMetrics;
            int mapWidth = metrics.navPlaneWidth;
            int mapDepth = metrics.navPlaneDepth;
            float posOffset = 0.5f;

            GameObject parent = GameObject.Find("GridDataVisuals");
            GameObject sqMarker = (canPass) ? GameObject.CreatePrimitive(PrimitiveType.Cylinder) : GameObject.CreatePrimitive(PrimitiveType.Cube);
            sqMarker.name = "grid_marker_(" + x + "," + z + ")";
            sqMarker.transform.parent = parent.transform;
            sqMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            sqMarker.transform.position = new Vector3((mapWidth / 2) - x - posOffset, 0.6f, -(mapDepth / 2) + z + posOffset);

            MeshRenderer renderer = sqMarker.GetComponent<MeshRenderer>();
            renderer.material = _mainScene.MaterialsByName[(canPass) ? "03F7A9_A255_MatteTransparent" : "FC4C09_A255_MatteTransparent"];
            renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, (canPass) ? 0.5f : 0.8f);

            if (sqMarker.GetComponent<BoxCollider>() != null) Destroy(sqMarker.GetComponent<BoxCollider>());
            if (sqMarker.GetComponent<CapsuleCollider>() != null) Destroy(sqMarker.GetComponent<CapsuleCollider>());
        }

        #endregion
    }
}
