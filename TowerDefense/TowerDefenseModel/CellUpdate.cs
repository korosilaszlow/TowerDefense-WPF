using TowerDefensePersistence;
namespace TowerDefenseModel
{
    /// <summary>
    /// Megadja mely cella változott és hogyan
    /// </summary>
    public class CellUpdate
    {
        public Cell Cell { get; set; }
        public CellUpdateType CellUpdateType { get; set; }
    }
}
