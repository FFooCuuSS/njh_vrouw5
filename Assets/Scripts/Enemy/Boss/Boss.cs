using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class Boss : MonoBehaviour
{

    [SerializeField] protected float hp = 100;
    [SerializeField] protected float attackSpeed = 8f;

    protected int page = 1;
    private int attackPatern = 0;
    public bool canAttack = false;
    private int attackStack = 0;
    bool eyeattack = false;
    public Animator anim; 

    public Collider2D headCollider;

    private float groggyTime = 10f;
    bool isGroggy=false;


    [Header("Leg Patern")]
    public GameObject patern2;//Leg Patern
    BossPatern2 legAttack;


    [Header("Fixed Leg")]
    [SerializeField] private GameObject FixedLegPrefab;
    [SerializeField] private GameObject fixedLeg1;
    [SerializeField] private GameObject fixedLeg2;
    [SerializeField] private FixedLeg fLeg1;
    [SerializeField] private FixedLeg fLeg2;//--------------------------------------------------------------------------고정다리 스크립트
    [SerializeField] List<Vector2> leftFixedLegSpawnPoint;
    [SerializeField] List<Vector2> RightFixedLegSpawnPoint;


    [Header("Arousal Patern")]
    [SerializeField] private BossEyePattern eyeAttack;
    [SerializeField] private ThunderEffect thunderEffect;
    [SerializeField] private Animator ArousalScreenEffect;

    [Header("FallDown Patern")]
    [SerializeField] private BossFallDownPatern fallDownAttack;

    [Header("Suck Patern")]
    [SerializeField] private BossSuckPatern suckAttack;


    private void Awake()
    {
        legAttack = patern2.GetComponent<BossPatern2>();
        InstantiateFixedLeg();
        if (anim != null) GetComponent<Animator>();
        CanAttack();
    }
    //공격 명령
    private void Update()
    {
        if (canAttack)
        {
            Patern();
            StartCoroutine(CanAttack());
        }
        //두 다리 모두 Hp가 0보다 작을 경우 그로기 상태
        if (fLeg1== null && fLeg2== null && page == 1&&isGroggy==false) //-------------------------------------------------------------------
        {
            StartCoroutine(Groggy());
        }


    }
    //패턴 고르기
    private void Patern()
    {
        if(page==1)
        {
            attackPatern = Random.Range(1, 4);//1~3까지 포함
            
            switch (attackPatern)
            {
                case 1:
                case 2:
                    legAttack.Attack();
                    Debug.Log("legAttack");
                    break;
                case 3:
                    Debug.Log("fallDownAttack");
                    fallDownAttack.Attack();
                    break;
                default:
                    break;

            }
        }
        else if(page==2) 
        {
            if(attackStack==2)
            {
                attackPatern = Random.Range(0, 3);//0~2까지 포함
                attackStack = 0;
                if (eyeattack) attackPatern = 2;//각성 패턴 중에는 내려치기와 기본공격만 실행

                switch(attackPatern)
                {
                    case 0:
                        //각성 패턴
                        eyeAttack.SpawnEye(this.gameObject);
                        Debug.Log("ArousalAttack");
                        StartCoroutine(ReinforceAttack());
                        break;
                        
                    case 1:
                        Debug.Log("SuckAttack");
                        anim.SetBool("IsSuck", true);
                        suckAttack.SuckAttack();
                        
                        break;

                    case 2:
                        Debug.Log("FalldownAttack");
                        fallDownAttack.Attack();
                        break;
                }
            }
            else//a-a-b
            {
                legAttack.Attack();
                attackStack++;
            }
        }

    }
    //피격
    public virtual void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 60 && page == 1) 
        {
            Debug.Log("Page 2");
            headCollider.enabled = true;
            attackSpeed = 5f;
            page = 2;
        }
        if (hp <= 0)
        {
            Debug.Log("Boss die");
            anim.SetBool("Die",true);

            //효과음
            SoundManager.Instance.PlaySFX(23);

            Destroy(this.gameObject,1f);

        }
           
    }
    //공격 텀
    private IEnumerator CanAttack()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackSpeed);
        canAttack = true;
    }
    void TriggerSuckFalse()
    {
        anim.SetBool("IsSuck", false);
    }

    public IEnumerator Groggy()//각 고정 다리가 사라졌을 경우 이거 실행됨// 고정다리 부분에서 이거 실행시켜야됨
    {

        canAttack = false;
        isGroggy = true;
        headCollider.enabled = true;
        anim.SetBool("IsGroggy",true);
        yield return new WaitForSeconds(groggyTime);
        headCollider.enabled = false;
        canAttack = true;
        isGroggy = false;
        anim.SetBool("IsGroggy", false);
        if (hp >= 60) 
        {
            InstantiateFixedLeg();
        }
        else
        {
            headCollider.enabled = true;
        }
    }

    private void InstantiateFixedLeg()
    {
        int randomNum1 = Random.Range(0, leftFixedLegSpawnPoint.Count);
        int randomNum2 = Random.Range(0, RightFixedLegSpawnPoint.Count);
        fixedLeg1 = Instantiate(FixedLegPrefab, leftFixedLegSpawnPoint[randomNum1], Quaternion.identity);
        fixedLeg2 = Instantiate(FixedLegPrefab, RightFixedLegSpawnPoint[randomNum2], Quaternion.identity);

        fLeg1 = fixedLeg1.GetComponent<FixedLeg>();
        fLeg2 = fixedLeg2.GetComponent<FixedLeg>();
        fLeg1.SetParent(this);
        fLeg2.SetParent(this);
    }
    

    public IEnumerator ReinforceAttack()
    {
        eyeattack = true;
        anim.SetBool("IsArousal", true);
        ArousalScreenEffect.SetBool("ArousalEffect", true);
        thunderEffect.TriggerThunderOn();
        attackSpeed = 2f;
        
        yield return new WaitForSeconds(6f);
        eyeattack = false;
        
        anim.SetBool("IsArousal",false);
        thunderEffect.TriggerThunderOff();
        attackSpeed = 8f;
    }

    public float GetAtkSpeed()
    {
        return attackSpeed;
    }
    public Transform GetTransform()
    {
        return transform;
    }
    public void DieLeg(FixedLeg fixedLeg)
    {
        if(fLeg1==fixedLeg)
        {
            fLeg1 =null;
            fixedLeg1=null;
        }
        else
        {
            fLeg2 =null;
            fixedLeg2=null;
        }
    }
}
