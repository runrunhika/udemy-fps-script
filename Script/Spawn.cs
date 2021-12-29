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
            //insideUnitSphere = ���a���P�̋��̂��烉���_���Ȑ��l���擾�ł���
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;

            int randomIndex = RandomIndex(zombiePrefab);

            NavMeshHit hit;

            //�����ʒu��NavMesh�̏ゾ������
            //SamplePosition = ����NavMesh�O�ɏo�Ă��܂��Ă���ꍇ�͂��������ԋ߂�NavMesh��Point���擾
            if (NavMesh.SamplePosition(randomPos, out hit, 5.0f, NavMesh.AllAreas))
            {
                Instantiate(zombiePrefab[randomIndex], randomPos, Quaternion.identity);
            }
            //�����ʒu���n����󒆂̏ꍇ
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
