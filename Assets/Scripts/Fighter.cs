using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
	public MyConstants.Action action;

	[Header("Value")]
	[Range(0, 1)] public int fighterNumber = 0;
	public bool canInput = true;

	[Header("Stats")]
	public float HP = 100;
	public float FP = 0;
	[SerializeField] protected float maxSpeed = 4.5f;
	[SerializeField] protected float jumpForce = 7.5f;
	[SerializeField] protected float counterDamageRate = 1.2f;
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

	protected virtual void Start()
	{
		animator = GetComponent<Animator>();
		rigidbody2d = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		fighterAudio = transform.Find("Audio").GetComponent<FighterAudio>();
		groundSensor = transform.Find("GroundSensor").GetComponent<Sensor>();

		ResetTag();
	}

	protected virtual void Update()
	{
		HandleCantInputTime(Time.deltaTime);

		Death();

		OnGround();

		IsFalling();

		SetAirspeed();

		CountCoolTime(Time.deltaTime);

		//HandleUI();

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

			if (action == MyConstants.Action.LethalMove)
			{
				hitLethalMove = true;
			}

			collider.enabled = false;

			GiveDamage(enemyFighter);
		}
	}

	#region HandleValue

	public void ResetTag()
	{
        if (fighterNumber == 0)
        {
            tag = "Player1";
        }
        else if (fighterNumber == 1)
        {
            tag = "Player2";
        }
    }

	public void ResetState()
	{
		SetAction(0);
		foreach (BoxCollider2D boxCollider2D in motionColliders)
		{
			boxCollider2D.enabled = false;
		}
	}

	private void OnGround()
	{
		if (action == MyConstants.Action.JumpAttack) return;

		if (!isGround && groundSensor.State())
		{
			isGround = true;
			action = MyConstants.Action.None;
			animator.SetBool("Grounded", isGround);
			animator.SetBool("Jump", false);
		}
	}

	private void IsFalling()
	{
		if (isGround && !groundSensor.State())
		{
			isGround = false;
			action = MyConstants.Action.Jump;
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
		if (value <= 0)
		{
			action = MyConstants.Action.None;

			return;
		}

		action = (MyConstants.Action)(int)Mathf.Pow(2, value - 1);
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
		canInput = true;
	}

	public void OffInput()
	{
		canInput = false;
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
		if (!canInput) return;
		if ((action & (MyConstants.Action.Attack | MyConstants.Action.ChargedAttack | MyConstants.Action.JumpAttack | MyConstants.Action.LethalMove)) != 0) return;

		bool inputRight = Input.GetKey(KeySetting.keys[fighterNumber, 3]);
		bool inputLeft = Input.GetKey(KeySetting.keys[fighterNumber, 1]);

		facingDirection = (inputLeft ? -1 : 0) + (inputRight ? 1 : 0);
		animator.SetInteger("FacingDirection", facingDirection);
		transform.eulerAngles = Vector3.up * (inputLeft ? 180 : 0);

		rigidbody2d.velocity = new Vector2(facingDirection * maxSpeed, rigidbody2d.velocity.y);
	}

	private void Jump()
	{
		if (!canInput) return;
		if (!isGround) return;
		if ((action & (MyConstants.Action.Attack | MyConstants.Action.ChargedAttack | MyConstants.Action.JumpAttack | MyConstants.Action.LethalMove)) != 0) return;

		if (Input.GetKeyDown(MyConstants.KeySetting.keys[fighterNumber, 0]))
		{
			isGround = false;
			action = MyConstants.Action.Jump;
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
		if (action == MyConstants.Action.Jump) return;

		if (Input.GetKey(KeySetting.keys[fighterNumber, 2]))
		{
			action = MyConstants.Action.Guard;

			animator.CrossFade("Guard", 0.2f);
		}
		else if (Input.GetKeyUp(KeySetting.keys[fighterNumber, 2]))
		{
			action = MyConstants.Action.None;
		}
	}

	private void Attack()
	{
		if (!canInput) return;
		if (!isGround) return;
		if (action != MyConstants.Action.None) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
		{
			action = MyConstants.Action.Attack;

			animator.CrossFade("Attack", 0);
		}
	}

	private void ChargedAttack()
	{
		if (!canInput) return;
		if (!isGround) return;
		if (action != MyConstants.Action.None) return;
		if (countChargedAttack > 0) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 5]))
		{
			action = MyConstants.Action.ChargedAttack;

			animator.CrossFade("ChargedAttack", 0);

			countChargedAttack = chargedAttackCoolDownTime;
		}
	}

	private void JumpAttack()
	{
		if (!canInput) return;
		if (isGround) return;
		if (action != MyConstants.Action.Jump) return;
		if (countJumpAttack > 0) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
		{
			action = MyConstants.Action.JumpAttack;

			animator.CrossFade("JumpAttack", 0);

			countJumpAttack = jumpAttackCoolDownTime;
		}
	}

	private void LethalMove()
	{
		if (!canInput) return;
		if (!isGround) return;
		if (action != MyConstants.Action.None) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 6]))
		{
			action = MyConstants.Action.LethalMove;

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
			Vector3 dustSpawnPosition = transform.position + new Vector3(dustXOffset * facingDirection, 0.0f, 0.0f);
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

		int count;

		for (count = 0; count < absorptionRates.Length; count++)
		{
			if (action != (MyConstants.Action)(4 * Mathf.Pow(2, count))) continue;

			damage += damages[count];

			if (enemyFighter.action == MyConstants.Action.Guard)
			{
				damage *= absorptionRates[count];

				isGuard = true;

				cantInputTime = 1.5f;
			}

			if ((enemyFighter.action & (MyConstants.Action.None | MyConstants.Action.Hit | MyConstants.Action.Guard)) == 0)
			{
				damage *= counterDamageRate;
			}

			break;
		}

		float lethalMoveCantInputTime = 0;

		if (count + 1 == absorptionRates.Length)
		{
			lethalMoveCantInputTime = lethalMove.length + 0.5f;
		}

		enemyFighter.HP -= damage;
		enemyFighter.Hit(isGuard, transform.rotation.y, lethalMoveCantInputTime);
	}
	#endregion
}
