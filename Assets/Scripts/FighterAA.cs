using Photon.Pun;
using System;
using UnityEngine;

public class FighterAA : Prototype, IAction
{
	private bool attacking = false;

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();

		if (!m_photonView.IsMine) return;

		Direction direction = Direction.None;
		
		if (!Input.anyKeyDown) return;

		if (Input.GetKeyDown(KeyCode.A))
		{
			direction = PhotonNetwork.IsMasterClient ? Direction.Left : Direction.Right;
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			direction = Direction.Guard;
		}
		else if (Input.GetKeyDown(KeyCode.D))
		{
			direction = PhotonNetwork.IsMasterClient ? Direction.Right : Direction.Left;
		}
		else if (Input.GetKeyDown(KeyCode.W))
		{
			direction = Direction.Up;
		}

		//Skill attackType = Skill.None;

		//if (Input.GetKeyDown(KeyCode.J))
		//{
		//	attackType = Skill.Attack;

		//	if (!m_grounded)
		//	{
		//		attackType = Skill.JumpAttack;
		//	}
		//}
		//else if (Input.GetKeyDown(KeyCode.K))
		//{
		//	attackType = Skill.ChargedAttack;
			
		//	if (!m_grounded)
		//	{
		//		attackType = Skill.JumpAttack;
		//	}
		//}
		//else if (Input.GetKeyDown(KeyCode.L))
		//{
		//	attackType = Skill.LethalMove;
		//}

		//print(attackType);
	}

	public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(transform.position);
			stream.SendNext(m_procession);
			stream.SendNext(m_grounded);
			stream.SendNext(m_spriteRenderer.flipX);
			stream.SendNext(m_jump);
		}
		else if (stream.IsReading)
		{
			try
			{
				transform.position = (Vector3)stream.ReceiveNext();
				m_procession = (bool)stream.ReceiveNext();
				m_grounded = (bool)stream.ReceiveNext();
				m_spriteRenderer.flipX = (bool)stream.ReceiveNext();
				m_jump = (bool)stream.ReceiveNext();
			}
			catch (NullReferenceException) { }
		}
	}

	public virtual void Attack()
	{

	}

	public virtual void ChargedAttack()
	{

	}

	public virtual void JumpAttack()
	{
		
	}

	public virtual void LethalMove()
	{

	}
}
