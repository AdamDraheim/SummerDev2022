using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{

    public int maxHealth;
    public float damageCooldown;
    protected float currDamCooldown;
    protected int health;

    // Start is called before the first frame update
    void Start()
    {
        this.health = maxHealth;
    }

    // Update is called once per frame
    protected void Update()
    {
        this.currDamCooldown -= Time.deltaTime;
        this.currDamCooldown = (this.currDamCooldown <= 0 ? 0 : this.currDamCooldown);
    }

    public void Damage()
    {
        if (this.currDamCooldown <= 0)
        {
            this.health -= 1;
            this.currDamCooldown = damageCooldown;
        }
    }
}
