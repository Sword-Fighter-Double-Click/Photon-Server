using System;
using UnityEngine;
using UnityEngine.UI;
using MyConstants;

public abstract class Fighter : MonoBehaviour
{
	[Header("Caching")]
	[SerializeField] private Image HPBar;
	[SerializeField] private Image FPBar;
	[SerializeField] private GameObject lethalMoveScreen;
	[SerializeField] private AnimationClip lethalMove;
	[SerializeField] private BoxCollider2D[] motionColliders = new BoxCollider2D[4];

	[SerializeField] private GameObject m_RunStopDust;
	[SerializeField] private GameObject m_JumpDust;
	[SerializeField] private GameObject m_LandingDust;

	[Header("Action")]
	public FighterAction fighterAction;

	[Header("Value")]
	[Range(0, 1)] public int fighterNumber = 0;
	public bool canInput = true;
	public Fighter enemyFighter;

	[Header("Stats")]
	public float HP = 100;
	public float FP = 0;
	[SerializeField] protected float maxSpeed = 4.5f;
	[SerializeField] protected float jumpForce = 7.5f;
	[SerializeField] protected float counterDamageRate = 1.2f; // 얘들 나중에 클래스로 만들어서 객체화하기
	[SerializeField] protected float attackDamage = 6f;
	[SerializeField] protected float chargedAttackDamage = 12;
	[SerializeField] protected float jumpAttackDamage = 15;
	[SerializeField] protected float lethalMoveDamage = 35;
	[SerializeField] protected float attackAbsorptionRate = 0.95f;
	[SerializeField] protected float chargedAttackAbsorptionRate = 0.75f;
	[SerializeField] protected float jumpAttackAbsorptionRate = 0.7f;
	[SerializeField] protected float lethalMoveAbsorptionRate = 0.5f;
	[SerializeField] protected float chargedAttackFP = 3;
	[SerializeField] protected float jumpAttackCoolFP = 5;
	[SerializeField] protected float lethalMoveFP = 75;
	[SerializeField] protected float chargedAttackCoolDownTime = 1.2f;
	[SerializeField] protected float jumpAttackCoolDownTime = 1.5f;
	[SerializeField] protected float hitKnockBackPower = 10;
	[SerializeField] protected float guardKnockBackPower = 5;

	protected Animator animator;
	protected Rigidbody2D rigidbody2d;
	protected SpriteRenderer spriteRenderer;
	private Sensor groundSensor;
	private FighterAudio fighterAudio;

	protected bool isGround = false;
	private int facingDirection = 0;

	private float countChargedAttack;
	private float countJumpAttack;

	protected float cantInputTime = 0;

	protected bool hitLethalMove;

	private GameObject lethalMoveScreenClone;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		rigidbody2d = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		fighterAudio = transform.Find("Audio").GetComponent<FighterAudio>();
		groundSensor = transform.Find("GroundSensor").GetComponent<Sensor>();
	}

	protected virtual void Start()
	{
		SettingUI();
	}

	protected virtual void Update()
	{
		print(tag +" : "+ fighterAction);

		HandleCantInputTime(Time.deltaTime);

		Death();

		OnGround();

		IsFalling();

		SetAirspeed();

		CountCoolTime(Time.deltaTime);

		HandleUI();

		Jump();

		Movement();

		Guard();

		Attack();

		ChargedAttack();

		JumpAttack();

		LethalMove();

		foreach (BoxCollider2D collider in motionColliders)
		{
			if (!collider.enabled) continue;

			Fighter enemyFighter = SearchFighterWithinRange(collider);

			if (enemyFighter == null) continue;

			if (fighterAction == FighterAction.LethalMove)
			{
				hitLethalMove = true;
			}

			collider.enabled = false;

			GiveDamage(enemyFighter);
		}
	}

	#region HandleValue

	private void SettingUI()
	{
		if (fighterNumber == 0)
		{
			GameObject player1UI = GameObject.FindGameObjectWithTag("Player1UI");
			HPBar = player1UI.transform.Find("HPBar").GetComponent<Image>();
			FPBar = player1UI.transform.Find("FPBar").GetComponent<Image>();
		}
		else if (fighterNumber == 1)
		{
			GameObject player2UI = GameObject.FindGameObjectWithTag("Player2UI");
			HPBar = player2UI.transform.Find("HPBar").GetComponent<Image>();
			FPBar = player2UI.transform.Find("FPBar").GetComponent<Image>();
		}
	}

	public void ResetState()
	{
		if (fighterNumber == 0)
		{
			tag = "Player1";
			//spriteRenderer.flipX = false;
		}
		else if (fighterNumber == 1)
		{
			tag = "Player2";
			//spriteRenderer.flipX = true;
		}

		animator.SetInteger("FacingDirection", 0);
		HP = 100;
		FP = 0;
		SetAction(0);
		animator.SetTrigger("RoundStart");
		foreach (BoxCollider2D boxCollider2D in motionColliders)
		{
			boxCollider2D.enabled = false;
		}
	}

	private void OnGround()
	{
		if (fighterAction == FighterAction.JumpAttack) return;

		if (!isGround && groundSensor.State())
		{
			//print("Do");
			isGround = true;
			fighterAction = FighterAction.None;
			animator.SetBool("Grounded", isGround);
			animator.SetBool("Jump", false);
		}
	}

	private void IsFalling()
	{
		if (isGround && !groundSensor.State())
		{
			isGround = false;
			fighterAction = FighterAction.Jump;
			animator.SetBool("Grounded", isGround);
		}
	}

	private void SetAirspeed()
	{
		animator.SetFloat("AirSpeedY", rigidbody2d.velocity.y);
	}

	private void CountCoolTime(float deltaTime)
	{
		if (countChargedAttack > 0)
		{
			countChargedAttack -= deltaTime;
		}
		if (countJumpAttack > 0)
		{
			countJumpAttack -= deltaTime;
		}
	}

	void SetAction(int value)
	{
		if (value < (int)FighterAction.None && value > (int)FighterAction.LethalMove) return;

		fighterAction = (FighterAction)value;
	}

	private void HandleCantInputTime(float deltaTime)
	{
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

	void SetCantInputTime(float value)
	{
		cantInputTime = value;
	}

	public void OnInput()
	{
		cantInputTime = 0;
	}

	public void OffInput()
	{

		cantInputTime = float.MaxValue;
	}
	#endregion

	#region HandleHitBox
	void OnHitBox(int number)
	{
		motionColliders[number].enabled = true;
	}

	void OffHitBox(int number)
	{
		motionColliders[number].enabled = false;
	}
	#endregion

	#region HandleUI
	private void HandleUI()
	{
		HPBar.fillAmount = HP * 0.01f;
		FPBar.fillAmount = FP * 0.01f;
	}
	#endregion

	#region Action
	private void Movement()
	{
		//print(fighterAction);
		if (!canInput) return; // 공격하는데 이동합니다...
		if (fighterAction == FighterAction.None)
		{
			//print("Move");
			bool inputRight = Input.GetKey(KeySetting.keys[fighterNumber, 3]);
			bool inputLeft = Input.GetKey(KeySetting.keys[fighterNumber, 1]);

			int direction = (inputLeft ? -1 : 0) + (inputRight ? 1 : 0);
			// 여기 상대 플레이어 바라보게 만들기!
			facingDirection = direction;
			animator.SetInteger("FacingDirection", facingDirection);

			transform.eulerAngles = (enemyFighter.transform.position.x > transform.position.x ? Vector3.zero : Vector3.up * 180);

			rigidbody2d.velocity = new Vector2(facingDirection * maxSpeed, rigidbody2d.velocity.y);
		}
	}

	private void Jump()
	{
		if (!canInput) return;
		if (!isGround) return;
		if (fighterAction != FighterAction.None) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 0]))
		{
			isGround = false;
			fighterAction = FighterAction.Jump;
			animator.SetBool("Grounded", isGround);
			animator.SetBool("Jump", true);
			rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, jumpForce);
			groundSensor.Disable(0.2f);
		}
	}

	private void Guard()
	{
		if (!canInput) return;
		if (!isGround) return;
        if (fighterAction != FighterAction.None) return;

        if (Input.GetKey(KeySetting.keys[fighterNumber, 2]))
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

	private void Attack()
	{
		if (!canInput) return;
		if (!isGround) return;
        if (fighterAction != FighterAction.None) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
		{
			fighterAction = FighterAction.Attack;
			//print(fighterAction);

			animator.CrossFade("Attack", 0);
		}
	}

	private void ChargedAttack()
	{
		if (!canInput) return;
		if (!isGround) return;
        if (fighterAction != FighterAction.None) return;
        if (countChargedAttack > 0) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 5]))
		{
			fighterAction = FighterAction.ChargedAttack;

			animator.CrossFade("ChargedAttack", 0);

			countChargedAttack = chargedAttackCoolDownTime;
		}
	}

	private void JumpAttack()
	{
		if (!canInput) return;
		if (isGround) return;
        if (fighterAction != FighterAction.Jump) return;
        if (countJumpAttack > 0) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
		{
			fighterAction = FighterAction.JumpAttack;

			animator.CrossFade("JumpAttack", 0);

			countJumpAttack = jumpAttackCoolDownTime;
		}
	}

	private void LethalMove()
	{
		if (!canInput) return;
		if (!isGround) return;
        if (fighterAction != FighterAction.None) return;

        if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 6]))
		{
			fighterAction = FighterAction.LethalMove;

			animator.CrossFade("LethalMove", 0);
		}
	}

	private void Hit(bool isGuard, float enemyRotationY, float lethalMoveCantInputTime)
	{
		Vector2 knockBackPath = enemyRotationY == 0 ? Vector2.right : Vector2.left;

		if (isGuard)
		{
			cantInputTime = lethalMoveCantInputTime > 0 ? lethalMoveCantInputTime : 1;
			rigidbody2d.AddForce(knockBackPath * guardKnockBackPower, ForceMode2D.Impulse);
		}
		else
		{
			cantInputTime = lethalMoveCantInputTime > 0 ? lethalMoveCantInputTime : 1.5f;
			rigidbody2d.AddForce(knockBackPath * hitKnockBackPower, ForceMode2D.Impulse);
		}
	}

	private void Death()
	{
		if (HP <= 0)
		{
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
			{
				animator.CrossFade("Death", 0);
			}
		}
	}
	#endregion

	#region Action Effect
	private void SpawnDustEffect(GameObject dust, float dustXOffset = 0)
	{
		if (dust != null)
		{
			Vector3 dustSpawnPosition = transform.position + new Vector3(dustXOffset * facingDirection, 0f, 0f);
			GameObject newDust = Instantiate(dust, dustSpawnPosition, Quaternion.identity);
			newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(facingDirection, 1, 1);
		}
	}

	void AE_runStop()
	{
		fighterAudio.PlaySound("RunStop");
		float dustXOffset = 0.6f;
		SpawnDustEffect(m_RunStopDust, dustXOffset);
	}

	void AE_footstep()
	{
		fighterAudio.PlaySound("Footstep");
	}

	void AE_Jump()
	{
		fighterAudio.PlaySound("Jump");
		SpawnDustEffect(m_JumpDust);
	}

	void AE_Landing()
	{
		fighterAudio.PlaySound("Landing");
		SpawnDustEffect(m_LandingDust);
	}

	protected void OnLethalMoveScreen()
	{
		lethalMoveScreenClone = Instantiate(lethalMoveScreen);

		lethalMoveScreenClone.SetActive(true);
	}

	protected void OffLethalMoveScreen()
	{
		Destroy(lethalMoveScreenClone);
	}
	#endregion

	#region HandleHitDetection
	private Fighter SearchFighterWithinRange(Collider2D searchRange)
	{
		RaycastHit2D[] raycastHits = Physics2D.BoxCastAll(searchRange.bounds.center, searchRange.bounds.size, 0f, transform.rotation.y == 0 ? Vector2.right : Vector2.left, 0.01f, LayerMask.GetMask("Player"));

		foreach (RaycastHit2D raycastHit in raycastHits)
		{
			if (CompareTag(raycastHit.collider.tag)) continue;

			return raycastHit.collider.GetComponent<Fighter>();
		}

		return null;
	}

	private void GiveDamage(Fighter enemyFighter)
	{
		bool isGuard = false;

		float[] damages = { attackDamage, chargedAttackDamage, jumpAttackDamage, lethalMoveDamage };

		float[] absorptionRates = { attackAbsorptionRate, chargedAttackAbsorptionRate, jumpAttackAbsorptionRate, lethalMoveAbsorptionRate };

		float damage = 0;

		//int count;

		//for (count = 0; count < absorptionRates.Length; count++)
		//{
		//	if (!fighterAction.Equals((FighterAction)count)) continue;
			
			damage += damages[(int)fighterAction-4];

			if (enemyFighter.fighterAction == FighterAction.Guard)
			{
				SetAction(0);

				damage = damages[(int)fighterAction - 4] - damage * absorptionRates[(int)fighterAction - 4];

				isGuard = true;

				cantInputTime = 1.5f;
			}
			else if (!(enemyFighter.fighterAction == FighterAction.None || enemyFighter.fighterAction == FighterAction.Hit))
			{
                damage *= counterDamageRate;
            }

			//break;
		//}

		float lethalMoveCantInputTime = 0;

		if (fighterAction.Equals(FighterAction.LethalMove))
		{
			lethalMoveCantInputTime = lethalMove.length + 0.5f;
		}
		
		enemyFighter.HP -= damage;
		enemyFighter.Hit(isGuard, transform.rotation.y, lethalMoveCantInputTime);
	}
	#endregion
}
