using Photon.Pun;
using UnityEngine;

// ĳ���� �߻�ȭ Ŭ���� ������� ��ũ�� ����
public class ArkSha : Fighter
{
	[SerializeField] private GameObject breakEffect;

	[Header("Arksha Value")]
	[SerializeField] private float ultimatePower = 5;
	// �������� �� �󸶳� �̵��ϴ����� ���ϴ� ����
	[SerializeField] private float jumpAttackUpPower = 5;
	[SerializeField] private float jumpAttackDownPower = 100;
	// �������� �� �ּ��� ���߿� ���ִ� �ð��� ���ϴ� ����
	[SerializeField] private float stayJumpAttack = 0.85f;
	[SerializeField] private float maxStayJumpAttack = 1.5f;

	private float jumpAttackDamage;

	/// <summary>
	/// �������� ��� ������ Ȯ���ϴ� ����
	/// </summary>
	private bool isStayJumpAttack;

	/// <summary>
	/// �������� �� ���߿� ���ִ� �ð��� �����ϴ� ����
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
	/// ���� ���� ���. ���߿� ���ִ� �ð� ī��Ʈ
	/// </summary>
	void StayJumpAttack()
	{
		isStayJumpAttack = true;
		pressJumpAttack = false;
		countStayJumpAttack = 0;
	}

	/// <summary>
	/// ���߿� ���ִ� �ð��� ����Ͽ� �������� ������ ����
	/// </summary>
	void SetJumpAttackDamage()
	{
		jumpAttackDamage *= Mathf.Clamp(countStayJumpAttack, 0.3f, 1) / maxStayJumpAttack;
	}

	/// <summary>
	/// �������� ������ �ʱ�ȭ
	/// </summary>
	void InitializeJumpAttackDamage()
	{
		jumpAttackDamage = skills[2].damage;
	}

	/// <summary>
	/// �������� ����
	/// </summary>
    private void TryJumpAttack()
	{
		isStayJumpAttack = false;

		animator.CrossFade("TryJumpAttack", 0);

		// �Ʒ��� �̵�
		MoveDuringJumpAttack(-1);
	}

	/// <summary>
	/// ���� �̵�(1), �Ʒ��� �̵�(0)
	/// </summary>
	/// <param name="path"></param>
	void MoveDuringJumpAttack(int path)
	{
		//rigidBody.velocity = path > 0 ? Vector3.up * jumpAttackUpPower : Vector3.down * jumpAttackDownPower;
		velocity = path > 0 ? Vector3.up * jumpAttackUpPower : Vector3.down * jumpAttackDownPower;
	}

	void MoveDuringUltimate()
	{
		//rigidBody.velocity = Vector3.up * ultimatePower;
		velocity = Vector3.up * ultimatePower;
	}

	/// <summary>
	/// �ñر� ȿ�� ���
	/// </summary>
    void HandleUltimateAnimation()
    {
		// �¾Ҵٸ� 
        if (hitUltimate)
        {
			// �߰�Ÿ �߻�
			animator.CrossFade("HitUltimate", 0);
			// �ִϸ��̼� ���ݷ� �и�!!
			// �ñر� ���ȭ�� Ȱ��ȭ
            OnUltimateScreen();

            hitUltimate = false;
        }
        else
        {
			// �ñر� ���� �ִϸ��̼� ���
			animator.CrossFade("MissUltimate", 0);
        }
    }
}