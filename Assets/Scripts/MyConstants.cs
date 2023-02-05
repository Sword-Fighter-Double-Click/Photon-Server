using UnityEngine;

namespace MyConstants
{
	public enum Action
	{
		None = 0,
		Hit = 1,
		Jump = 2,
		Guard = 4,
		Attack = 8,
		ChargedAttack = 16,
		JumpAttack = 32,
		LethalMove = 64
	};

	public class KeySetting
	{
		public readonly static KeyCode[,] keys =
		{
			{ KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.J, KeyCode.K, KeyCode.L },
			{ KeyCode.UpArrow,  KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 }
		};
	}
}