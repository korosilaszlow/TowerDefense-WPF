namespace TowerDefenseViewModel
{
    /// <summary>
    /// Pályaszerkesztő építési művelet
    /// </summary>
    public enum BuildActionMode
    {
        /// <summary>Kék vár áthelyezése </summary>
        RepositionCastle1 = 0,
        /// <summary>Vörös vár áthelyezése </summary>
        RepositionCastle2 = 1,
        /// <summary>Mezőség lerakása</summary>
        PlacePlain = 2,
        /// <summary>Állóvíz lerakása </summary>
        PlaceWater = 3,
        /// <summary>Hegység lerakása </summary>
        PlaceMountain = 4
    }
}
