using System.Collections;
using UnityEngine;

// 캐릭터 추상화 클래스 상속으로 스피로 설계
public class Speero : Fighter
{
	[Header("Speero Object")]
	[SerializeField] private GameObject ultimateAnimation;

	// 점프공격 시 얼마나 이동하는지를 정하는 변수
	[Header("Speero Value")]
	[SerializeField] private float jumpAttackRushPower = 30;
	[SerializeField] private float jumpAttackDownPower = 100;

	/// <summary>
	/// 생성된 궁극기 애니메이션 오브젝트의 데이터를 저장하는 변수
	/// </summary>
	private GameObject ultimateAnimationClone = null;

	private bool usingUltimateAnimation;

	protected override void Update()
	{
		base.Update();

		// 궁극기를 사용
		if (usingUltimateAnimation)
		{
			// 애니메이션이 출력되고 삭제되었다면
			if (ultimateAnimationClone == null)
			{
				ultimateAnimationClone = null;
				
				usingUltimateAnimation = false;
				
				SetPlayerVisible(1);
				
				OffUltimateScreen();
				
				// IDLE 상태로 초기화
				fighterAction = FighterAction.None;
			}
		}
	}

	/// <summary>
	/// 캐릭터 이미지 활성화(1), 비활성화(0)
	/// </summary>
	/// <param name="value"></param>
	void SetPlayerVisible(int value)
	{
		if (value == 0)
		{
			spriteRenderer.enabled = false;

		}
		else if (value == 1)
		{
			spriteRenderer.enabled = true;
		}
	}

	/// <summary>
	/// 점프공격 시 위치 이동
	/// </summary>
	void MoveDuringJumpAttack()
	{
		animator.SetBool("Jump", false);
		rigidBody.velocity = Vector2.down * jumpAttackDownPower + (transform.rotation.y == 0 ? 1 : -1) * jumpAttackRushPower * Vector2.right;
		isGround = true;
		animator.SetBool("Grounded", true);
	}

	/// <summary>
	/// 카운터 데미지 배율 설정
	/// </summary>
	/// <param name="value"></param>
	void SetCounterDamageRate(float value)
	{
		status.counterDamageRate = value;
	}

	/// <summary>
	/// 적의 궁극기 피격 여부에 따라 애니메이션 출력
	/// </summary>
	void HandleUltimate()
	{
		hitUltimate = false;

		// 맞았다면
		if (counterAttack)
		{
			SetCounterDamageRate(2);
			enemyFighter.fighterAction = FighterAction.Attack;
			//OnUltimateScreen();
			StartCoroutine(UltimateHit());
		}
		// 맞지 않았다면
		else
		{
			// IDLE 상태로 초기화
			fighterAction = FighterAction.None;
		}
	}

	private IEnumerator UltimateHit()
	{
		yield return new WaitForSeconds(0.5f);
		
		animator.CrossFade("UltimateHit", 0f);
	}

	void SpawnAnimation()
	{
		SetCounterDamageRate(1.2f);
		SetPlayerVisible(0);

		usingUltimateAnimation = true;
		ultimateAnimationClone = Instantiate(ultimateAnimation);
	}
}
