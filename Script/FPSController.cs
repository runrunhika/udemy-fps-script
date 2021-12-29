using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class FPSController : MonoBehaviour
{
    float x, z;
    float speed = 0.1f;
    public GameObject cam;
    //カメラの回転を取得するための変数
    Quaternion cameraRot, characterRot;
    //Aimの感度
    float Xsensityvity = 3f, Ysensityvity = 3f;
    //マウスカーソルの on/off
    bool cursorLock = true;
    //視点角度の制限用の変数
    float minX = -90f, maxX = 90f;
    public Animator animator;
        //所持弾薬・最高所持弾薬・マガジン内の弾薬・マガジン内の最大数
    int ammunition = 50, maxAmmunition = 50, ammoclip = 10, maxAmmoClip = 10;
    int playerHP = 100, maxPlayerHP = 100;
    public Slider hpBar;
    public Text ammoText;
    public GameObject gameOverText;  //TextだとSetActiveを使えない
    public GameObject gameClearText;
    public GameObject mainCamera, subCamera;
    //音系
    public AudioSource playerFootStep;
    public AudioClip WalkFootStepSE, RunFootStepSE;
    //回復（数値）
    public int ammoBox, medBox;
    //Volume:画面状態
    public PostProcessVolume volume;
    float damageDisplay;

    Rigidbody rb;
    public LayerMask groundLayer;
    public Transform groundCheckPoint;

    void Start()
    {
        //カメラとPlayerの角度を取得
        cameraRot = cam.transform.localRotation;
        characterRot = transform.localRotation;

        GameState.canShoot = true;
        GameState.GameOver = false;

        gameOverText.SetActive(false);
        gameClearText.SetActive(false);

        hpBar.value = playerHP;
        ammoText.text = ammoclip + "/" + ammunition;

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        DisplayChange();

        if (GameState.GameOver)
        {
            return;
        }

        //マウスの水平&垂直方向の移動を取得      //感度
        float xRot = Input.GetAxis("Mouse X") * Ysensityvity;
        float yRot = Input.GetAxis("Mouse Y") * Xsensityvity;
        //マウスの水平&垂直方向の移動を反映
        cameraRot *= Quaternion.Euler(-yRot, 0, 0);
        characterRot *= Quaternion.Euler(0, xRot, 0);
        //視点角度の制限
        cameraRot = ClampRotation(cameraRot);
        //実際のカメラとPlayerに反映
        cam.transform.localRotation = cameraRot;
        transform.localRotation = characterRot;

         UpdateCCursorLock();

         if(Input.GetMouseButton(0) && GameState.canShoot)
         {
             if(ammoclip > 0)
             {
                animator.SetTrigger("Fire");
                GameState.canShoot = false;

                ammoclip--;
                AmmoUpdate();
             }
             else
             {
                Weapon.instance.TriggerSE();
             }
         }

         if(Input.GetKeyDown(KeyCode.R))
         {
            int amountNeed = maxAmmoClip - ammoclip;
            int ammoAvailable = amountNeed < ammunition ? amountNeed : ammunition;

            if(amountNeed != 0 && ammunition != 0)
            {
                animator.SetTrigger("Reload");
                ammunition -= ammoAvailable;
                ammoclip += ammoAvailable;
                AmmoUpdate();
            }
         }


        if (IsGround())
        {
            if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
            {
                if (!animator.GetBool("Walk"))
                {
                    animator.SetBool("Walk", true);
                }
                //足音が鳴っていない & 走っていない　場合
                if (!FootStepCheck() && animator.GetBool("Run"))
                {
                    PlayerWalkFootStep(WalkFootStepSE);
                }
            }
            else if (animator.GetBool("Walk"))
            {
                animator.SetBool("Walk", false);

                StopFootStep();
            }

            if (z > 0 && Input.GetKey(KeyCode.LeftShift))
            {
                if (!animator.GetBool("Run"))
                {
                    animator.SetBool("Run", true);
                    speed = 0.25f;
                }
                else if (!FootStepCheck())
                {
                    PlayerRunFootStep(RunFootStepSE);
                }
            }
            else if (animator.GetBool("Run"))
            {
                animator.SetBool("Run", false);
                speed = 0.1f;

                StopFootStep();
            }
        }
        else
        {
            //ジャンプ中に足音を鳴らさない
            StopFootStep();
        }

        

        if(Input.GetMouseButton(1))
        {
            subCamera.SetActive(true);
            mainCamera.GetComponent<Camera>().enabled = false;
        }
        else if(subCamera.activeSelf)
        {
            subCamera.SetActive(false);
            mainCamera.GetComponent<Camera>().enabled = true;
        }

        Jump();
    }

    //Player移動 (KeyBoardの値を受けっとっている)
    private void FixedUpdate()
    {
        if(GameState.GameOver)
        {
            return;
        }

        x = 0;
        z = 0;

        x = Input.GetAxisRaw("Horizontal") * speed;
        z = Input.GetAxisRaw("Vertical") * speed;

        //移動時　跳ねるバグ修正
        //原因 カメラのYに値が入っていたから
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;

        //現在地に入力された値分、移動させる
        // transform.position += new Vector3(x, 0, z);

        //カメラが向いている方向にPlayerが移動する
         transform.position += camForward * z + cam.transform.right * x;
    }

    //マウスカーソルの表示を切り替える変える
    public void UpdateCCursorLock()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLock = false;
        }
        else if(Input.GetMouseButton(0))
        {
            cursorLock = true;
        }

        if(cursorLock)
        {
            //カーソル非表示
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if(!cursorLock)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    //視点角度の制限
    private Quaternion ClampRotation(Quaternion q)
    {
        //q = x,y,z,w = (x,y,z = ベクトル（量と向き)   w = スカラー(重さ・体重))
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1f;
        //視点角度をQuaternion => ボイラーに変換(摂氏にする)
        float angleX = Mathf.Atan(q.x) * Mathf.Rad2Deg * 2f;
        //視点可動域を設定
        angleX = Mathf.Clamp(angleX, minX, maxX);
        //Quaternion型に戻す
        q.x = Mathf.Tan(angleX * Mathf.Deg2Rad * 0.5f);
        return q;
    }

    //Player歩きSE
    public void PlayerWalkFootStep(AudioClip clip)
    {
        playerFootStep.loop = true;
        //音の高さ
        playerFootStep.pitch = 1f;
        playerFootStep.clip = clip;
        playerFootStep.Play();
    }

    //Player走りSE
    public void PlayerRunFootStep(AudioClip clip)
    {
        playerFootStep.loop = true;
        playerFootStep.pitch = 1.3f;
        playerFootStep.clip = clip;
        playerFootStep.Play();
    }

    //音を止める
    public void StopFootStep()
    {
        playerFootStep.Stop();
        playerFootStep.loop = false;
        playerFootStep.pitch = 1f;
    }

    //HPを減らす
    public void TakeHit(float damage)
    {
        playerHP = (int)Mathf.Clamp(playerHP - damage, 0, playerHP);

        HPUpdate();

        if (playerHP <= 0 && !GameState.GameOver)
        {
            GameState.GameOver = true;
            gameOverText.SetActive(true);

            Invoke("Restart", 3f);
        }
    }

    public void Restart()
    {
        //カーソルを表示させる
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Title");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Goal")
        {
            gameClearText.SetActive(true);

            Invoke("Restart", 3f);
        }
        else if(other.tag == "Ammo")
        {
            //最高所持弾薬　＞　所持弾薬
            if (maxAmmunition > ammunition)
            {
                ammunition += ammoBox;

                //最高所持弾薬を超えた場合
                if (maxAmmunition < ammunition)
                {
                    ammunition = maxAmmunition;
                }
                AmmoUpdate();

                Destroy(other.gameObject);
            }
        }
        else if (other.tag == "Med")
        {
            if (maxPlayerHP > playerHP)
            {
                playerHP += medBox;

                if (maxPlayerHP < playerHP)
                {
                    playerHP = maxPlayerHP;
                }

                HPUpdate();

                Destroy(other.gameObject);
            }
        }
    }

    public void HPUpdate()
    {
        hpBar.value = playerHP;
    }

    public void AmmoUpdate()
    {
        ammoText.text = ammoclip + "/" + ammunition;
    }

    //体力判定　画面の状態を設定：画面を徐々に明るくする
    public void DisplayChange()
    {
        if(playerHP > 40)
        {
            damageDisplay = 0f;
        }
        else if(playerHP <= 40 && playerHP > 20)
        {
            damageDisplay = 0.5f;
        }
        else if(playerHP <= 20 && playerHP > 10)
        {
            damageDisplay = 0.8f;
        }
        else if (playerHP <= 10 && playerHP > 0)
        {
            damageDisplay = 1f;
        }

        volume.weight = damageDisplay;
    }

    //ジャンプ
    public void Jump()
    {
        if (IsGround() && Input.GetKeyDown(KeyCode.Space)) 
        {
            //Yへ力を加える   ForceMode.Impulse : 瞬時に力を加える
            rb.AddForce(new Vector3(0, 4, 0), ForceMode.Impulse);
        }
    }

    //Playerが地面と接触しているか判定
    public bool IsGround()
    {
        return Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayer);
    }

    //足音が鳴っているか判定
    public bool FootStepCheck()
    {
        //鳴っていたら true
        return playerFootStep.isPlaying;
    }
}
