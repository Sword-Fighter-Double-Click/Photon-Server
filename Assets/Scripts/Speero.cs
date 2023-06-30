using Photon.Pun;
using System.Collections;
using UnityEngine;

// ĳ���� �߻�ȭ Ŭ���� ������� ���Ƿ� ����
public class Speero : Fighter
{
	[Header("Speero Object")]
	[SerializeField] private GameObject ultimateAnimation;

	// �������� �� �󸶳� �̵��ϴ����� ���ϴ� ����
	[Header("Speero Value")]
	[SerializeField] private float jumpAttackRushPower = 30;
	[SerializeField] private float jumpAttackDownPower = 100;
	[SerializeField] private float backDashAttackRushPower = 3;

	/// <summary>
	/// ������ �ñر� �ִϸ��̼� ������Ʈ�� �����͸� �����ϴ� ����
	/// </summary>
	private GameObject ultimateAnimationClone = null;

	private bool usingUltimateAnimation;

    protected override void Update()
	{
		base.Update();

		// �ñر⸦ ���
		if (usingUltimateAnimation)
		{
			// �ִϸ��̼��� ��µǰ� �����Ǿ��ٸ�
			if (ultimateAnimationClone == null)
			{
				ultimateAnimationClone = null;
				
				usingUltimateAnimation = false;
				
				SetPlayerVisible(1);
				
				OffUltimateScreen();
				
				// IDLE ���·� �ʱ�ȭ
				fighterAction = FighterAction.None;
			}
		}
	}

	/// <summary>
	/// ĳ���� �̹��� Ȱ��ȭ(1), ��Ȱ��ȭ(0)
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
	/// �������� �� ��ġ �̵�
	/// </summary>
	void MoveDuringJumpAttack()
	{
		animator.SetBool("Jump", false);
		//rigidBody.velocity = Vector2.down * jumpAttackDownPower + (transform.rotation.y == 0 ? 1 : -1) * jumpAttackRushPower * Vector2.right;
		velocity = Vector2.down * jumpAttackDownPower + (transform.rotation.y == 0 ? 1 : -1) * jumpAttackRushPower * Vector2.right;
		isGround = true;
		animator.SetBool("Grounded", true);
	}

	void MoveDuringBackDashAttack()
	{
        velocity.x -= (int)fighterPosition * backDashAttackRushPower;
    }

	/// <summary>
	/// ī���� ������ ���� ����
	/// </summary>
	/// <param name="value"></param>
	void SetCounterDamageRate(float value)
	{
		status.counterDamageRate = value;
	}

	/// <summary>
	/// ���� �ñر� �ǰ� ���ο� ���� �ִϸ��̼� ���
	/// </summary>
	void HandleUltimate()
	{
		hitUltimate = false;

		// �¾Ҵٸ�
		if (counterAttack)
		{
			SetCounterDamageRate(2);
			enemyFighter.fighterAction = FighterAction.Attack;
			//OnUltimateScreen();
			StartCoroutine(UltimateHit());
		}
		// ���� �ʾҴٸ�
		else
		{
			// IDLE ���·� �ʱ�ȭ
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
