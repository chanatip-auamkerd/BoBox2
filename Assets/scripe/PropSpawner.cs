using System.Collections.Generic;
using UnityEngine;

public class PropSpawner : MonoBehaviour
{
    public static PropSpawner Instance;

    [Header("Prop Prefabs")]
    [Tooltip("ใส่ Prefab สิ่งกีดขวางแบบต่างๆ เช่น แผ่นไม้, ลูกบอล, สามเหลี่ยม")]
    public GameObject[] propPrefabs;

    [Header("Spawn Count")]
    public int minProps = 8;   
    public int maxProps = 13;  

    [Header("Spawn Area")]
    public float minX = -4.2f; 
    public float maxX = 3.2f;  
    public float minY = 0.2f;  
    public float maxY = 4.2f; 

    [Header("Spawn Spacing & Rules")]
    [Tooltip("ระยะห่างขั้นต่ำระหว่าง Prop แต่ละชิ้น ยิ่งค่าน้อยยิ่งเกิดได้ชิดและหนาแน่น")]
    public float minDistanceBetweenProps = 0.9f;
    [Tooltip("จำนวนรอบการพยายามหาที่ว่าง ยิ่งเยอะยิ่งยัดของลงได้เยอะ")]
    public int maxPlacementAttempts = 70;

    [Header("Random Variations")]
    public bool randomRotation = true;
    public float minAngle = -45f;
    public float maxAngle = 45f;
    public Vector2 scaleRange = new Vector2(0.7f, 1.1f);

    private List<Vector2> spawnedPositions = new List<Vector2>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RespawnAllProps();
    }
    public void RespawnAllProps()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        spawnedPositions.Clear();

        if (propPrefabs == null || propPrefabs.Length == 0)
        {
            Debug.LogWarning("⚠️ ยังไม่ได้ใส่ Prop Prefabs ใน PropSpawner!");
            return;
        }
        int propsToSpawn = Random.Range(minProps, maxProps + 1);

        for (int i = 0; i < propsToSpawn; i++)
        {
            Vector2 spawnPos = Vector2.zero;
            bool foundValidPosition = false;

            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                float randX = Random.Range(minX, maxX);
                float randY = Random.Range(minY, maxY);
                Vector2 candidatePos = new Vector2(randX, randY);

                if (IsPositionValid(candidatePos))
                {
                    spawnPos = candidatePos;
                    foundValidPosition = true;
                    break;
                }
            }

            if (foundValidPosition)
            {
                GameObject selectedPrefab = propPrefabs[Random.Range(0, propPrefabs.Length)];
                Quaternion rotation = Quaternion.identity;
                if (randomRotation)
                {
                    rotation = Quaternion.Euler(0, 0, Random.Range(minAngle, maxAngle));
                }

                GameObject prop = Instantiate(selectedPrefab, spawnPos, rotation, transform);
                float randomScale = Random.Range(scaleRange.x, scaleRange.y);
                prop.transform.localScale = prop.transform.localScale * randomScale;

                spawnedPositions.Add(spawnPos);
            }
        }
    }

    private bool IsPositionValid(Vector2 candidatePos)
    {
        foreach (Vector2 pos in spawnedPositions)
        {
            if (Vector2.Distance(candidatePos, pos) < minDistanceBetweenProps)
            {
                return false;
            }
        }
        return true;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}