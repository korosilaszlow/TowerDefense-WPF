namespace TowerDefenseViewModel
{
    /// <summary>
    /// Cella típusát jelző enum
    /// </summary>
    public enum CellType
    {
        /// <summary>Mezőség</summary>
        Plain,
        /// <summary>Állóvíz</summary>
        Water,
        /// <summary>Hegység</summary>
        Mountain,

        /// <summary>Kék kastély</summary>
        Player1Castle,
        /// <summary>Vörös kastély</summary>
        Player2Castle,

        /// <summary>Kék távolsági torony</summary>
        Player1RangedTower,
        /// <summary>Kék segédtorony</summary>
        Player1SupportTower,
        /// <summary>Kék sebzőtorony</summary>
        Player1DamageTower,

        /// <summary>Vörös távolsági torony</summary>
        Player2RangedTower,
        /// <summary>Vörös segédtorony</summary>
        Player2SupportTower,
        /// <summary>Vörös sebzőtorony</summary>
        Player2DamageTower
    }

}
