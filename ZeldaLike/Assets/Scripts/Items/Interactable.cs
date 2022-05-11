using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{

    [SerializeField]
    private GameObject player;
    [SerializeField]
    private float radius;

    // Update is called once per frame
    protected void Update()
    {
        if(Vector3.Distance(this.transform.position, player.transform.position) <= radius)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
    }

    protected abstract void Interact();

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position, radius);
    }

}
