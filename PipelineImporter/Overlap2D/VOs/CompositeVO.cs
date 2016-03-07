using System;
using System.Collections;
using System.Collections.Generic;


namespace Nez.Overlap2D.Runtime
{
	public class CompositeVO
	{
		public List<SimpleImageVO> sImages = new List<SimpleImageVO>();
		public List<CompositeItemVO> sComposites = new List<CompositeItemVO>();

		public List<Image9patchVO> sImage9patchs;
		public List<TextBoxVO> sTextBox;
		public List<LabelVO> sLabels;
		public List<SelectBoxVO> sSelectBoxes;
		public List<ParticleEffectVO> sParticleEffects;
		public List<LightVO> sLights;
		public List<SpriteAnimationVO> sSpriteAnimations;
		public List<SpineVO> sSpineAnimations;
		public List<SpriterVO> sSpriterAnimations;
		public List<ColorPrimitiveVO> sColorPrimitives;
		public List<LayerItemVO> layers;
	}
}

