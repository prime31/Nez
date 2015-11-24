using System;


namespace Nez
{
	public interface ITriggerCallback
	{
		void onTriggerEnter( Collider other );
	}
}

