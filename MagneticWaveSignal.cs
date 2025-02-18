using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticWaveSignal : MonoBehaviour
{
    public int pointsPerWave = 50;
    public float waveRadius = 1.0f;
    public float waveGrowthSpeed = 1.0f;
    public float waveLifetime = 3.0f;
    public Material waveMaterial;
    public int maxWaves = 5;
    public float baseWidth = 0.05f;
    public float topWidth = 0.15f;
    [Header("Настройки асимметрии")]
    [Range(0f, 2f)] public float leftSideScaleFactor = 0.5f;
    [Range(1f, 3f)] public float rightSideScaleFactor = 1.5f;
    [Header("Растяжение волны")]
    public float delayTime = 1.0f;
    public float stretchSpeed = 1.0f;
    [Header("Начальные размеры волны")]
    public float initialWaveScaleX = 1.0f;
    public float initialWaveScaleY = 2.0f;

    private Queue<GameObject> activeWaves = new Queue<GameObject>();
    private Coroutine stretchCoroutine;
    void Start()
    {
        StartCoroutine(GenerateWaves());
    }
    IEnumerator GenerateWaves()
    {
        while (true)
        {
            if (activeWaves.Count < maxWaves)
            {
                CreateWave();
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
    void CreateWave()
    {
        GameObject waveObject = new GameObject("MagneticWave");
        waveObject.transform.SetParent(transform);
        LineRenderer lineRenderer = waveObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = pointsPerWave;
        lineRenderer.material = waveMaterial;
        lineRenderer.startWidth = baseWidth;
        lineRenderer.endWidth = topWidth;
        activeWaves.Enqueue(waveObject);
        StartCoroutine(AnimateWave(lineRenderer, waveObject));
    }

    IEnumerator AnimateWave(LineRenderer lineRenderer, GameObject waveObject)
    {
        float radius = waveRadius;
        float timeAlive = 0;
        while (timeAlive < waveLifetime)
        {
            radius += waveGrowthSpeed * Time.deltaTime;
            DrawDrop(lineRenderer, radius, timeAlive / waveLifetime);
            timeAlive += Time.deltaTime;
            yield return null;
        }
        if (activeWaves.Count > 0)
        {
            GameObject waveToDestroy = activeWaves.Dequeue();
            Destroy(waveToDestroy);
        }
    }

    void DrawDrop(LineRenderer lineRenderer, float radius, float progress)
    {
        Vector3[] points = new Vector3[pointsPerWave];
        for (int i = 0; i < pointsPerWave; i++)
        {
            float angle = Mathf.Lerp(-90f, 270f, i / (float)(pointsPerWave - 1));
            float radian = angle * Mathf.Deg2Rad;
            float scale = Mathf.Pow(Mathf.Cos(radian), 4);
            float scaledRadius = radius * scale;
            float asymmetricScaleX = 1f;
            float asymmetricScaleY = 1f;

            if (angle < 0)
            {
                asymmetricScaleX = 1f;
                asymmetricScaleY = Mathf.Lerp(1f, leftSideScaleFactor, Mathf.InverseLerp(-90f, 0f, angle));
            }
            else
            {
                asymmetricScaleX = Mathf.Lerp(1f, rightSideScaleFactor, Mathf.InverseLerp(0f, 270f, angle));
                asymmetricScaleY = 1f;
            }
            float finalRadiusX = scaledRadius * asymmetricScaleX * initialWaveScaleX;
            float finalRadiusY = scaledRadius * asymmetricScaleY * initialWaveScaleY;
            float x = Mathf.Cos(radian) * finalRadiusX;
            float y = Mathf.Sin(radian) * finalRadiusY;
            float z = 0;

            points[i] = new Vector3(x, y, z);
        }

        lineRenderer.SetPositions(points);
        float startWidth = Mathf.Lerp(baseWidth, topWidth, progress);
        float endWidth = Mathf.Lerp(baseWidth, topWidth, progress);
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
    }
    public void IncreaseRightWaveLength(float flarePower)
    {
        if (stretchCoroutine != null)
        {
            StopCoroutine(stretchCoroutine);
        }
        stretchCoroutine = StartCoroutine(StretchRightWave(flarePower));
    }
    IEnumerator StretchRightWave(float flarePower)
    {
        yield return new WaitForSeconds(delayTime);

        while (rightSideScaleFactor < flarePower)
        {
            rightSideScaleFactor += Time.deltaTime * stretchSpeed;
            yield return null;
        }
    }
}
