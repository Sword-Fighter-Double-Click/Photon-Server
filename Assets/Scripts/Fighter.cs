using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Fighter : MonoBehaviour
{
    public enum FighterAction
    {
        None,
        Hit,
        Jump,
        Guard,
        Attack,
        ChargedAttack,
        JumpAttack,
        Ultimate
    };

    public enum FighterPosition
    {
        Left = -1,
        Right = 1
    }

    [Header("Caching")]
    [SerializeField] private Image HPBar;
    [SerializeField] private Image FPBar;
    [SerializeField] private GameObject ultimateScreen;
    [SerializeField] private AnimationClip ultimate;

    // 기존 에셋에 있던 먼지 효과 프리팹 오브젝트
    //[SerializeField] private GameObject m_RunStopDust;
    //[SerializeField] private GameObject m_JumpDust;
    //[SerializeField] private GameObject m_LandingDust;

    [Header("Action")]
    // Attack, None, Guard와 같이 플레이어의 상태를 지정하는 변수
    public FighterAction fighterAction;

    [Header("Value")]
    // 0번은 왼쪽 플레이어, 1번은 오른쪽 플레이어로 지정됨
    protected int fighterNumber;
    protected FighterPosition fighterPosition;
    /// <summary>
    /// 키 입력 여부를 정하는 변수
    /// </summary>
    [SerializeField] private bool canInput = true;
    public bool isDead = false;
    // 상대 캐릭터 클래스 저장 변수
    private Fighter enemyFighter;

    // 최대 속도, 점프 높이, 카운터 데미지 배율, 공격 데미지, 가드 시 데미지 감소율 등등 여러 스테이터스 값을 저장하는 변수들
    [Header("Stats")]
    [SerializeField] private float currentHP;
    public float currentUltimateGage;
    [SerializeField] private float currentSpeed;
    [SerializeField] protected FighterStatus status;
    [SerializeField] protected FighterSkill[] skills = new FighterSkill[5];

    protected Animator animator;
    protected Rigidbody rigidBody;
    protected SpriteRenderer spriteRenderer;
    private Sensor groundSensor;
    protected AudioSource audioSource;

    private int attackCount = 1;
    private bool canNextAttack;
    private int run;
    private int backDash;
    private float backDashDelay = 0.75f;
    private float countBackDashDelay;
    private float walkPressTime = 0.2f;
    private float countWalkPressTime;
    //private FighterAudio fighterAudio;

    /// <summary>
    /// 그라운드 체크 변수
    /// </summary>
    protected bool isGround = false;
    /// <summary>
    /// 이동 방향 변수 -1은 좌, 1은 우
    /// </summary>
    private int facingDirection = 0;

    // 키 입력할 수 없는 시간을 정하는 변수
    protected float cantInputTime = 0;

    /// <summary>
    /// 적이 궁극기에 맞았는지를 저장하는 변수
    /// </summary>
    protected bool hitUltimate;

    private GameObject ultimateScreenClone;
    private PhysicMaterial physicMaterial;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //fighterAudio = transform.Find("Audio").GetComponent<FighterAudio>();
        groundSensor = GetComponentInChildren<Sensor>();
        audioSource = Camera.main.GetComponent<AudioSource>();
        physicMaterial = GetComponentInChildren<CapsuleCollider>().material;

        currentSpeed = status.speed;
    }

    // 자식 클래스에서도 쓸 수 있도록 추상화
    protected virtual void Update()
    {
        SetPositionWithEnemyFighter();

        HandleCantInputTime(Time.deltaTime);

        Death();

        OnGround();

        SetAirspeed();

        SetVelocityX();

        HandleUI();

        Jump();

        Turn();

        Run();

        BackDash();

        Guard();

        Attack();

        ChargedAttack();

        JumpAttack();

        Ultimate();

        // 켜져 있는 Collider의 크기를 이용하여 공격판정을 생성하고 적이 맞았는지 확인하는 반복문
        foreach (FighterSkill skill in skills)
        {
            if (!skill.colliderEnabled) continue;

            if (SearchFighterWithinRange(skill.collider))
            {
                // 적이 맞았다면

                if (fighterAction == FighterAction.Ultimate)
                {
                    hitUltimate = true;
                }

                skill.colliderEnabled = false;

                // 데미지를 가함
                GiveDamage(skill);
            }
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    #region HandleValue

    /// <summary>
    /// 현재 fighterNumber에 따라 UI 캐싱
    /// </summary>
    public void SettingUI()
    {
        Transform UI;
        foreach (string word in new string[] { "1", "2" })
        {
            string temp = "Player" + word;
            if (CompareTag(temp))
            {
                UI = GameObject.FindGameObjectWithTag(temp + "UI").transform;
                HPBar = UI.Find("HPBar").GetComponent<Image>();
                FPBar = UI.Find("Ultimate").Find("Empty").GetComponent<Image>();
            }
        }
    }

    /// <summary>
    /// fighterNumber에 따라 태그 설정 및 스테이터스, 애니메이션, 공격판정 초기화
    /// </summary>
    public void ResetState()
    {
        // 애니메이션 및 스테이터스 초기화
        animator.SetInteger("FacingDirection", 0);
        currentHP = status.HP;
        currentUltimateGage = 0;
        isDead = false;
        SetAction(FighterAction.None.ToString());
        animator.SetTrigger("Reset");

        if (CompareTag("Player1"))
        {
            fighterNumber = 0;
            fighterPosition = FighterPosition.Left;
        }
        else if (CompareTag("Player2"))
        {
            fighterNumber = 1;
            fighterPosition = FighterPosition.Right;
        }

        // 공격판정 초기화
        foreach (FighterSkill skill in skills)
        {
            skill.colliderEnabled = false;
        }

        // rigidbody 초기화
        rigidBody.velocity = Vector3.zero;
    }

    public void SetEnemyFighter(Fighter fighter)
    {
        enemyFighter = fighter;
    }

    private void SetPositionWithEnemyFighter()
    {
        fighterPosition = enemyFighter.transform.position.x > transform.position.x ? FighterPosition.Left : FighterPosition.Right;
    }

    /// <summary>
    /// 그라운드 체크
    /// </summary>
    private void OnGround()
    {
        if (!isGround && groundSensor.State())
        {
            isGround = true;
            fighterAction = FighterAction.None;
            animator.SetBool("Grounded", isGround);
            animator.SetBool("Jump", false);
            //physicMaterial.dynamicFriction = 1;
        }
    }

    /// <summary>
    /// AirSpeed 값 할당
    /// </summary>
    private void SetAirspeed()
    {
        // airSpeedY가 0 이하가 되면 낙하 애니메이션 출력
        animator.SetFloat("AirSpeedY", rigidBody.velocity.y);
    }

    private void SetVelocityX()
    {
        animator.SetFloat("VelocityX", rigidBody.velocity.x);
    }

    /// <summary>
    /// 플레이어의 상태를 정수로 설정
    /// </summary>
    /// <param name="value"></param>
    //void SetAction(FighterAction value)
    //{
    //    fighterAction = value;
    //}

    void SetAction(string value)
    {
        fighterAction = (FighterAction)Enum.Parse(typeof(FighterAction), value);
    }

    /// <summary>
    /// 입력 불가 시간 계산
    /// </summary>
    /// <param name="deltaTime"></param>
    private void HandleCantInputTime(float deltaTime)
    {
        // 입력 불가 시간이 남아있으면 키 입력 불가
        if (cantInputTime > 0)
        {
            cantInputTime -= deltaTime;

            canInput = false;
        }
        else
        {
            canInput = true;
        }
    }

    /// <summary>
    /// 입력 불가 시간 설정
    /// </summary>
    /// <param name="value"></param>
    void SetCantInputTime(float value)
    {
        cantInputTime = value;
    }

    void OnCanNextAttack()
    {
        canNextAttack = true;
    }

    void OffCanNextAttack()
    {
        canNextAttack = false;
    }

    /// <summary>
    /// 입력이 가능하게 설정
    /// </summary>
    public void OnInput()
    {
        cantInputTime = 0;
    }

    /// <summary>
    /// 입력이 불가능하게 설정
    /// </summary>
    public void OffInput()
    {
        cantInputTime = float.MaxValue;
    }
    #endregion

    #region HandleHitBox

    /// <summary>
    /// 인자 내 문자열에 해당하는 공격 판정 collider 활성화 설정
    /// </summary>
    /// <param name="value"></param>
    void OnHitBox(string value)
    {
        for (int count = 0; count < skills.Length; count++)
        {
            if (skills[count].name.Equals(value))
            {
                skills[count].colliderEnabled = true;
            }
        }
    }

    void OffHitBox(string value)
    {
        for (int count = 0; count < skills.Length; count++)
        {
            if (skills[count].name.Equals(value))
            {
                skills[count].colliderEnabled = false;
            }
        }
    }
    #endregion

    #region HandleUI
    /// <summary>
    /// 체력 및 스킬 게이지 UI에 표현
    /// </summary>
    private void HandleUI()
    {
        HPBar.fillAmount = currentHP / status.HP;
        FPBar.fillAmount = currentUltimateGage / status.ultimateGage;
    }
    #endregion

    #region Action
    /// <summary>
    /// 이동 및 플레이어 방향 조정
    /// </summary>
    private void Move()
    {
        if (!canInput) return;
        if (rigidBody.velocity.x > 0.1f) return;
        // IDLE과 점프일 때만 이동 함수 진입
        if (!(fighterAction == FighterAction.None || fighterAction == FighterAction.Jump)) return;

        // 키 입력 여부 저장
        bool inputRight = Input.GetKey(KeySetting.keys[fighterNumber, 3]);
        bool inputLeft = Input.GetKey(KeySetting.keys[fighterNumber, 1]);

        // 방향 설정 및 저장
        int direction = (inputLeft ? -1 : 0) + (inputRight ? 1 : 0);

        facingDirection = direction;

        // 이동 애니메이션 출력
        animator.SetInteger("FacingDirection", facingDirection);

        // 이동
        transform.position += new Vector3(facingDirection * currentSpeed,0,0)*Time.deltaTime;
        //rigidBody.velocity = new Vector3(facingDirection * currentSpeed, rigidBody.velocity.y);
    }

    private void Turn()
    {
        // 이동 방향에 따라 이미지 방향 설정
        transform.eulerAngles = fighterPosition == FighterPosition.Left ? Vector3.zero : 180 * Vector3.up;
    }

    private void Run()
    {
        if (run == 1 && ((Time.time - countWalkPressTime) > walkPressTime))
        {
            run = 0;
        }

        int keyNumber = fighterPosition == FighterPosition.Left ? 3 : 1;

        if ((run == 2 && Input.GetKeyUp(KeySetting.keys[fighterNumber, keyNumber])) || (run == 2 && !isGround))
        {
            //print("Dash");
            run = 0;
            currentSpeed *= 10 / 15f;
            //rigidBody.AddForce(50 * (int)fighterPosition * Vector3.left, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, keyNumber]))
        {
            if (run == 0)
            {
                countWalkPressTime = Time.time;
                run = 1;
            }

            else if (run == 1 && ((Time.time - countWalkPressTime) < walkPressTime))
            {
                run = 2;
                currentSpeed *= 1.5f;
            }
        }
    }

    private void BackDash()
    {
        if (!canInput) return;
        if (!isGround) return;

        if (countBackDashDelay > 0)
        {
            countBackDashDelay -= Time.deltaTime;
            return;
        }

        if (backDash == 1 && ((Time.time - countWalkPressTime) > walkPressTime))
        {
            backDash = 0;
        }

        int keyNumber = fighterPosition == FighterPosition.Left ? 1 : 3;

        if (backDash == 2 && Input.GetKeyUp(KeySetting.keys[fighterNumber, keyNumber]))
        {
            //print("Dash");
            backDash = 0;
            //rigidBody.AddForce(50 * (int)fighterPosition * Vector3.left, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, keyNumber]))
        {
            if (backDash == 0)
            {
                countWalkPressTime = Time.time;
                backDash = 1;
            }

            else if (backDash == 1 && ((Time.time - countWalkPressTime) < walkPressTime))
            {
                backDash = 0;
                rigidBody.AddForce(10 * (int)fighterPosition * Vector3.right, ForceMode.Impulse);
                animator.CrossFade("BackDash", 0f);
                countBackDashDelay = backDashDelay;
            }
        }
    }

    /// <summary>
    /// 점프
    /// </summary>
    private void Jump()
    {
        if (!canInput) return;
        if (!isGround) return;
        // IDLE 상태에만 함수 진입
        if (fighterAction != FighterAction.None) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 0]))
        {
            // 벽끼임 방지를 위해 마찰력 0으로 설정
            //rigidBody.sharedMaterial.friction = 0;
            isGround = false;
            fighterAction = FighterAction.Jump;
            animator.SetBool("Grounded", isGround);
            // 애니메이션 출력
            animator.SetBool("Jump", true);
            // 점프
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, status.jumpForce);
            groundSensor.Disable(0.2f);

            //physicMaterial.dynamicFriction = 0;
        }
    }

    /// <summary>
    /// 가드
    /// </summary>
    private void Guard()
    {
        if (!canInput) return;
        if (!isGround) return;
        // IDLE 및 가드 상태에만 함수 진입
        if (!(fighterAction == FighterAction.None || fighterAction == FighterAction.Guard)) return;

        // 키를 누르고 있으면 가드 활성화, 떼면 가드 비활성화
        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 2]))
        {
            fighterAction = FighterAction.Guard;
            animator.CrossFade("Guard", 0f);
        }
        else if (Input.GetKeyUp(KeySetting.keys[fighterNumber, 2]))
        {
            fighterAction = FighterAction.None;
            animator.SetTrigger("UnGuard");
        }
    }

    /// <summary>
    /// 일반공격
    /// </summary>
    private void Attack()
    {
        if (!canInput) return;
        if (!isGround) return;

        if (!Input.GetKeyDown(KeySetting.keys[fighterNumber, 4])) return;

        // IDLE 상태에만 함수 진입
        if (fighterAction == FighterAction.None)
        {
            fighterAction = FighterAction.Attack;
            animator.CrossFade("Attack1", 0);
        }
        else if (fighterAction == FighterAction.Attack)
        {
            if (canNextAttack)
            {
                animator.CrossFade("Attack2", 0);
                canNextAttack = false;
            }
        }
    }

    /// <summary>
    /// 강공격
    /// </summary>
    private void ChargedAttack()
    {
        if (!canInput) return;
        if (!isGround) return;
        // IDLE 상태에만 함수 진입
        if (fighterAction != FighterAction.None) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 5]))
        {
            fighterAction = FighterAction.ChargedAttack;

            animator.CrossFade("ChargedAttack", 0);
        }
    }

    /// <summary>
    /// 점프공격
    /// </summary>
    private void JumpAttack()
    {
        if (!canInput) return;
        if (isGround) return;
        // 점프 상태에서만 함수 진입
        if (fighterAction != FighterAction.Jump) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
        {
            fighterAction = FighterAction.JumpAttack;

            animator.CrossFade("JumpAttack", 0);
        }
    }

    /// <summary>
    /// 궁극기
    /// </summary>
    private void Ultimate()
    {
        if (!canInput) return;
        if (!isGround) return;
        // IDLE 상태에서만 함수 진입
        if (fighterAction != FighterAction.None) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 6]))
        {
            if (currentUltimateGage < status.ultimateGage) return;

            currentUltimateGage = 0;

            fighterAction = FighterAction.Ultimate;

            animator.CrossFade("Ultimate", 0);
        }
    }

    /// <summary>
    /// 피격 
    /// </summary>
    /// <param name="isGuard"></param>
    /// <param name="facingDirection"></param>
    /// <param name="ultimateCantInputTime"></param>
    private void Hit(float damage, bool isGuard, float facingDirection, float ultimateCantInputTime)
    {
        currentHP -= damage;

        Vector2 knockBackPath = facingDirection * Vector2.right;

        // 가드 시 입력 불가와 넉백이 시간이 가드를 안했을 때보다 줄어듭니다.
        // 궁극기는 시전시간이 끝날 때까지 고정적으로 입력을 못하게 만듭니다.
        if (isGuard)
        {
            // 입력 불가 시간 설정
            cantInputTime = ultimateCantInputTime > 0 ? ultimateCantInputTime : 0.1f;
            // 넉백
            rigidBody.AddForce(knockBackPath * status.guardKnockBackPower, ForceMode.Impulse);
        }
        else
        {
            // 현재 공격 중단
            OffHitBox(fighterAction.ToString());
            animator.CrossFade("Hit", 0);
            SetAction(FighterAction.Hit.ToString());
            // 입력 불가 시간 설정
            cantInputTime = ultimateCantInputTime > 0 ? ultimateCantInputTime : 0.3f;
            // 넉백
            rigidBody.AddForce(knockBackPath * status.hitKnockBackPower, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// 사망
    /// </summary>
    private void Death()
    {
        // HP가 0이 되면 애니메이션을 출력
        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
            print("!!!");
            animator.CrossFade("Death", 0);
            OffInput();
        }
    }
    #endregion

    #region Action Effect
    // 에셋 먼지 효과 함수
    //private void SpawnDustEffect(GameObject dust, float dustXOffset = 0)
    //{
    //    if (dust != null)
    //    {
    //        Vector3 dustSpawnPosition = transform.position + new Vector3(dustXOffset * facingDirection, 0f, 0f);
    //        GameObject newDust = Instantiate(dust, dustSpawnPosition, Quaternion.identity);
    //        newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(facingDirection, 1, 1);
    //    }
    //}

    //void AE_runStop()
    //{
    //	fighterAudio.PlaySound("RunStop");
    //	float dustXOffset = 0.6f;
    //	SpawnDustEffect(m_RunStopDust, dustXOffset);
    //}

    //void AE_footstep()
    //{
    //	fighterAudio.PlaySound("Footstep");
    //}

    //void AE_Jump()
    //{
    //	fighterAudio.PlaySound("Jump");
    //	SpawnDustEffect(m_JumpDust);
    //}

    //void AE_Landing()
    //{
    //	fighterAudio.PlaySound("Landing");
    //	SpawnDustEffect(m_LandingDust);
    //}

    /// <summary>
    /// 궁극기 이미지 활성화
    /// </summary>
    protected void OnUltimateScreen()
    {
        //ultimateScreenClone = Instantiate(ultimateScreen);

        //ultimateScreenClone.SetActive(true);
    }

    /// <summary>
    /// 궁극기 이미지 비활성화
    /// </summary>
    protected void OffUltimateScreen()
    {
        //Destroy(ultimateScreenClone);
    }
    #endregion

    #region HandleHitDetection
    /// <summary>
    /// 공격판정 생성 및 공격판정에 인식된 적 플레이어 데이터 반환
    /// </summary>
    /// <param name="searchRange"></param>
    /// <returns></returns>
    private bool SearchFighterWithinRange(BoxCollider searchRange)
    {
        int count = Physics.OverlapBoxNonAlloc(searchRange.bounds.center, searchRange.bounds.size / 2, new Collider[2], Quaternion.identity, LayerMask.GetMask("Player"));
        return count > 1;
    }

    /// <summary>
    /// 적 플레이어에게 데미지를 가하는 함수
    /// </summary>
    /// <param name="fighterSkill"></param>
    private void GiveDamage(FighterSkill fighterSkill)
    {
        bool isGuard = false;

        float damage = 0;

        // 피격 당해 입력 불가 시간이 있다면 아래 코드를 실행하지 않음
        if (cantInputTime > 0) return;

        // 현재 공격의 데미지 저장
        damage += fighterSkill.damage;

        // 가드 시 데미지 감소
        if (enemyFighter.fighterAction == FighterAction.Guard)
        {
            damage = fighterSkill.damage - fighterSkill.damage * fighterSkill.absorptionRate;

            isGuard = true;

            cantInputTime = 0.5f;

            SetAction(FighterAction.None.ToString());
        }
        // 적 플레이어가 공격 애니메이션일 시, 카운데 배율을 곱하여 데미지 증가
        else if (!(enemyFighter.fighterAction == FighterAction.None || enemyFighter.fighterAction == FighterAction.Hit))
        {
            damage *= status.counterDamageRate;
        }

        // 현재 공격이 궁극기일 시, 시전시간+0.5초의 입력 불가 시간을 적에게 적용한다.
        float ultimateCantInputTime = 0;
        if (fighterAction == FighterAction.Ultimate)
        {
            ultimateCantInputTime = ultimate.length + 0.5f;
        }

        enemyFighter.Hit(damage, isGuard, fighterPosition == FighterPosition.Left ? 1 : -1, ultimateCantInputTime);

        currentUltimateGage = Mathf.Clamp(currentUltimateGage + 10, 0, 100);
        enemyFighter.currentUltimateGage = Mathf.Clamp(enemyFighter.currentUltimateGage + 5, 0, 100);
    }
    #endregion
}
