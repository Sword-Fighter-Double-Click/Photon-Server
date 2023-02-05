using UnityEngine;

public class Speero : Fighter
{
	[Header("Speero Object")]
	[SerializeField] GameObject lethalMoveAnimation;

	[Header("Speero Value")]
	[SerializeField] private float jumpAttackRushPower = 30;
	[SerializeField] private float jumpAttackDownPower = 100;

	private BoxCollider2D playerHitBox;

	private GameObject lethalMoveAnimationClone = null;

	private bool usingLethalMoveAnimation;

	protected override void Start()
	{
		base.Start();

		playerHitBox = GetComponent<BoxCollider2D>();
	}

	protected override void Update()
	{
		base.Update();

		if (usingLethalMoveAnimation)
		{
			if (lethalMoveAnimationClone == null)
			{
				lethalMoveAnimationClone = null;
				
				usingLethalMoveAnimation = false;
				
				SetPlayerVisible(1);
				
				OffLethalMoveScreen();
				
				action = MyConstants.Action.None;
			}
		}
	}

	void SetPlayerHitBox(int value)
	{
		if (value == 0)
		{
			playerHitBox.enabled = false;
			cantInputTime = float.MaxValue;
		}
		else if (value == 1)
		{
			playerHitBox.enabled = true;
			cantInputTime = 0;
		}
	}

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

	void MoveDuringJumpAttack()
	{
		rigidbody2d.AddForce(Vector2.down * jumpAttackDownPower + (transform.rotation.y == 0 ? 1 : -1) * jumpAttackRushPower * Vector2.right, ForceMode2D.Impulse);
	}

	void SetCounterDamageRate(float value)
	{
		counterDamageRate = value;
	}

	void HandleLethalMoveAnimation()
	{
		if (hitLethalMove)
		{
			usingLethalMoveAnimation = true;

			lethalMoveAnimationClone = Instantiate(lethalMoveAnimation);
			
			SetPlayerVisible(0);
			
			OnLethalMoveScreen();

			hitLethalMove = false;
		}
		else
		{
			action = MyConstants.Action.None;
		}
	}
}
