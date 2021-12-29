using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawn : MonoBehaviour
{
    public GameObject[] zombiePrefab;
    public int number;
    public float spawnRadius;
    public bool spawnOnStart;

    // Start is called before the first frame update
    void Start()
    {
        if (spawnOnStart)
        {
            SpawnAll();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnAll()
    {
        for(int i = 0; i < number; i++)
        {
            //Create to Zombie Position Range Random
            //insideUnitSphere = 半径が１の球体からランダムな数値を取得できる
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;

            int randomIndex = RandomIndex(zombiePrefab);

            NavMeshHit hit;

            //生成位置がNavMeshの上だった時
            //SamplePosition = もしNavMesh外に出てしまっている場合はそこから一番近いNavMeshのPointを取得
            if (NavMesh.SamplePosition(randomPos, out hit, 5.0f, NavMesh.AllAreas))
            {
                Instantiate(zombiePrefab[randomIndex], randomPos, Quaternion.identity);
            }
            //生成位置が地中や空中の場合
            else
            {
                i--;
            }
        }
    }

    public int RandomIndex(GameObject[] games)
    {
        return Random.Range(0, games.Length);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            SpawnAll();
        }
    }
}
