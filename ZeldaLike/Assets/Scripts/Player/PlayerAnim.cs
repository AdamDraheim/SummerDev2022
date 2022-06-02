using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnim : MonoBehaviour
{

    public Animator anim;
    public Vector2 facingDir;
    public float animMoveThreshold;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 vel = this.GetComponent<PlayerMovement>().GetHorizVel();
        //Player is considered moving if there is player input or player model still moving after input, and is on the ground
        bool moving = (this.GetComponent<PlayerMovement>().HasInput() || vel.magnitude >= animMoveThreshold) 
            && this.GetComponent<PlayerMovement>().IsGrounded();

        anim.SetBool("Moving", moving);

        if (moving)
        {
            

            if (!vel.Equals(Vector2.zero))
            {
                this.transform.rotation = Quaternion.FromToRotation(Vector3.forward, new Vector3(vel.x, 0, vel.y));
            }
        }

    }

    private void OnValidate()
    {
        this.transform.rotation = Quaternion.FromToRotation(Vector3.forward, new Vector3(facingDir.x, 0, facingDir.y));
    }
}
