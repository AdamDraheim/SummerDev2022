using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaveBlock : MonoBehaviour
{

    private MeshFilter filter;

    // Start is called before the first frame update
    void Start()
    {
        filter = this.GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        MakeWaves();
    }

    private void MakeWaves()
    {
        Vector3[] vertices = filter.mesh.vertices;



        for (int idx = 0; idx < vertices.Length; idx++)
        {
            
            vertices[idx].y = WaveManager.waveManager.CalculateWaves(vertices[idx] + this.transform.position);

        }

        filter.mesh.vertices = vertices;
        filter.mesh.RecalculateNormals();
    }
}
