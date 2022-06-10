using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Enemy
{
    [Header("Animation")]
    public Animator anim;
    public Vector2 facingDir;

    [Header("Player Interaction")]
    public float speed;
    public float attackDistance;
    public float targetDistance;

    private bool isMoving;
    private bool isAttacking;
    private GameObject player;
    private Vector3 dir;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectOfType<PlayerMovement>().gameObject;
    }

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
        isMoving = false;
        isAttacking = false;
        if (Vector3.Distance(this.transform.position, player.transform.position) <= targetDistance)
        {
            if (Vector3.Distance(this.transform.position, player.transform.position) > attackDistance)
            {
                MoveTowardPlayer();
                isMoving = true;
            }
            else
            {
                Debug.Log("ATTACK");
                isAttacking = true;
            }
        }
        Animate();

    }

    private void MoveTowardPlayer()
    {
        dir = Nodemap.nodeMapManager.GetFlowDirectionAtPosition(this.transform.position);
        dir = (0.5f * dir) + 0.5f * -(this.transform.position - player.transform.position).normalized;
        this.transform.position += dir * speed * Time.deltaTime;
    }

    private void Animate()
    {
        this.anim.SetBool("Moving", isMoving);
        if (!isMoving && isAttacking)
        {
            this.anim.SetBool("Attacking", isMoving);
        }

        if (!dir.Equals(Vector2.zero))
        {
            this.transform.rotation = Quaternion.FromToRotation(Vector3.forward, new Vector3(dir.x, 0, dir.z));
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position, this.attackDistance);
        Gizmos.DrawWireSphere(this.transform.position, this.targetDistance);
    }
}
