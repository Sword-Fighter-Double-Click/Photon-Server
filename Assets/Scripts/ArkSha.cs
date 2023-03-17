using UnityEngine;

public class ArkSha : Fighter
{
	[Header("Arksha Value")]
	[SerializeField] private float jumpAttackUpPower = 5;
	[SerializeField] private float jumpAttackDownPower = 100;
	[SerializeField] private float stayJumpAttack = 0.85f;

	private float originalJumpAttackDamage;

	private bool isStayJumpAttack;

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
			if (Input.GetKeyUp(MyConstants.KeySetting.keys[fighterNumber, 4]))
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

	void StayJumpAttack()
	{
		isStayJumpAttack = true;
		countStayJumpAttack = stayJumpAttack;
	}

	void SetJumpAttackDamage()
	{
		jumpAttackDamage *= (stayJumpAttack - countStayJumpAttack) / stayJumpAttack;
	}

	void InitializeJumpAttackDamage()
	{
		jumpAttackDamage = originalJumpAttackDamage;
	}


    private void TryJumpAttack()

	{
		isStayJumpAttack = false;

		animator.CrossFade("TryJumpAttack", 0);
		MoveDuringJumpAttack(-1);
	}

	void MoveDuringJumpAttack(int path)
	{
		rigidbody2d.AddForce(path > 0 ? Vector2.up * jumpAttackUpPower : Vector2.down * jumpAttackDownPower, ForceMode2D.Impulse);
	}

    void HandleLethalMoveAnimation()
    {
        if (hitLethalMove)
        {
			animator.CrossFade("HitLethalMove", 0);

            OnLethalMoveScreen();

            hitLethalMove = false;
        }
        else
        {
			animator.CrossFade("MissLethalMove", 0);
        }
    }
}