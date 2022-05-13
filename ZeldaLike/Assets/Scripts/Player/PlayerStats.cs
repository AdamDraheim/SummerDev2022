using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private int maxHealth;

    [SerializeField]
    [Tooltip("How long the player is invincible before taking damage again")]
    private float damageCooldown;

    private int currHealth;
    private float currCooldown;

    // Start is called before the first frame update
    void Start()
    {
        this.currHealth = this.maxHealth;        
    }

    // Update is called once per frame
    void Update()
    {
        //Decrease time for damage cooldown if positive
        this.currCooldown -= (this.currCooldown > 0 ? Time.deltaTime : 0);

        if (Input.GetKeyDown(KeyCode.P))
        {
            Damage();
        }
    }

    /// <summary>
    /// Damages the player for 1 hit if the damage cooldown is not active
    /// </summary>
    public void Damage()
    {
        if (this.currCooldown <= 0)
        {
            this.currHealth -= 1;

            if (this.currHealth <= 0)
            {
                Die();
            }
            else
            {
                this.currCooldown = this.damageCooldown;
            }
        }
    }

    private void Die()
    {
        Debug.Log("DEAD");
    }
}
