using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
//[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Fighter : MonoBehaviour
{
    public enum FighterAction
    {
        None,
        Hit,
        Jump,
        Dash,
        Guard,
        Attack,
        ChargedAttack,
        JumpAttack,
        AntiAirAttack,
        BackDashAttack,
        Ultimate
    };

    public enum FighterPosition
    {
        Left = -1,
        Right = 1
    }


    // ���� ���¿� �ִ� ���� ȿ�� ������ ������Ʈ
    //[SerializeField] private GameObject m_RunStopDust;
    //[SerializeField] private GameObject m_JumpDust;
    //[SerializeField] private GameObject m_LandingDust;

    [Header("Action")]
    // Attack, None, Guard�� ���� �÷��̾��� ���¸� �����ϴ� ����
    public FighterAction fighterAction;

    [Header("Value")]
    // 0���� ���� �÷��̾�, 1���� ������ �÷��̾�� ������
    protected int fighterNumber;
    protected FighterPosition fighterPosition;
    /// <summary>
    /// Ű �Է� ���θ� ���ϴ� ����
    /// </summary>
    [SerializeField] private bool canInput = true;
    public bool isDead = false;
    // ��� ĳ���� Ŭ���� ���� ����
    protected Fighter enemyFighter;

    // �ִ� �ӵ�, ���� ����, ī���� ������ ����, ���� ������, ���� �� ������ ������ ��� ���� �������ͽ� ���� �����ϴ� ������
    [Header("Stats")]
    [SerializeField] private float currentHP;
    public float currentUltimateGage;
    [SerializeField] private float currentSpeed;
    [SerializeField] protected FighterStatus status;
    [SerializeField] protected FighterSkill[] skills = new FighterSkill[5];

    [Header("Cashing")]
    [SerializeField] private Image HPBar;
    [SerializeField] private Image FPBar;
    [SerializeField] private GameObject ultimateScreen;
    [SerializeField] private float ultimateCantInputTime;

    protected Animator animator;
    protected CharacterController characterController;
    protected SpriteRenderer spriteRenderer;
    protected AudioSource audioSource;

    protected bool counterAttack;
    private float attackDelay = 0.4f;
    private float countAttackDelay;
    private bool canNextAttack;
    private int run;
    private int backDash;
    private int backDashDirection;
    private float backDashMinTime = 0.45f;
    private float countBackDashMinTime;
    private float backDashDelay = 0.75f;
    private float countBackDashDelay;
    private float walkPressTime = 0.35f;
    private float countWalkPressTime;
    //private FighterAudio fighterAudio;

    private Vector3 moveDirection;
    public Vector3 velocity;

    /// <summary>
    /// �׶��� üũ ����
    /// </summary>
    protected bool isGround = false;
    /// <summary>
    /// �̵� ���� ���� -1�� ��, 1�� ��
    /// </summary>
    private int facingDirection = 0;

    // Ű �Է��� �� ���� �ð��� ���ϴ� ����
    protected float cantInputTime = 0;

    /// <summary>
    /// ���� �ñر⿡ �¾Ҵ����� �����ϴ� ����
    /// </summary>
    protected bool hitUltimate;

    private GameObject ultimateScreenClone;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //fighterAudio = transform.Find("Audio").GetComponent<FighterAudio>();
        audioSource = Camera.main.GetComponent<AudioSource>();

        currentSpeed = status.speed;
    }

    // �ڽ� Ŭ���������� �� �� �ֵ��� �߻�ȭ
    protected virtual void Update()
    {
        SetPositionWithEnemyFighter();

        HandleCantInputTime(Time.deltaTime);

        Death();

        SetAirspeed();

        SetVelocityX();

        HandleUI();

        Turn();

        Move();

        Jump();

        Gravity();

        Run();

        BackDash();

        Guard();

        Attack();

        StrongAttack();

        JumpAttack();

        AntiAirAttack();

        BackDashAttack();

        Ultimate();

        // ���� �ִ� Collider�� ũ�⸦ �̿��Ͽ� ���������� �����ϰ� ���� �¾Ҵ��� Ȯ���ϴ� �ݺ���
        foreach (FighterSkill skill in skills)
        {
            if (!skill.colliderEnabled) continue;

            if (SearchFighterWithinRange(skill.collider))
            {
                // ���� �¾Ҵٸ�

                if (fighterAction == FighterAction.Ultimate)
                {
                    hitUltimate = true;
                }

                skill.colliderEnabled = false;

                // �������� ����
                GiveDamage(skill);
            }
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    #region HandleValue

    /// <summary>
    /// ���� fighterNumber�� ���� UI ĳ��
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
                FPBar = UI.Find("Ultimate").Find("Enable").GetComponent<Image>();
            }
        }
    }

    /// <summary>
    /// fighterNumber�� ���� �±� ���� �� �������ͽ�, �ִϸ��̼�, �������� �ʱ�ȭ
    /// </summary>
    public void ResetState()
    {
        // �ִϸ��̼� �� �������ͽ� �ʱ�ȭ
        animator.SetInteger("FacingDirection", 0);
        currentHP = status.HP;
        currentUltimateGage = 0;
        isDead = false;
        SetAction(FighterAction.None.ToString());
        animator.CrossFade("Reset", 0);

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

        // �������� �ʱ�ȭ
        foreach (FighterSkill skill in skills)
        {
            skill.colliderEnabled = false;
        }

        // rigidbody �ʱ�ȭ
        velocity = Vector3.zero;
        moveDirection = Vector3.zero;
    }

    public void SetEnemyFighter(Fighter fighter)
    {
        enemyFighter = fighter;
    }

    private void SetPositionWithEnemyFighter()
    {
        print(enemyFighter.transform.position.x);
        print(transform.position.x);
        fighterPosition = enemyFighter.transform.position.x > transform.position.x ? FighterPosition.Left : FighterPosition.Right;
    }

    /// <summary>
    /// AirSpeed �� �Ҵ�
    /// </summary>
    private void SetAirspeed()
    {
        // airSpeedY�� 0 ���ϰ� �Ǹ� ���� �ִϸ��̼� ���
        animator.SetFloat("AirSpeedY", velocity.y);
    }

    private void SetVelocityX()
    {
        animator.SetFloat("VelocityX", Mathf.Abs(velocity.x));
    }

    /// <summary>
    /// �÷��̾��� ���¸� ������ ����
    /// </summary>
    /// <param name="value"></param>
    void SetAction(string value)
    {
        fighterAction = (FighterAction)Enum.Parse(typeof(FighterAction), value);
    }

    /// <summary>
    /// �Է� �Ұ� �ð� ���
    /// </summary>
    /// <param name="deltaTime"></param>
    private void HandleCantInputTime(float deltaTime)
    {
        // �Է� �Ұ� �ð��� ���������� Ű �Է� �Ұ�
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

    void SetAttackDelay(float value)
    {
        countAttackDelay = value;
    }

    /// <summary>
    /// �Է� �Ұ� �ð� ����
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
    /// �Է��� �����ϰ� ����
    /// </summary>
    public void OnInput()
    {
        cantInputTime = 0;
    }

    /// <summary>
    /// �Է��� �Ұ����ϰ� ����
    /// </summary>
    public void OffInput()
    {
        cantInputTime = float.MaxValue;
    }
    #endregion

    #region HandleHitBox

    /// <summary>
    /// ���� �� ���ڿ��� �ش��ϴ� ���� ���� collider Ȱ��ȭ ����
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
    /// ü�� �� ��ų ������ UI�� ǥ��
    /// </summary>
    private void HandleUI()
    {
        HPBar.fillAmount = currentHP / status.HP;
        FPBar.fillAmount = currentUltimateGage / status.ultimateGage;
    }
    #endregion

    #region Action

    private void Gravity()
    {
        isGround = characterController.isGrounded && velocity.y < 0;

        if (isGround && velocity.y != -1)
        {
            velocity.y = -1f;
            fighterAction = FighterAction.None;
            animator.SetBool("Grounded", true);
            animator.SetBool("Jump", false);
        }
        else if (!isGround)
        {
            velocity.y += Physics.gravity.y * 3 * Time.deltaTime;
            animator.SetBool("Grounded", false);
        }

        moveDirection.y = velocity.y;
        moveDirection.x += velocity.x;
        if (velocity.x > 0)
        {
            velocity.x = Mathf.Clamp(velocity.x - 30 * Time.deltaTime, 0, float.MaxValue);
        }
        else if (velocity.x < 0)
        {
            velocity.x = Mathf.Clamp(velocity.x + 30 * Time.deltaTime, float.MinValue, 0);
        }
    }

    /// <summary>
    /// �̵� �� �÷��̾� ���� ����
    /// </summary>
    private void Move()
    {
        if (!canInput) return;
        if (Mathf.Abs(velocity.x) > 0.1f) return;

        // IDLE�� ������ ���� �̵� �Լ� ����
        if (!(fighterAction == FighterAction.None || fighterAction == FighterAction.Jump))
        {
            moveDirection = Vector3.zero;
            return;
        }

        // Ű �Է� ���� ����
        bool inputRight = Input.GetKey(KeySetting.keys[fighterNumber, 3]);
        bool inputLeft = Input.GetKey(KeySetting.keys[fighterNumber, 1]);

        // ���� ���� �� ����
        int direction = (inputLeft ? -1 : 0) + (inputRight ? 1 : 0);

        facingDirection = direction;

        // �̵� �ִϸ��̼� ���
        animator.SetInteger("FacingDirection", facingDirection * -(int)fighterPosition);

        // �̵�
        moveDirection = currentSpeed * facingDirection * Vector3.right;
    }

    private void Turn()
    {
        // �̵� ���⿡ ���� �̹��� ���� ����
        transform.eulerAngles = fighterPosition == FighterPosition.Left ? Vector3.zero : 180 * Vector3.up;
    }

    private void Run()
    {
        if (facingDirection != 0 && -(int)fighterPosition != facingDirection)
        {
            run = 0;
            currentSpeed = status.speed;
            animator.SetBool("Run", false);
        }

        if (run == 1 && ((Time.time - countWalkPressTime) > walkPressTime))
        {
            run = 0;
        }

        int keyNumber = fighterPosition == FighterPosition.Left ? 3 : 1;

        if (run == 2 && Input.GetKeyUp(KeySetting.keys[fighterNumber, keyNumber]))
        {
            run = 0;
            currentSpeed = status.speed;
            animator.SetBool("Run", false);
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

                animator.SetBool("Run", true);
            }
        }
    }

    private void BackDash()
    {
        if (countBackDashMinTime > 0)
        {
            countBackDashMinTime -= Time.deltaTime;
            animator.SetFloat("BackDashTime", countBackDashMinTime);
        }

        if (facingDirection != 0 && (int)fighterPosition != facingDirection)
        {
            backDash = 0;
        }

        if (!canInput) return;
        if (!isGround) return;

        if (countBackDashDelay > 0)
        {
            countBackDashDelay -= Time.deltaTime;
            return;
        }

        if (backDash > 0 && ((Time.time - countWalkPressTime) > walkPressTime))
        {
            backDash = 0;
        }

        int keyNumber = fighterPosition == FighterPosition.Left ? 1 : 3;

        if (backDash == 2 && Input.GetKeyUp(KeySetting.keys[fighterNumber, keyNumber]))
        {
            backDash = 0;
            fighterAction = FighterAction.Dash;
            velocity.x += (int)fighterPosition * 5;
            animator.CrossFade("BackDash", 0f);
            countBackDashDelay = backDashDelay;
            countBackDashMinTime = backDashMinTime;
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
                backDash = 2;
            }
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    private void Jump()
    {
        if (!canInput) return;
        if (!isGround) return;
        // IDLE ���¿��� �Լ� ����
        if (fighterAction != FighterAction.None) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 0]))
        {
            fighterAction = FighterAction.Jump;
            animator.SetBool("Grounded", false);
            // �ִϸ��̼� ���
            animator.SetBool("Jump", true);
            // ����
            velocity.y += 30;
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    private void Guard()
    {
        if (!canInput) return;
        if (!isGround) return;

        // IDLE �� ���� ���¿��� �Լ� ����
        if (!(fighterAction == FighterAction.None || fighterAction == FighterAction.Guard)) return;

        // Ű�� ������ ������ ���� Ȱ��ȭ, ���� ���� ��Ȱ��ȭ
        if (Input.GetKey(KeySetting.keys[fighterNumber, 2]))
        {
            fighterAction = FighterAction.Guard;
            animator.CrossFade("Guard", 0f);
        }
        else
        {
            fighterAction = FighterAction.None;
        }

        animator.SetBool("Guard", fighterAction == FighterAction.Guard);
    }

    /// <summary>
    /// �Ϲݰ���
    /// </summary>
    private void Attack()
    {
        if (countAttackDelay > 0)
        {
            countAttackDelay -= Time.deltaTime;
            return;
        }

        if (!isGround) return;
        if (!canInput) return;

        if (!Input.GetKeyDown(KeySetting.keys[fighterNumber, 4])) return;

        // IDLE ���¿��� �Լ� ����
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
                countAttackDelay = attackDelay;
            }
        }
    }

    /// <summary>
    /// ������
    /// </summary>
    private void StrongAttack()
    {
        if (!canInput) return;
        if (!isGround) return;
        // IDLE ���¿��� �Լ� ����
        if (fighterAction != FighterAction.None) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 5]))
        {
            fighterAction = FighterAction.ChargedAttack;

            animator.CrossFade("StrongAttack", 0);
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    private void JumpAttack()
    {
        if (!canInput) return;
        if (isGround) return;
        // ���� ���¿����� �Լ� ����
        if (fighterAction != FighterAction.Jump) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
        {
            fighterAction = FighterAction.JumpAttack;

            animator.CrossFade("JumpAttack", 0);
        }
    }

    private void AntiAirAttack()
    {
        if (!canInput) return;
        if (!isGround) return;

        if (fighterAction != FighterAction.None) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 6]))
        {
            fighterAction = FighterAction.AntiAirAttack;
            animator.CrossFade("AntiAirAttack", 0);
        }
    }

    private void BackDashAttack()
    {
        if (!canInput) return;
        if (!isGround) return;

        if (fighterAction != FighterAction.Dash) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
        {
            fighterAction = FighterAction.BackDashAttack;
            animator.CrossFade("BackDashAttack", 0);
        }
    }

    /// <summary>
    /// �ñر�
    /// </summary>
    private void Ultimate()
    {
        if (!canInput) return;
        if (!isGround) return;
        // IDLE ���¿����� �Լ� ����
        if (fighterAction != FighterAction.None) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 7]))
        {
            if (currentUltimateGage < status.ultimateGage) return;

            currentUltimateGage = 0;

            fighterAction = FighterAction.Ultimate;

            animator.CrossFade("Ultimate", 0);
        }
    }

    /// <summary>
    /// �ǰ� 
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="isGuard"></param>
    /// <param name="facingDirection"></param>
    /// <param name="ultimateCantInputTime"></param>
    private void Hit(float damage, bool isGuard, float facingDirection, float ultimateCantInputTime)
    {
        currentHP -= damage;

        Vector2 knockBackPath = facingDirection * Vector2.right;

        // ���� �� �Է� �Ұ��� �˹��� �ð��� ���带 ������ ������ �پ��ϴ�.
        // �ñر�� �����ð��� ���� ������ ���������� �Է��� ���ϰ� ����ϴ�.
        if (isGuard)
        {
            // �Է� �Ұ� �ð� ����
            cantInputTime = ultimateCantInputTime > 0 ? ultimateCantInputTime : 0.1f;
        }
        else
        {
            // ���� ���� �ߴ�
            OffHitBox(fighterAction.ToString());
            animator.CrossFade("Hit", 0);
            SetAction(FighterAction.Hit.ToString());
            // �Է� �Ұ� �ð� ����
            cantInputTime = ultimateCantInputTime > 0 ? ultimateCantInputTime : 0.3f;
        }

        // �˹�
        if (ultimateCantInputTime <= 0)
        {
            velocity.x += facingDirection * 0.2f;
        }
    }

    /// <summary>
    /// ���
    /// </summary>
    private void Death()
    {
        // HP�� 0�� �Ǹ� �ִϸ��̼��� ���
        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
            animator.CrossFade("Death", 0);
            OffInput();
        }
    }
    #endregion

    #region Action Effect
    // ���� ���� ȿ�� �Լ�
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
    /// �ñر� �̹��� Ȱ��ȭ
    /// </summary>
    protected void OnUltimateScreen()
    {
        //ultimateScreenClone = Instantiate(ultimateScreen);

        //ultimateScreenClone.SetActive(true);
    }

    /// <summary>
    /// �ñر� �̹��� ��Ȱ��ȭ
    /// </summary>
    protected void OffUltimateScreen()
    {
        //Destroy(ultimateScreenClone);
    }
    #endregion

    #region HandleHitDetection
    /// <summary>
    /// �������� ���� �� ���������� �νĵ� �� �÷��̾� ������ ��ȯ
    /// </summary>
    /// <param name="searchRange"></param>
    /// <returns></returns>
    private bool SearchFighterWithinRange(BoxCollider searchRange)
    {
        Collider[] colliders = new Collider[2];
        int count = Physics.OverlapBoxNonAlloc(searchRange.bounds.center, searchRange.bounds.size / 2, colliders, Quaternion.identity, LayerMask.GetMask("Player"));
        foreach (Collider collider in colliders)
        {
            if (collider == null) continue;
            if (collider.CompareTag(tag)) count--;
        }
        return count > 0;
    }

    /// <summary>
    /// �� �÷��̾�� �������� ���ϴ� �Լ�
    /// </summary>
    /// <param name="fighterSkill"></param>
    private void GiveDamage(FighterSkill fighterSkill)
    {
        bool isGuard = false;

        float damage = 0;

        // �ǰ� ���� �Է� �Ұ� �ð��� �ִٸ� �Ʒ� �ڵ带 �������� ����
        if (cantInputTime > 0) return;

        // ���� ������ ������ ����
        damage += fighterSkill.damage;

        // ���� �� ������ ����
        if (enemyFighter.fighterAction == FighterAction.Guard)
        {
            damage = fighterSkill.damage - fighterSkill.damage * fighterSkill.absorptionRate;

            isGuard = true;

            cantInputTime = 0.5f;

            SetAction(FighterAction.None.ToString());
        }
        // �� �÷��̾ ���� �ִϸ��̼��� ��, ī� ������ ���Ͽ� ������ ����
        else if (!(enemyFighter.fighterAction == FighterAction.None || enemyFighter.fighterAction == FighterAction.Hit))
        {
            damage *= status.counterDamageRate;
            counterAttack = true;
        }

        // ���� ������ �ñر��� ��, �����ð�+0.5���� �Է� �Ұ� �ð��� ������ �����Ѵ�.
        float ucit = 0;
        if (fighterAction == FighterAction.Ultimate)
        {
            ucit = ultimateCantInputTime;
        }

        enemyFighter.Hit(damage, isGuard, fighterPosition == FighterPosition.Left ? 1 : -1, ucit);

        currentUltimateGage = Mathf.Clamp(currentUltimateGage + 10, 0, status.ultimateGage);
        enemyFighter.currentUltimateGage = Mathf.Clamp(enemyFighter.currentUltimateGage + 5, 0, status.ultimateGage);
    }
    #endregion
}
