
namespace FancyScrollView
{
    /// <summary>
    /// MutablePrefabMapping store the prefab index with data index and cell size of the prefab
    /// </summary>
    public class MutablePrefabMapping
    {
        /// <summary>
        /// index of the data source
        /// </summary>
        public int DataSourceIndex { get; set; }
       
        /// <summary>
        /// cell size of the  FlexBase
        /// </summary>
        public float CellSize { get; set; }
    }
}