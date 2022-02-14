using UnityEngine;


namespace Ohira
{
	public partial class Satellite
	{
		public class SatelliteFall : SatelliteStateBase
		{
			public override void OnEnter(Satellite owner, SatelliteStateBase prevState = null)
			{
				owner.ChangeStyle(owner.startStyle);
				owner.rb.velocity = Vector3.zero;
				owner.rb.angularVelocity = Vector3.zero;
				owner.rb.useGravity = true;
				owner.rb.constraints = RigidbodyConstraints.None;
				owner.rb.constraints = RigidbodyConstraints.FreezePositionX;
				owner.rb.constraints = RigidbodyConstraints.FreezePositionZ;
				owner.colliderr.enabled = true;
				owner.bodyCol.isTrigger = false;
			}
		}
	}
}