using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaveManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How tall waves are")]
    private float waveSize;
    [SerializeField]
    [Tooltip("How far apart a wave crest is")]
    private float waveWidth;
    [SerializeField]
    [Tooltip("How fast the waves move")]
    private float waveStrength;

    public Vector2 windDirection;

    public static WaveManager waveManager;

    private float time;
    // Start is called before the first frame update
    void Start()
    {
        waveManager = this;
        time = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!windDirection.Equals(Vector2.zero))
        {
            windDirection = windDirection.normalized;
        }
        else
        {
            windDirection = Vector2.up;
        }

        time += Time.deltaTime * waveStrength;

        if (time >= 2 * Mathf.PI)
            time -= (2 * Mathf.PI);
    }

    public float CalculateWaves(Vector3 pos)
    {
        float x_dir = waveSize * Mathf.Sin((pos.x + time) * waveWidth);
        float z_dir = waveSize * Mathf.Sin((pos.z + time) * waveWidth);

        float y_val = x_dir * windDirection.x + z_dir * windDirection.y;
        return y_val;
    }
}
