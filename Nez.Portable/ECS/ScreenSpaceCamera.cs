namespace Nez
{
	public class ScreenSpaceCamera : Camera
	{
		/// <summary>
		/// we are screen space, so our matrixes should always be identity
		/// </summary>
		protected override void UpdateMatrixes()
		{
		}
	}
}