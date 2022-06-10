using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T>
{

    private int size;
    private PQNode<T> root;

    public PriorityQueue(){
        this.size = 0;
    }

    public void Enqueue(T element, float priority)
    {
        PQNode<T> newNode = new PQNode<T>(element, priority);
        size += 1;

        if(root == null)
        {
            root = newNode;
        }
        else
        {
            if(root.GetPriority() > priority)
            {
                newNode.SetNextNode(root);
                root = newNode;
            }
            else
            {
                root.SetNextNode(newNode);
            }
        }
    }

    public T Dequeue()
    {
        if(root == null)
        {
            return default(T);
        }

        T val = root.GetKey();

        root = root.GetNext();
        size -= 1;
        return val;
    }

    public int GetSize()
    {
        return this.size;
    }

}

class PQNode<T>
{

    private T key;
    private float value;
    private PQNode<T> next;

    public PQNode(T key, float value)
    {
        this.key = key;
        this.value = value;
    }

    public void SetNextNode(PQNode<T> node)
    {
        if(this.next == null)
        {
            this.next = node;
            return;
        }

        if(node.value < this.next.value)
        {
            PQNode<T> temp = this.next;
            this.next = node;
            this.next.SetNextNode(temp);
        }
        else
        {
            this.next.SetNextNode(node);
        }
    }

    public float GetPriority()
    {
        return this.value;
    }

    public T GetKey()
    {
        return key;
    }
    public PQNode<T> GetNext()
    {
        return this.next;
    }
}