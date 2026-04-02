using System.Collections;
using UnityEngine;

public class FarmPlot : MonoBehaviour
{
    [SerializeField] private bool isCustomGrid;
    [SerializeField] private GameObject[] farmTiles;
    [SerializeField] private Vector2 gridSize;
    [SerializeField] private float tileIncrement;

    private void Start()
    {
        if (isCustomGrid) return;
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                Vector3 position = transform.position + new Vector3(i * farmTiles[0].transform.localScale.x + tileIncrement * i, farmTiles[0].transform.position.y, j * farmTiles[0].transform.localScale.z + tileIncrement * j);
                GameObject tile =  Instantiate(farmTiles[0], position, Quaternion.identity);
                tile.transform.SetParent(transform, true);
                //TileManager.Instance.AddTilesToList(tile.GetComponent<FarmTile>());
            }
        }
    }
}
