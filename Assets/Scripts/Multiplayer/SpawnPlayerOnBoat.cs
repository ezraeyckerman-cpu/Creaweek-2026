using UnityEngine;

public class SpawnPlayerOnBoat : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnLocations; // Assign in inspector OR find automatically

    void Awake()
    {
        // Auto-find SpawnLocations if not assigned
        if (spawnLocations == null)
        {
            GameObject obj = GameObject.Find("SpawnLocations");
            if (obj != null)
            {
                spawnLocations = obj.transform;
            }
            else
            {
                Debug.LogError("SpawnLocations object not found!");
                return;
            }
        }

        // Make sure there are spawn points
        if (spawnLocations.childCount == 0)
        {
            Debug.LogError("No spawn points found under SpawnLocations!");
            return;
        }

        // Try to find a free spawn point
        Transform chosenSpawn = null;

        // Shuffle-style attempt
        int attempts = spawnLocations.childCount;
        for (int i = 0; i < attempts; i++)
        {
            Transform potential = spawnLocations.GetChild(Random.Range(0, spawnLocations.childCount));

            // Check if it has no children (empty spot)
            if (potential.childCount == 0)
            {
                chosenSpawn = potential;
                break;
            }
        }

        // If no empty spawn found, stop
        if (chosenSpawn == null)
        {
            Debug.LogWarning("No free spawn locations available!");
            return;
        }

        // Spawn player
        GameObject player = Instantiate(playerPrefab, chosenSpawn.position, chosenSpawn.rotation);

        // Parent it to the spawn point
        player.transform.SetParent(chosenSpawn);
    }
}