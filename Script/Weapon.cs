using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //音系
    public AudioSource weapon;
    public AudioClip reloadingSE, fireSE, triggerSE;

    public static Weapon instance;

    public Transform shotDirection;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CanShoot()
    {
        GameState.canShoot = true;
    }

    public void FireSE()
    {
        weapon.clip = fireSE;
        weapon.Play();
    }
    public void ReloadSE()
    {
        weapon.clip = reloadingSE;
        weapon.Play();
    }
    public void TriggerSE()
    {   //音が鳴っていないとき（音が重なるのを防ぐ）
        if(!weapon.isPlaying)
        {
            weapon.clip = triggerSE;
            weapon.Play();
        }
    }

    //当たり判定（レーザーを飛ばす）
    public void Shooting()
    {
        //レーザーに当たったオブジェクトの情報を格納
        RaycastHit hitInfo;
                            //レーザーを飛ばす位置 , 飛ばす方向,  out(=>中身が空でも引数として設定できる)
        if(Physics.Raycast(
            shotDirection.transform.position, shotDirection.transform.forward, out hitInfo ,300))
        {   //レーザーに当たったオブジェクトを格納
            GameObject hitGameObject = hitInfo.collider.gameObject;

            //レーザーに当たったオブジェクトがZombieControllerを所持しているか判定
            if(hitInfo.collider.gameObject.GetComponent<ZombieController>() != null)
            {
                ZombieController hitZombie = 
                    hitInfo.collider.gameObject.GetComponent<ZombieController>();

                if(Random.Range(0, 10) > 5)
                {
                    hitZombie.ZombieDeath();

                    GameObject rdPrefab = hitZombie.ragdoll;
                    GameObject NewRD = 
                        Instantiate(
                            rdPrefab, 
                            hitGameObject.transform.position, 
                            hitGameObject.transform.rotation);
                            
                    NewRD.transform.Find("mixamorig:Hips").GetComponent<Rigidbody>()
                        .AddForce(shotDirection.transform.forward * 1000);
                    Destroy(hitGameObject);
                }
                else
                {
                    hitZombie.ZombieDeath();
                }
            }
        }
    }
}
