namespace Nez
{
	public class PaletteCyclerMaterial : Material<PaletteCyclerEffect>
	{
		public PaletteCyclerMaterial()
		{
			effect = new PaletteCyclerEffect();
		}

		public override void onPreRender( Camera camera )
		{
			effect.updateTime();
		}

	}
}

