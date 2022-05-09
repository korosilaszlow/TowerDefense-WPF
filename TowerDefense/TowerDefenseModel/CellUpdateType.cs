namespace TowerDefenseModel
{
    /// <summary>
    /// Cellák változásának típusai
    /// </summary>
    public enum CellUpdateType
    {
        Fired,          //ha az itt lévő torony lőtt egyet
        Hit,            //ha ezt a cellát meglőtte egy torony
        Changed,        //ha megváltozott a cella típusa
        Upgraded,       //ha a rajta lévő torony szintje megnőtt
        SoldierEntered, //ha belépett rá egy katona (vásárlásnál amikor a várra katona kerül, az is ez)
        SoldierLeft,    //ha kilépett róla egy katona
        SoldierDied,    //ha meghalt rajta egy katona
    }
}