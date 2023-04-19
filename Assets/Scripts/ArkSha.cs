using UnityEngine;

// 캐릭터 추상화 클래스 상속으로 아크샤 설계
public class ArkSha : Fighter
{
	[Header("Arksha Value")]
	// 점프공격 시 얼마나 이동하는지를 정하는 변수
	[SerializeField] private float jumpAttackUpPower = 5;
	[SerializeField] private float jumpAttackDownPower = 100;
	// 점프공격 시 최소한 공중에 떠있는 시간을 정하는 변수
	[SerializeField] private float stayJumpAttack = 0.85f;

	private float originalJumpAttackDamage;

	/// <summary>
	/// 점프공격 대기 중인지 확인하는 변수
	/// </summary>
	private bool isStayJumpAttack;

	/// <summary>
	/// 점프공격 시 공중에 떠있는 시간을 저장하는 변수
	/// </summary>
	private float countStayJumpAttack;

	private bool isLethalMove;

	protected override void Start()
	{
		base.Start();

		originalJumpAttackDamage = jumpAttackDamage;
	}

	protected override void Update()
	{
		base.Update();

		if (!isStayJumpAttack) return;

		countStayJumpAttack -= Time.deltaTime;

		if (countStayJumpAttack > 0)
		{
			if (Input.GetKeyUp(KeySetting.keys[fighterNumber, 4]))
			{
				TryJumpAttack();
			}
		}
		else
		{
			countStayJumpAttack = 0;
			TryJumpAttack();
		}
	}

	/// <summary>
	/// 점프 공격 대기. 공중에 떠있는 시간 카운트
	/// </summary>
	void StayJumpAttack()
	{
		isStayJumpAttack = true;
		countStayJumpAttack = stayJumpAttack;
	}

	/// <summary>
	/// 공중에 떠있는 시간에 비례하여 점프공격 데미지 설정
	/// </summary>
	void SetJumpAttackDamage()
	{
		jumpAttackDamage *= (stayJumpAttack - countStayJumpAttack) / stayJumpAttack;
	}

	/// <summary>
	/// 점프공격 데미지 초기화
	/// </summary>
	void InitializeJumpAttackDamage()
	{
		jumpAttackDamage = originalJumpAttackDamage;
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
		rigidBody.AddForce(path > 0 ? Vector2.up * jumpAttackUpPower : Vector2.down * jumpAttackDownPower, ForceMode.Impulse);
	}

	/// <summary>
	/// 궁극기 효과 출력
	/// </summary>
    void HandleLethalMoveAnimation()
    {
		// 맞았다면
        if (hitLethalMove)
        {
			// 추가타 발생
			animator.CrossFade("HitLethalMove", 0);

			// 궁극기 배경화면 활성화
            OnLethalMoveScreen();

            hitLethalMove = false;
        }
        else
        {
			// 궁극기 종료 애니메이션 출력
			animator.CrossFade("MissLethalMove", 0);
        }
    }
}