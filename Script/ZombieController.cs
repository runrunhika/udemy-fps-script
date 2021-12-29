using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    Animator animator;
    NavMeshAgent agent;
    public float walkingSpeed;

    enum STATE {IDLE, WANDER, ATTACK, CHASE, DEAD};
    //初期状態設定
    STATE state = STATE.IDLE;

    GameObject target;
    public float runSpeed;
    public int attackDamage;

    public GameObject ragdoll;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if(target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player");
        }
    }

    //モーションを止める
    public void TurnOffTrigger()
    {
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetBool("Attack", false);
        animator.SetBool("Death", false);
    }

    //Playerとの距離を測量
    float DistanceToPlayer()
    {
        if(GameState.GameOver)
        {
            return Mathf.Infinity;
        }
        return Vector3.Distance(target.transform.position, transform.position);
    }

    //既定の距離よりも近い場合、Playerを見つけたという判定を行う
    bool CanSeePlayer()
    {
        if(DistanceToPlayer() < 15)
        {
            return true;
        }

        return false;
    }

    //Playerに逃げられ、見失ったとき
    bool ForGetPlayer()
    {   //見失う
        if(DistanceToPlayer() > 16)
        {
            return true;
        }
        //見失ってない
        return false;
    }

    //攻撃
    public void DamagePlayer()
    {
        if(target != null)
        {
            target.GetComponent<FPSController>().TakeHit(attackDamage);
        }
    }

    //死んだとき
    public void ZombieDeath()
    {
        TurnOffTrigger();
        animator.SetBool("Death", true);
        state = STATE.DEAD;
    }

    // Update is called once per frame
    void Update()
    {
        //列挙型を使う
        switch(state)
        {
            case STATE.IDLE:
                //モーションを止める
                TurnOffTrigger();

                if(CanSeePlayer())
                {
                    state = STATE.CHASE;
                }
                //確率でWANDER状態にする
                else if(Random.Range(0, 5000) < 5)
                {
                    state = STATE.WANDER;
                }
                break;

            case STATE.WANDER:
                //目的地がないとき
                if(!agent.hasPath)
                {
                    float newX = transform.position.x + Random.Range(-5, 5);
                    float newZ = transform.position.x + Random.Range(-5, 5);

                    Vector3 NextPos = new Vector3(newX, transform.position.y, newZ);
                    //目的の設定
                    agent.SetDestination(NextPos);
                    //目的地との距離が0になると止まる
                    agent.stoppingDistance = 0;

                    TurnOffTrigger();

                    agent.speed = walkingSpeed;
                    animator.SetBool("Walk", true);
                }

                if(Random.Range(0, 5000) < 5)
                {
                    state = STATE.IDLE;
                    agent.ResetPath();
                }

                if(CanSeePlayer())
                {
                    state = STATE.CHASE;
                }
                break;

            case STATE.CHASE:
                //GameOver
                if(GameState.GameOver)
                {
                    TurnOffTrigger();
                    agent.ResetPath();
                    state = STATE.WANDER;

                    return;
                }
                //目的地をPlayerに設定する = Playerを追いかける
                agent.SetDestination(target.transform.position);
                //目的地到着で止まる (3の理由は、0だとPlayerに重なってしまうから)
                agent.stoppingDistance = 3;

                TurnOffTrigger();

                agent.speed = runSpeed;
                animator.SetBool("Run", true);

                //目的地までの距離 <= 3
                if(agent.remainingDistance <= agent.stoppingDistance)
                {
                    state = STATE.ATTACK;
                }

                //Playerを見失った
                if(ForGetPlayer())
                {
                    agent.ResetPath();
                    state = STATE.WANDER;
                }
                break;
            case STATE.ATTACK:
                //GameOver
                if(GameState.GameOver)
                {
                    TurnOffTrigger();
                    agent.ResetPath();
                    state = STATE.WANDER;

                    return;
                }
                TurnOffTrigger();
                animator.SetBool("Attack", true);

                //Playerのほうを向く
                transform.LookAt(
                    new Vector3(
                        target.transform.position.x, 
                        transform.position.y, 
                        target.transform.position.z));

                if(DistanceToPlayer() > agent.stoppingDistance + 2)
                {
                    state = STATE.CHASE;
                }
                break;
            
            case STATE.DEAD:
                Destroy(agent);

                gameObject.GetComponent<DestroyZombie>().DeadZombie();
                break;
        }
    }
}
