namespace TowerDefenseViewModel
{
    /// <summary>
    /// A játékos által végzett műveletet reprezentáló enum
    /// </summary>
    public enum PlayerActionMode
    {
        /// <summary>Nincs speciális művelet</summary>
        None = 0,
        /// <summary>Távolsági torony építése</summary>
        BuildRangedTower = 1,
        /// <summary>Sebzőtorony építése</summary>
        BuildDamageTower = 2,
        /// <summary>Segédtorony építése</summary>
        BuildSupportTower = 3,
        /// <summary>Torony fejlesztése</summary>
        UpgradeTower = 4,
        /// <summary>Torony rombolása</summary>
        RemoveTower = 5
    }

}
