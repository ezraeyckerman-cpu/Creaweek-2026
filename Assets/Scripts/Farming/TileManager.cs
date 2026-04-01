using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance;


    [Space(10), Header("Bomb settings")]
    public int MaxBombs = 4;
    public int BombAmount;
    public GameObject BombObject;

    public List<FarmTile> FarmTiles = new List<FarmTile>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddTilesToList(FarmTile tile)
    {
        FarmTiles.Add(tile);
    }

    public void GenerateBombs()
    {
        if(FarmTiles.Count < 4)
            return;

        List<FarmTile> randomTiles = FarmTiles.OrderBy(x => UnityEngine.Random.value).Take(4).ToList();
        
        foreach(FarmTile tile in randomTiles)
        {
            if (BombAmount > MaxBombs)
                break;

            if(!tile.HasBomb)
            {
                tile.BombObject = BombObject;
                tile.Spawnbomb();
                tile.HasBomb = true;
                tile.gameObject.name = "Tile with mine";
                BombAmount++;
            }
        }
    }
}
