# Sword Fighter

<h4>2022 2학기 대소고 교내 18팀 TeamInterlink 해커톤 작품 리메이크 (웹툰 '더블클릭' IP를 활용한 2D 대전 격투 게임)</h4>

<hr class='hr-solid'/>

<h4>게임 시연</h4>

<A href=""> 게임 시연 영상 </A><br>

<hr class='hr-solid'/>

<h3>시스템 구조</h3>

<details>
<summary><i>인 게임 이미지</i></summary>
<br>
 - 타이틀<br>
  <img width="640" alt="image" src="https://user-images.githubusercontent.com/80941288/230789018-66cde7ee-d0c4-459d-9ddc-42888fc271e2.png"><br>
  <br>
 - 플레이<br>
  <img width="640" alt="image" src="https://user-images.githubusercontent.com/80941288/230789074-14989f2e-1d97-4a7b-add3-1a296e0581ee.png"><br>
  <img width="640" alt="image" src="https://user-images.githubusercontent.com/80941288/230789086-9b578934-e1c3-4975-816f-bdb4c3444a3c.png"><br>
  <img width="640" alt="image" src="https://user-images.githubusercontent.com/80941288/230789101-d6e495ff-f414-424b-8979-898becf0edd1.png"><br>
  <img width="640" alt="image" src="https://user-images.githubusercontent.com/80941288/230789143-04b217af-5834-4c7d-9c2a-427d0ad3da76.png"><br>
  <img width="640" alt="image" src="https://user-images.githubusercontent.com/80941288/230789183-ea1429a4-a4eb-40ad-9249-48e5210c2131.png"><br>
  <img width="640" alt="image" src="https://user-images.githubusercontent.com/80941288/230789206-a2b192c7-5d30-4a86-af60-5be5c9012ee7.png"><br>
  <img width="640" alt="image" src="https://user-images.githubusercontent.com/80941288/230789224-2420ca2e-a840-43a4-95f4-3c609be255af.png"><br>
  <img width="640" alt="image" src="https://user-images.githubusercontent.com/80941288/230789241-bde6467b-f0c9-4796-89a6-e4a990f1cfe9.png"><br>
  <br>
</details>

<img width="700" alt="image" src="https://user-images.githubusercontent.com/80941288/230789744-d4c262cd-9d4a-48e5-becd-e26020b80979.png"><br>

<hr class='hr-solid'/>

<h3>주요 코드</h3>
<b>Fighter</b><br>
&nbsp;&nbsp;&nbsp;&nbsp;● 캐릭터의 모체가 되는 추상화 클래스입니다.<br>
&nbsp;&nbsp;&nbsp;&nbsp;● 이동, 공격, 피격, 가드, 그라운드 체크, 애니메이션 변경 등의 기능이 있습니다.
<details>
    <summary><i>자세한 코드</i></summary>
    
  ```C#
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

	// 기존 에셋에 있던 먼지 효과 프리팹 오브젝트
	[SerializeField] private GameObject m_RunStopDust;
	[SerializeField] private GameObject m_JumpDust;
	[SerializeField] private GameObject m_LandingDust;

	[Header("Action")]
	// Attack, None, Guard와 같이 플레이어의 상태를 지정하는 변수
	public FighterAction fighterAction;

	[Header("Value")]
	// 0번은 왼쪽 플레이어, 1번은 오른쪽 플레이어로 지정됨
	[Range(0, 1)] public int fighterNumber = 0;
	// 키 입력 여부를 정하는 변수
	public bool canInput = true;
	// 상대 캐릭터 클래스 저장 변수
	public Fighter enemyFighter;

	// 최대 속도, 점프 높이, 카운터 데미지 배율, 공격 데미지, 가드 시 데미지 감소율 등등 여러 스테이터스 값을 저장하는 변수들
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

	// 그라운드 체크 변수
	protected bool isGround = false;
	// 이동 방향 변수 -1은 좌, 1은 우
	private int facingDirection = 0;

	// 강공격과 점프공격의 쿨타임을 계산하는 변수
	private float countChargedAttack;
	private float countJumpAttack;

	// 키 입력할 수 없는 시간을 정하는 변수
	protected float cantInputTime = 0;

	// 적이 궁극기에 맞았는지를 저장하는 변수
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

	// 자식 클래스에서도 쓸 수 있도록 추상화
	protected virtual void Start()
	{
		SettingUI();
	}

	// 자식 클래스에서도 쓸 수 있도록 추상화
	protected virtual void Update()
	{
		HandleCantInputTime(Time.deltaTime);

		Death();

		OnGround();

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

		// 켜져 있는 Collider의 크기를 이용하여 공격판정을 생성하고 적이 맞았는지 확인하는 반복문
		foreach (BoxCollider2D collider in motionColliders)
		{
			if (!collider.enabled) continue;

			Fighter enemyFighter = SearchFighterWithinRange(collider);

			if (enemyFighter == null) continue;

			// 적이 맞았다면

			if (fighterAction == FighterAction.LethalMove)
			{
				hitLethalMove = true;
			}

			collider.enabled = false;

			// 데미지를 가함
			GiveDamage(enemyFighter);
		}
	}

	#region HandleValue

	// 현재 fighterNumber에 따라 UI 캐싱
	private void SettingUI()
	{
		if (fighterNumber == 0)
		{
			GameObject player1UI = GameObject.FindGameObjectWithTag("Player1UI");
			HPBar = player1UI.transform.Find("HPBar").GetComponent<Image>();
			FPBar = player1UI.transform.Find("FPBar").GetComponent<Image>();
			rigidbody2d.sharedMaterial = GameObject.Find("Fighter1PhysicsMaterial").GetComponent<Rigidbody2D>().sharedMaterial;
		}
		else if (fighterNumber == 1)
		{
			GameObject player2UI = GameObject.FindGameObjectWithTag("Player2UI");
			HPBar = player2UI.transform.Find("HPBar").GetComponent<Image>();
			FPBar = player2UI.transform.Find("FPBar").GetComponent<Image>();
			rigidbody2d.sharedMaterial = GameObject.Find("Fighter2PhysicsMaterial").GetComponent<Rigidbody2D>().sharedMaterial;
		}
	}

	// fighterNumber에 따라 태그 설정 및 스테이터스, 애니메이션, 공격판정 초기화
	public void ResetState()
	{
		// 태그 설정
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

		// 애니메이션 및 스테이터스 초기화
		animator.SetInteger("FacingDirection", 0);
		HP = 100;
		FP = 0;
		SetAction(0);
		animator.SetTrigger("RoundStart");

		// 공격판정 초기화
		foreach (BoxCollider2D boxCollider2D in motionColliders)
		{
			boxCollider2D.enabled = false;
		}
	}

	// 그라운드 체크
	private void OnGround()
	{
		if (fighterAction == FighterAction.JumpAttack) return;

		if (!isGround && groundSensor.State())
		{
			// 마찰력 10으로 설정
			rigidbody2d.sharedMaterial.friction = 10;
			isGround = true;
			fighterAction = FighterAction.None;
			animator.SetBool("Grounded", isGround);
			animator.SetBool("Jump", false);
		}
	}

	// AirSpeed 값 할당
	private void SetAirspeed()
	{
		// airSpeedY가 0 이하가 되면 낙하 애니메이션 출력
		animator.SetFloat("AirSpeedY", rigidbody2d.velocity.y);
	}

	// 강공격과 점프공격 쿨타임 계산
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

	// 플레이어의 상태를 정수로 설정
	void SetAction(int value)
	{
		if (value < (int)FighterAction.None && value > (int)FighterAction.LethalMove) return;

		fighterAction = (FighterAction)value;
	}

	// 입력 불가 시간 계산
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

	// 입력 불가 시간 설정
	void SetCantInputTime(float value)
	{
		cantInputTime = value;
	}

	// 입력이 가능하게 설정
	public void OnInput()
	{
		cantInputTime = 0;
	}

	// 입력이 불가능하게 설정
	public void OffInput()
	{
		cantInputTime = float.MaxValue;
	}
	#endregion

	#region HandleHitBox
	// 인자 내 정수값에 해당하는 공격 판정 collider 활성화
	void OnHitBox(int number)
	{
		motionColliders[number].enabled = true;
	}

	// 인자 내 정수값에 해당하는 공격 판정 collider 비활성화
	void OffHitBox(int number)
	{
		motionColliders[number].enabled = false;
	}
	#endregion

	#region HandleUI
	// HP 및 스킬 게이지 UI에 표현
	private void HandleUI()
	{
		HPBar.fillAmount = HP * 0.01f;
		FPBar.fillAmount = FP * 0.01f;
	}
	#endregion

	#region Action
	// 이동 및 플레이어 방향 조정
	private void Movement()
	{
		if (!canInput) return;
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

		// 이동 방향에 따라 이미지 방향 설정
		transform.eulerAngles = (enemyFighter.transform.position.x > transform.position.x ? Vector3.zero : Vector3.up * 180);
		
		// 이동
		rigidbody2d.velocity = new Vector2(facingDirection * maxSpeed, rigidbody2d.velocity.y);
	}

	// 점프
	private void Jump()
	{
		if (!canInput) return;
		if (!isGround) return;
		// IDLE 상태에만 함수 진입
		if (fighterAction != FighterAction.None) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 0]))
		{
			// 벽끼임 방지를 위해 마찰력 0으로 설정
			rigidbody2d.sharedMaterial.friction = 0;
			isGround = false;
			fighterAction = FighterAction.Jump;
			animator.SetBool("Grounded", isGround);
			// 애니메이션 출력
			animator.SetBool("Jump", true);
			// 점프
			rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, jumpForce);
			groundSensor.Disable(0.2f);
		}
	}
	
	// 가드
	private void Guard()
	{
		if (!canInput) return;
		if (!isGround) return;
		// IDLE 및 가드 상태에만 함수 진입
		if (!(fighterAction == FighterAction.None || fighterAction == FighterAction.Guard)) return;

		// 키를 누르고 있으면 가드 활성화, 떼면 가드 비활성화
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

	// 일반공격
	private void Attack()
	{
		if (!canInput) return;
		if (!isGround) return;
		// IDLE 상태에만 함수 진입
		if (fighterAction != FighterAction.None) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
		{
			fighterAction = FighterAction.Attack;

			animator.CrossFade("Attack", 0);
		}
	}

	// 강공격
	private void ChargedAttack()
	{
		if (!canInput) return;
		if (!isGround) return;
		// IDLE 상태에만 함수 진입
		if (fighterAction != FighterAction.None) return;
		if (countChargedAttack > 0) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 5]))
		{
			fighterAction = FighterAction.ChargedAttack;

			animator.CrossFade("ChargedAttack", 0);
			
			// 강공격 이후 쿨타임 설정
			countChargedAttack = chargedAttackCoolDownTime;
		}
	}

	// 점프공격
	private void JumpAttack()
	{
		if (!canInput) return;
		if (isGround) return;
		// 점프 상태에서만 함수 진입
		if (fighterAction != FighterAction.Jump) return;
		if (countJumpAttack > 0) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
		{
			fighterAction = FighterAction.JumpAttack;

			animator.CrossFade("JumpAttack", 0);
			
			// 점프공격 이후 쿨타임 설정
			countJumpAttack = jumpAttackCoolDownTime;
		}
	}

	// 궁극기
	private void LethalMove()
	{
		if (!canInput) return;
		if (!isGround) return;
		// IDLE 상태에서만 함수 진입
		if (fighterAction != FighterAction.None) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 6]))
		{
			fighterAction = FighterAction.LethalMove;

			animator.CrossFade("LethalMove", 0);
		}
	}

	// 피격
	private void Hit(bool isGuard, float enemyRotationY, float lethalMoveCantInputTime)
	{
		Vector2 knockBackPath = enemyRotationY == 0 ? Vector2.right : Vector2.left;

		// 가드 시 입력 불가와 넉백이 시간이 가드를 안했을 때보다 줄어듭니다.
		// 궁극기는 시전시간이 끝날 때까지 고정적으로 입력을 못하게 만듭니다.
		if (isGuard)
		{
			// 입력 불가 시간 설정
			cantInputTime = lethalMoveCantInputTime > 0 ? lethalMoveCantInputTime : 0.1f;
			// 넉백
			rigidbody2d.AddForce(knockBackPath * guardKnockBackPower, ForceMode2D.Impulse);
		}
		else
		{
			// 현재 공격 중단
			for (int count = 0; count < motionColliders.Length; count++)
			{
				OffHitBox(count);
			}
			SetAction(0);
			// 입력 불가 시간 설정
			cantInputTime = lethalMoveCantInputTime > 0 ? lethalMoveCantInputTime : 0.3f;
			// 넉백
			rigidbody2d.AddForce(knockBackPath * hitKnockBackPower, ForceMode2D.Impulse);
		}
	}

	// 사망
	private void Death()
	{
		// HP가 0이 되면 애니메이션을 출력
		if (HP <= 0)
		{
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
			{
				animator.CrossFade("Death", 0);
			}
		}
	}
	#endregion

	// 에셋 먼지 효과 함수
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
	// 공격판정 생성 및 공격판정에 인식된 적 플레이어 데이터 반환
	private Fighter SearchFighterWithinRange(Collider2D searchRange)
	{
		// BoxCast로 공격판정 생성
		RaycastHit2D[] raycastHits = Physics2D.BoxCastAll(searchRange.bounds.center, searchRange.bounds.size, 0f, transform.rotation.y == 0 ? Vector2.right : Vector2.left, 0.01f, LayerMask.GetMask("Player"));

		foreach (RaycastHit2D raycastHit in raycastHits)
		{
			// 태그가 같으면, 즉 나 자신이면 다음 코드를 실행하지 않음
			if (CompareTag(raycastHit.collider.tag)) continue;

			// 공격판정에 들어온 적 플레이어 데이터 반환
			return raycastHit.collider.GetComponent<Fighter>();
		}

		// 찾지 못하면 null 반환
		return null;
	}

	// 적 플레이어에게 데미지를 가하는 함수
	private void GiveDamage(Fighter enemyFighter)
	{
		bool isGuard = false;

		// 공격 별 데미지 배열에 저장
		float[] damages = { attackDamage, chargedAttackDamage, jumpAttackDamage, lethalMoveDamage };

		// 공격 별 가드 시 데미지 감소율 배열에 저장
		float[] absorptionRates = { attackAbsorptionRate, chargedAttackAbsorptionRate, jumpAttackAbsorptionRate, lethalMoveAbsorptionRate };

		float damage = 0;

		// 피격 당해 입력 불가 시간이 있다면 아래 코드를 실행하지 않음
		if (cantInputTime > 0) return;

		// 현재 공격의 데미지 저장
		damage += damages[(int)fighterAction - 4];

		// 가드 시 데미지 감소
		if (enemyFighter.fighterAction == FighterAction.Guard)
		{
			damage = damages[(int)fighterAction - 4] - damages[(int)fighterAction - 4] * absorptionRates[(int)fighterAction - 4];

			isGuard = true;

			cantInputTime = 0.5f;

			SetAction(0);
		}
		// 적 플레이어가 공격 애니메이션일 시, 카운데 배율을 곱하여 데미지 증가
		else if (!(enemyFighter.fighterAction == FighterAction.None || enemyFighter.fighterAction == FighterAction.Hit))
		{
			damage *= counterDamageRate;
		}

		// 현재 공격이 궁극기일 시, 시전시간+0.5초의 입력 불가 시간을 적에게 적용한다.
		float lethalMoveCantInputTime = 0;
		if (fighterAction == FighterAction.LethalMove)
		{
			lethalMoveCantInputTime = lethalMove.length + 0.5f;
		}

		enemyFighter.HP -= damage;
		enemyFighter.Hit(isGuard, transform.rotation.y, lethalMoveCantInputTime);
	}
	#endregion
}
  ```
</details><br>

<b>Speero</b><br>
&nbsp;&nbsp;&nbsp;&nbsp;● .<br>
&nbsp;&nbsp;&nbsp;&nbsp;● .
<details>
    <summary><i>자세한 코드</i></summary>
    
  ```C#
  
  ```
</details><br>

<b>ArkSha</b><br>
&nbsp;&nbsp;&nbsp;&nbsp;● .<br>
&nbsp;&nbsp;&nbsp;&nbsp;● .
<details>
    <summary><i>자세한 코드</i></summary>
    
  ```C#
  ```
</details>
