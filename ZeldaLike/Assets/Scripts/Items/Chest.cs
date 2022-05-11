using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{
    public GameObject pivot;
    public float open_speed;
    public float maxOpenAngle;
    private bool isOpen;
    private float curr_open_time;

    protected override void Interact()
    {
        isOpen = true;
    }

   
    protected new void Update()
    {
        base.Update();
        OpenChest();
    }

    private void OpenChest()
    {
        if (isOpen)
        {

            curr_open_time += open_speed * Time.deltaTime;

            curr_open_time = (curr_open_time > 1 ? 1 : curr_open_time);

            float angle = maxOpenAngle * Bezier(curr_open_time);

            pivot.transform.localRotation = Quaternion.Euler(-angle, 0, 0);
            
        }
    }

    private float Bezier(float t_step)
    {
        //p1 is always 0
        float p2 = 0.7f * 3 * t_step * Mathf.Pow(1 - t_step, 2);
        float p3 = 1.5f * 3 * t_step * t_step * (1 - t_step);
        float p4 = 1 * Mathf.Pow(t_step, 3);
        return p2 + p3 + p4;
    }

}
