using UnityEngine;

namespace MyConstants
{
	public enum FighterAction
	{
		None = 0,
		Hit = 1,
		Jump = 2,
		Guard = 3,
		Attack = 4,
		ChargedAttack = 5,
		JumpAttack = 6,
		LethalMove = 7
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