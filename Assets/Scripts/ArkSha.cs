using UnityEngine;

public class ArkSha : Fighter
{
	[Header("Arksha Value")]
	[SerializeField] private float jumpAttackUpPower = 30;
	[SerializeField] private float jumpAttackDownPower = 100;

	private BoxCollider2D playerHitBox;

	protected override void Start()
	{
		base.Start();

		playerHitBox = GetComponent<BoxCollider2D>();
	}

	protected override void Update()
	{
		base.Update();
	}

	void MoveDuringJumpAttack()
	{
		//rigidbody2d.AddForce(Vector2.down * jumpAttackDownPower + (transform.rotation.y == 0 ? 1 : -1) * jumpAttackRushPower * Vector2.right, ForceMode2D.Impulse);
	}
}
