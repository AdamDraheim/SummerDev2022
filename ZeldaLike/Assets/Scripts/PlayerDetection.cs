using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{

    [SerializeField]
    [Tooltip("How far from gravitational direction that the normal can work in degree")]
    private float steepness;
    private bool hitGround;

    [SerializeField]
    [Tooltip("Layermask for ground collision")]
    private LayerMask mask;

    private Vector3 normal = new Vector3(0, 1, 0);


    // Start is called before the first frame update
    void Start()
    {
        normal = Vector3.up;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag.Equals("ground"))
        {
            if (Physics.Raycast(this.transform.position, Vector3.down, out RaycastHit hit, 1.25f, mask))
            {
                if (CheckSteepCollision(hit.normal.normalized))
                {
                    normal = hit.normal.normalized;
                    hitGround = true;
                }
                else
                {
                    normal = Vector3.up;
                    hitGround = false;
                }
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("ground"))
        {
            normal = Vector3.up;
            hitGround = false;
        }
    }

    private bool CheckSteepCollision(Vector3 normal)
    {
        Vector3 pointNormal = normal;
        Vector3 gravity_dir = new Vector3(0, 1, 0);

        float dot = Vector3.Dot(pointNormal, gravity_dir);

        float theta = Mathf.Acos(dot);

        if (theta * 180 / Mathf.PI < steepness)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Vector3 GetGroundNormal()
    {
        return this.normal;
    }

    public bool HitGround()
    {
        return this.hitGround;
    }

}
