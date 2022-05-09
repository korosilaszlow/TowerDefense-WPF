using System;
using System.Threading.Tasks;

namespace TowerDefensePersistence
{
    public interface IPersistence
    {
        Task<GameData> LoadAsync(String path);
        Task SaveAsync(String path, GameData gamedata);
    }
}
