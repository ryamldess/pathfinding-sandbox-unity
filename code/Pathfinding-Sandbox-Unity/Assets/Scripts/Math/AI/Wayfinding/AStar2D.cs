/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

using dk.Tools.Debug;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using SMath = System.Math;

namespace PFS.Math.AI.Wayfinding {
    /// <summary>
    /// A* implementation for Unity/C# on a 2D grid of data.
    /// </summary>
    public class AStar2D<NumberType> : System.Object {
        public DistanceFunction DistanceFunction { get => _distanceFunction; set => _distanceFunction = value; }

        protected DistanceFunction _distanceFunction = DistanceFunction.Euclidean;

        public CartesianDataGrid2D<NumberType> HeuristicWeights { get => _heuristicWeights; set => _heuristicWeights = value; }

        protected CartesianDataGrid2D<NumberType> _heuristicWeights = new CartesianDataGrid2D<NumberType>(0, 0);

        private List<SearchNode> _neighbors = default(List<SearchNode>);

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AStar2D() {
            ValidateNumberType();
            _neighbors = new List<SearchNode>((_distanceFunction == DistanceFunction.Chebyshev) ? 8 : 4);
        }

        /// <summary>
        /// Processes the A* algorithm.
        /// </summary>
        /// <param name="heuristicWeights">A weighted data grid to feed the heuristic.</param>
        /// <param name="start">The 3D origin point (y will be ignored).</param>
        /// <param name="destination">The 3D destination point (y will be ignored).</param>
        /// <returns>A 2D path.</returns>
        public List<Vector2> Process(CartesianDataGrid2D<NumberType> heuristicWeights, Vector3 start, Vector3 destination) {
            _heuristicWeights = heuristicWeights;

            return Process(start, destination);
        }

        /// <summary>
        /// Processes the A* algorithm.
        /// </summary>
        /// <param name="start">The 3D origin point (y will be ignored).</param>
        /// <param name="destination">The 3D destination point (y will be ignored).</param>
        /// <returns>A two-dimensional path through the data grid as a List<Vector2>.</returns>
        public List<Vector2> Process(Vector3 start, Vector3 destination) {
            SearchNode startNode = new SearchNode(start);
            SearchNode goalNode = new SearchNode(destination);
            startNode.gScore = 0;
            startNode.fScore = (int)(startNode.gScore + Distance(startNode, goalNode));

            /* If the weight of the goal node is the special value indicating 
               an obstacle, it is unreachable, so escape. */
            if (Weight(goalNode) >= Int16.MaxValue) return new List<Vector2>();

            /* Below is a pretty canonical implementation of A* except that we 
               apply weights from our nav grid as well. */

            AStar2DPQ openSet = new AStar2DPQ();
            List<SearchNode> closedSet = new List<SearchNode>();
            SearchNode currentNode = default(SearchNode);

            openSet.Add(startNode);

            while (openSet.Count > 0) {
                currentNode = openSet.Peek();

                if (currentNode.Equals(goalNode)) {
                    openSet.Clear();
                    break;
                } else {
                    currentNode = openSet.Dequeue();
                    closedSet.Add(currentNode);

                    _neighbors.Clear();
                    GetNeighbors(currentNode, ref _neighbors);

                    foreach (SearchNode n in _neighbors) {
                        var tentativeGScore = (currentNode.gScore + Distance(currentNode, n));

                        if (closedSet.Exists(e => (e.x == n.x && e.y == n.y)) && tentativeGScore >= currentNode.gScore) {
                            continue;
                        } else if (!closedSet.Exists(e => (e.x == n.x && e.y == n.y)) || (tentativeGScore < currentNode.gScore && Weight(n) < Weight(currentNode))) { // This weight score is the only non-canonical part of this implementation
                            n.Back = currentNode;
                            n.gScore = tentativeGScore;
                            n.fScore = (int)(n.gScore + Distance(n, goalNode));

                            if (!openSet.Contains(n)) openSet.Add(n);
                        }
                    }
                }
            }

            return ReconstructPath(start, destination, currentNode);
        }

        /// <summary>
        /// Gets the weight of a SearchNode according to its coordinates in the weighted nav grid.
        /// </summary>
        /// <param name="current">The node for which we want to derive the weight.</param>
        /// <returns>The weight of the node on the nav grid.</returns>
        private float Weight(SearchNode current) {
            var weight = _heuristicWeights.Data[current.x, current.y];
            float floatWeight = 0.0f;
            float.TryParse(weight.ToString(), out floatWeight);

            return floatWeight;
        }

        /// <summary>
        /// Gets all of the neighbors for a SearchNode. If the distance function is Chebyshev, diagonal neighbors are included.
        /// </summary>
        /// <param name="byNode">The node whose neighbors we want to look up.</param>
        /// <returns>The node's neighbors.</returns>
        private void GetNeighbors(SearchNode byNode, ref List<SearchNode> neighbors) {
            if (byNode.x - 1 > 0) neighbors.Add(new SearchNode(byNode.x - 1, byNode.y));
            if (byNode.y - 1 > 0) neighbors.Add(new SearchNode(byNode.x, byNode.y - 1));
            if (_distanceFunction == DistanceFunction.Chebyshev) if (byNode.x - 1 > 0 && byNode.y - 1 > 0) neighbors.Add(new SearchNode(byNode.x - 1, byNode.y - 1));
            if (_distanceFunction == DistanceFunction.Chebyshev) if (byNode.x + 1 < _heuristicWeights.Data.GetLength(0) && byNode.y - 1 > 0) neighbors.Add(new SearchNode(byNode.x + 1, byNode.y - 1));
            if (byNode.x + 1 < _heuristicWeights.Data.GetLength(0)) neighbors.Add(new SearchNode(byNode.x + 1, byNode.y)); 
            if (byNode.y + 1 < _heuristicWeights.Data.GetLength(1)) neighbors.Add(new SearchNode(byNode.x, byNode.y + 1));
            if (_distanceFunction == DistanceFunction.Chebyshev) if (byNode.x + 1 < _heuristicWeights.Data.GetLength(0) && byNode.y + 1 < _heuristicWeights.Data.GetLength(1)) neighbors.Add(new SearchNode(byNode.x + 1, byNode.y + 1));
            if (_distanceFunction == DistanceFunction.Chebyshev) if (byNode.x - 1 > 0 && byNode.y + 1 < _heuristicWeights.Data.GetLength(1)) neighbors.Add(new SearchNode(byNode.x - 1, byNode.y + 1));

            // Remove obstacle nodes. We've assigned the special value of Int16.MaxValue for this.
            neighbors.RemoveAll(a => Weight(a) >= Int16.MaxValue);
        }

        /// <summary>
        /// Reconstructs the path backwards from single-linked list structure of the SearchNode class.
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="destination">The destination point.</param>
        /// <param name="finalNode">The end node discovered via A*.</param>
        /// <returns>A path in the form of 2-dimensional vectors.</returns>
        private List<Vector2> ReconstructPath(Vector3 start, Vector3 destination, SearchNode finalNode) {
            List<Vector2> path = new List<Vector2>();
            SearchNode currentNode = finalNode;

            while (currentNode.Back != null) {
                path.Add(new Vector2(currentNode.Back.x, currentNode.Back.y));
                currentNode = currentNode.Back;
            }

            path.Reverse();

            path.Add(new Vector2(destination.x, destination.z));

            return path;
        }

        #region Distance functions

        /// <summary>
        /// Obtains the distance between two nodes according to the selected distance function.
        /// </summary>
        /// <param name="current">The current, or starting node.</param>
        /// <param name="goal">The goal, or target node.</param>
        /// <returns>The distance between the two nodes.</returns>
        private float Distance(SearchNode current, SearchNode goal) {
            if (_distanceFunction == DistanceFunction.Euclidean) return EuclideanDistance(current, goal);

            if (_distanceFunction == DistanceFunction.Manhattan) return ManhattanDistance(current, goal);

            if (_distanceFunction == DistanceFunction.Chebyshev) return ChebyshevDistance(current, goal);

            return 0.0f;
        }

        /// <summary>
        /// Calculates the Euclidean, or straight-line, distance between two nodes.
        /// </summary>
        /// <param name="current">The current, or starting node.</param>
        /// <param name="goal">The goal, or target node.</param>
        /// <returns>The Euclidean distance between the two nodes.</returns>
        private float EuclideanDistance(SearchNode current, SearchNode goal) {
            return SMath.Abs((new Vector2(goal.x, goal.y) - new Vector2(current.x, current.y)).magnitude);
        }

        /// <summary>
        /// Calculates the Manhattan distance between two nodes.
        /// </summary>
        /// <param name="current">The current, or starting node.</param>
        /// <param name="goal">The goal, or target node.</param>
        /// <returns>The Manhattan distance between the two nodes.</returns>
        private float ManhattanDistance(SearchNode current, SearchNode goal) {
            return SMath.Abs(current.x - goal.x) + SMath.Abs(current.y - goal.y);
        }

        /// <summary>
        /// Calculates the Chebyshev distance between two nodes.
        /// </summary>
        /// <param name="current">The current, or starting node.</param>
        /// <param name="goal"The goal, or target node.></param>
        /// <returns>The Chebyshev distance between the two nodes.</returns>
        private float ChebyshevDistance(SearchNode current, SearchNode goal) {
            float xDiff = SMath.Abs(goal.x - current.x);
            float yDiff = SMath.Abs(goal.y - current.y);

            return SMath.Max(xDiff, yDiff);
        }

        #endregion

        /// <summary>
        /// Validates that NumberType is a valid number type; this class can only operate on valid numeric types.
        /// </summary>
        protected void ValidateNumberType() {
            if (typeof(NumberType) != typeof(sbyte) &&
                typeof(NumberType) != typeof(byte) &&
                typeof(NumberType) != typeof(short) &&
                typeof(NumberType) != typeof(ushort) &&
                typeof(NumberType) != typeof(int) &&
                typeof(NumberType) != typeof(uint) &&
                typeof(NumberType) != typeof(long) &&
                typeof(NumberType) != typeof(ulong) &&
                typeof(NumberType) != typeof(nint) &&
                typeof(NumberType) != typeof(nuint) &&
                typeof(NumberType) != typeof(Int16) &&
                typeof(NumberType) != typeof(Int32) &&
                typeof(NumberType) != typeof(Int64) &&
                typeof(NumberType) != typeof(float) &&
                typeof(NumberType) != typeof(double) &&
                typeof(NumberType) != typeof(Double) &&
                typeof(NumberType) != typeof(decimal) &&
                typeof(NumberType) != typeof(Decimal)
            ) {
                throw new Exception("NumberType is not a valid numeric type. Operations on this class will likely fail.");
            }
        }
    }

    /// <summary>
    /// An enumeration of standard distance functions.
    /// </summary>
    public enum DistanceFunction {
        NULL = 0x0, 
        Euclidean, 
        Manhattan, 
        Chebyshev
    }

    /// <summary>
    /// Encapsulates and wraps fields for A* calculations of nodes in a navigation grid.
    /// </summary>
    internal class SearchNode {
        public SearchNode Back { get => _back; set => _back= value; }

        private SearchNode _back = null;

        public int fScore = 0;
        public float gScore = 0.0f;
        public int x = 0;
        public int y = 0;

        /// <summary>
        /// Default contructor.
        /// </summary>
        public SearchNode() {}

        /// <summary>
        /// Constructs a node from a Vector3 position.
        /// </summary>
        /// <param name="threeDCoordinates">A Vector3 position.</param>
        public SearchNode(Vector3 threeDCoordinates) {
            Convert3DTo2D(threeDCoordinates, out x, out y);
        }

        /// <summary>
        /// Constructs a node from x and y values in a 2D navigation grid.
        /// </summary>
        /// <param name="x">The x index into the grid.</param>
        /// <param name="y">The y index into the grid.</param>
        public SearchNode(int x, int y) {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Converts Vector3 point into integer x and y indices for a nav grid.
        /// </summary>
        /// <param name="threeDCoordinates">A Vector3 position.</param>
        /// <param name="x">The x index into the grid.</param>
        /// <param name="y">The y index into the grid.</param>
        private void Convert3DTo2D(Vector3 threeDCoordinates, out int x, out int y) {
            x = (int)threeDCoordinates.x;
            y = (int)threeDCoordinates.z;
        }

        /// <summary>
        /// Compares two SearchNodes and determines whether they are equal. For the pruposes of this 
        /// class, equality is simply defined as sharing a x and y values.
        /// </summary>
        /// <param name="obj">The comparison object.</param>
        /// <returns>A boolean; true if the objects are equal.</returns>
        public override bool Equals(System.Object obj) {
            if (((SearchNode)obj).x == x &&
                ((SearchNode)obj).y == y) {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// A subclass of PriorityQueue<T> specific to A*.
    /// </summary>
    internal class AStar2DPQ : PriorityQueue<SearchNode> {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AStar2DPQ() {
            _q = new Queue<SearchNode>();
        }

        /// <summary>
        /// Adds an element to the inner Queue.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <returns>An updated reference to this AStar2DPQ.</returns>
        public override PriorityQueue<SearchNode> Add(SearchNode element) {
            _q.Enqueue(element);
            _q = new Queue<SearchNode>(_q.OrderBy(min => min.fScore).ToArray());

            return this;
        }
    }

    /// <summary>
    /// A generic PriorityQueue class. .Net 7.0 has one of these, but we don't have access to it in Unity :(.
    /// </summary>
    /// <typeparam name="T">The type to store in the PriorityQueue.</typeparam>
    internal class PriorityQueue<T> : System.Object {
        public int Count { get => _q.Count; }

        public Queue<T> Q { get => _q; set => _q = value; }

        protected Queue<T> _q = default(Queue<T>);

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PriorityQueue() {
            _q = new Queue<T>();
        }

        /// <summary>
        /// Adds an element to the inner Queue.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <returns>An updated reference to this PriorityQueue<T>.</returns>
        public virtual PriorityQueue<T> Add(T element) {
            _q.Enqueue(element);
            
            return this;
        }

        /// <summary>
        /// Clears all elements from the inner Queue.
        /// </summary>
        /// <returns>An updated reference to this PriorityQueue<T>.</returns>
        public PriorityQueue<T> Clear() {
            _q.Clear();

            return this;
        }

        /// <summary>
        /// Returns whether the inner Queue contains the referenced element.
        /// </summary>
        /// <param name="element">The element to look up.</param>
        /// <returns>Whether the inner Queue contains the referenced element.</returns>
        public bool Contains(T element) {
            return _q.Contains(element);
        }

        /// <summary>
        /// Dequeues the first element from the inner Queue.
        /// </summary>
        /// <returns>The first element from the inner Queue after being dequeued.</returns>
        public virtual T Dequeue() {
            T element = _q.Dequeue();

            return element;
        }

        /// <summary>
        /// Peeks at the inner Queue, i.e. returns the first element.
        /// </summary>
        /// <returns>The first element of the inner Queue.</returns>
        public T Peek() {
            return _q.Peek();
        }
    }
}
