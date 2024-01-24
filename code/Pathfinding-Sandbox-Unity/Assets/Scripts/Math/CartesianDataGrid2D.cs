/** 
 * Authored by Steve Sedlmayr in 2024 for this pathfinding sandbox project.
 **/

namespace PFS.Math {
    /// <summary>
    /// Enscapsulates a 2-dimensional Cartesian grid containing arbitrary data types.
    /// </summary>
    public class CartesianDataGrid2D<DataType> : System.Object {
        public DataType[,] Data { get => _data; set => _data = value; }

        private DataType[,] _data = new DataType[0,0];
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CartesianDataGrid2D() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CartesianDataGrid2D(int x, int y) {
            _data = new DataType[x,y];
        }
    }
}
