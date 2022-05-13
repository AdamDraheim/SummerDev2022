using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{

    public PlayerStats player;
    public GameObject heartIcon;

    private Stack<GameObject> hearts;


    // Start is called before the first frame update
    void Start()
    {
        hearts = new Stack<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            int currHealth = player.GetHealth();
            int numHearts = hearts.Count;

            if(currHealth > numHearts)
            {
                GameObject newHeart = Instantiate(heartIcon);
                newHeart.transform.parent = this.transform;
                hearts.Push(newHeart);
            }
            else if(currHealth < numHearts)
            {
                GameObject lostHeart = hearts.Pop();
                Destroy(lostHeart);
            }
        }
    }
}
