using System.Collections;
using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private float force;
    [SerializeField] private float triggerTime;

    public FarmTile ParentTile;

    private bool _destroy = false;

    private void OnTriggerEnter(Collider other)
    {
        /*
        if (other.transform.tag == "Player")
        {
            if (other.gameObject.GetComponent<FarmInteraction>().Mode == InteractionMode.Plowing)
                StartCoroutine(TriggerMine());
        }
        */
    }

    private void Update()
    {
        if(_destroy)
        {
            Destroy(gameObject, 0.5f);
        }
    }

    public IEnumerator TriggerMine(FarmTile farmtile)
    {
        yield return new WaitForSeconds(triggerTime);

        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(range, 0.5f, range));
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<PlayerMovement>(out PlayerMovement movement))
            {
                movement.Bombed = true;
                Vector3 direction = collider.transform.position - transform.position;
                movement.AddVelocity(new Vector3(0, force, 0) + direction.normalized * force / 3);
                continue;
            }
            if (collider.TryGetComponent<FarmTile>(out FarmTile tile))
            {
                tile.ResetPlot();
                continue;
            }
            if (collider.TryGetComponent<PlantedCrop>(out PlantedCrop crop))
            {
                Destroy(collider.gameObject);
                continue;
            }
        }

        farmtile.HasBomb = false;
        _destroy = true;
        //ParentTile.HasBomb = false;
        //DestroyImmediate(this.gameObject);
    }
}
