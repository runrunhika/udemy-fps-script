using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyZombie : MonoBehaviour
{
    public bool rd;

    //�h���b�v�m��
    public int dorpToChance;
    //�h���b�v�A�C�e��
    public GameObject[] items;

    private bool Judgment;

    // Start is called before the first frame update
    void Start()
    {
        Judgment = true;

        if(rd)
        {
            DeadZombie();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeadZombie()
    {
        if(Judgment )
        {
            Judgment = false;
        }
        else
        {
            return;
        }

        Invoke("DestroyGameObject", 3f);

        //�A�C�e���h���b�v����
        if(Random.Range(0, 100) < dorpToChance)
        {
            Instantiate(items[Random.Range(0, items.Length)], transform.position, transform.rotation);
        }
    }

    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
