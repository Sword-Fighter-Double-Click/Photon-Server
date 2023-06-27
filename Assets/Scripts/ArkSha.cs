using UnityEngine;

// 캐릭터 추상화 클래스 상속으로 아크샤 설계
public class ArkSha : Fighter
{
	[SerializeField] private GameObject breakEffect;

	[Header("Arksha Value")]
	[SerializeField] private float ultimatePower = 5;
	// 점프공격 시 얼마나 이동하는지를 정하는 변수
	[SerializeField] private float jumpAttackUpPower = 5;
	[SerializeField] private float jumpAttackDownPower = 100;
	// 점프공격 시 최소한 공중에 떠있는 시간을 정하는 변수
	[SerializeField] private float stayJumpAttack = 0.85f;
	[SerializeField] private float maxStayJumpAttack = 1.5f;

	private float jumpAttackDamage;

	/// <summary>
	/// 점프공격 대기 중인지 확인하는 변수
	/// </summary>
	private bool isStayJumpAttack;

	/// <summary>
	/// 점프공격 시 공중에 떠있는 시간을 저장하는 변수
	/// </summary>
	private float countStayJumpAttack;

	private bool pressJumpAttack;

	private bool isLethalMove;

	protected override void Update()
	{
		base.Update();

		if (!isStayJumpAttack) return;

		if (fighterAction == FighterAction.Hit)
		{
			isStayJumpAttack = false;
			return;
		}

		countStayJumpAttack += Time.deltaTime;

		pressJumpAttack = Input.GetKey(KeySetting.keys[fighterNumber, 4]);

		if ((pressJumpAttack ? maxStayJumpAttack : stayJumpAttack) - countStayJumpAttack <= 0 || isGround)
		{
			TryJumpAttack();
		}
	}

	/// <summary>
	/// 점프 공격 대기. 공중에 떠있는 시간 카운트
	/// </summary>
	void StayJumpAttack()
	{
		isStayJumpAttack = true;
		pressJumpAttack = false;
		countStayJumpAttack = 0;
	}

	/// <summary>
	/// 공중에 떠있는 시간에 비례하여 점프공격 데미지 설정
	/// </summary>
	void SetJumpAttackDamage()
	{
		jumpAttackDamage *= Mathf.Clamp(countStayJumpAttack, 0.3f, 1) / maxStayJumpAttack;
	}

	/// <summary>
	/// 점프공격 데미지 초기화
	/// </summary>
	void InitializeJumpAttackDamage()
	{
		jumpAttackDamage = skills[2].damage;
	}

	/// <summary>
	/// 점프공격 실행
	/// </summary>
    private void TryJumpAttack()
	{
		isStayJumpAttack = false;

		animator.CrossFade("TryJumpAttack", 0);

		// 아래로 이동
		MoveDuringJumpAttack(-1);
	}

	/// <summary>
	/// 위로 이동(1), 아래로 이동(0)
	/// </summary>
	/// <param name="path"></param>
	void MoveDuringJumpAttack(int path)
	{
		rigidBody.velocity = path > 0 ? Vector3.up * jumpAttackUpPower : Vector3.down * jumpAttackDownPower;
	}

	void MoveDuringUltimate()
	{
		rigidBody.velocity = Vector3.up * ultimatePower;
	}

	/// <summary>
	/// 궁극기 효과 출력
	/// </summary>
    void HandleUltimateAnimation()
    {
		// 맞았다면 
        if (hitUltimate)
        {
			// 추가타 발생
			animator.CrossFade("HitUltimate", 0);
			// 애니메이션 공격력 분리!!
			// 궁극기 배경화면 활성화
            OnUltimateScreen();

            hitUltimate = false;
        }
        else
        {
			// 궁극기 종료 애니메이션 출력
			animator.CrossFade("MissUltimate", 0);
        }
    }
}