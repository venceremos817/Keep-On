using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Ohira
{
	public enum E_SATELLITE_TRAIL
	{
		TRAIL_FRONT,
		TRAIL_BACK,
	}

	public enum E_SATELLITE_VFX
	{
//		LASER_CHARGE = 0,
		LASER_RELEASE,
	}

	public enum E_SATELLITE_BEAMCHARGE_VFX
	{
		CHARGE0 = 0,
		CHARGE1,
		CHARGE2,
	}







	namespace Auxiliary
	{
		/// <summary>
		/// �Ȃ񂩕֗������Ȃ��
		/// </summary>
		public static class Auxiliary
		{
			#region IsInRange
			// �ϐ���2�l�͈͓̔��ɂ��邩�ǂ���
			// <param name="self"></param>
			// <param name="min"></param>
			// <param name="max"></param>
			// <returns>true: �͈͓�	false:�͈͊O</returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRnage(this int self, int min, int max)
			{
				return min <= self && self <= max;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRnage(this float self, float min, float max)
			{
				return min <= self && self <= max;
			}
			#endregion

			#region IsLessThan
			// �ϐ���comp�ȉ���
			// <param name="self"></param>
			// <param name="comp"></param>
			// <returns>true:�ȉ�</returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsLessThan(this int self, int comp)
			{
				return self <= comp;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsLessThan(this float self, float comp)
			{
				return self <= comp;
			}
			#endregion

			#region IsMoreThan
			// float�ϐ���comp�ȏォ
			// <param name="self"></param>
			// <param name="comp"></param>
			// <returns></returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsMoreThan(this float self, float comp)
			{
				return self >= comp;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsMoreThan(this int self, int comp)
			{
				return self >= comp;
			}
			#endregion

			#region Clamp
			// �͈͊O�Ȃ�͈͂Ɏ��܂�悤�Ɋۂ߂��l��Ԃ�
			// �͈͓��Ȃ炻�̂܂܂̒l��Ԃ�
			// <param name="self"></param>
			// <param name="min"></param>
			// <param name="max"></param>
			// <returns></returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static int Clamp(this int self, int min, int max)
			{
				int ret = self;

				if (ret < min) { ret = min; }
				else if (ret > max) { ret = max; }

				return ret;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static float Clamp(this float self, float min, float max)
			{
				float ret = self;

				if (ret < min) { ret = min; }
				else if (ret > max) { ret = max; }

				return ret;
			}
			#endregion

			#region ClampSelf
			// �͈͊O�Ȃ�͈͓��Ɏ��܂�悤�ɒl���ۂ߂�
			// <param name="self"></param>
			// <param name="min"></param>
			// <param name="max"></param>
			// <returns></returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static int ClampSelf(this int self, int min, int max)
			{
				if (self < min) { self = min; }
				else if (self > max) { self = max; }

				return self;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static float ClampSelf(this float self, float min, float max)
			{
				if (self < min) { self = min; }
				else if (self > max) { self = max; }

				return self;
			}
			#endregion

			#region ApproachRotate
			/// <summary>
			/// ����������
			/// </summary>
			/// <param name="self">����</param>
			/// <param name="target">����</param>
			/// <param name="t">���(0~1)�U��������x�݂�����</param>
			/// <param name="freezX">1:����	0:�����Ȃ�</param>
			/// <param name="freezY">1:����	0:�����Ȃ�</param>
			/// <param name="freezZ">1:����	0:�����Ȃ�</param>
			/// <returns></returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Vector3 ApproachRotate(this Transform self, Transform target, float t, byte freezX = 1, byte freezY = 0, byte freezZ = 1)
			{
				Vector3 toTarget = target.position - self.position;
				toTarget.x *= freezX;
				toTarget.y *= freezY;
				toTarget.z *= freezZ;

				return Vector3.Lerp(self.forward, toTarget.normalized, t);
			}
			#endregion

			#region Probably
			// �m����true��Ԃ�
			// <param name="percent">�p�[�Z���e�[�W(0~100)</param>
			// <returns></returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool Probably(float percent)
			{
				float probabilityRate = UnityEngine.Random.value * 100.0f;

				if (percent == 100.0f && probabilityRate == percent)
				{
					return true;
				}
				else if (probabilityRate < percent)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			#endregion


			/// <summary>
			/// int�^�ϐ��̌����𒲂ׂ�
			/// </summary>
			/// <param name="self"></param>
			/// <returns>����</returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static int Length(this int self)
			{
				return (self == 0) ? 1 : ((int)Mathf.Log10(self) + 1);
			}

			/// <summary>
			/// int�^���l���ꌅ���̔z��ɂ��ĕԂ�
			/// 12345 -> 1,2,3,4,5
			/// </summary>
			/// <param name="num"></param>
			/// <returns></returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static int[] ConvertToDigitArray(this int self, int minLength = 0, int maxLength = 100)
			{
				int arrSize = self.Length();
				arrSize = arrSize.ClampSelf(minLength, maxLength);
				int[] arr = new int[arrSize];
				arr.Initialize();
				int work = self;

				for (int i = 1; i < arrSize + 1; i++)
				{
					arr[arrSize - i] = work % 10;
					work /= 10;
				}

				return arr;
			}
		}
	}
}