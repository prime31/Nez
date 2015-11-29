/*
The MIT License (MIT)

Copyright (c) 2015 Valerio Santinelli

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nez.Overlap2D.Runtime
{
	public class CompositeVO
	{
		public List<SimpleImageVO> sImages = new List<SimpleImageVO>(1);
		public List<Image9patchVO> sImage9patchs = new List<Image9patchVO>(1);
		public List<TextBoxVO> sTextBox = new List<TextBoxVO>(1);
		public List<LabelVO> sLabels = new List<LabelVO>(1);
		public List<CompositeItemVO> sComposites = new List<CompositeItemVO>(1);
		public List<SelectBoxVO> sSelectBoxes = new List<SelectBoxVO>(1);
		public List<ParticleEffectVO> sParticleEffects = new List<ParticleEffectVO>(1);
		public List<LightVO> sLights = new List<LightVO>(1);
		public List<SpineVO> sSpineAnimations = new List<SpineVO>(1);
		public List<SpriteAnimationVO> sSpriteAnimations = new List<SpriteAnimationVO>(1);
		public List<SpriterVO> sSpriterAnimations = new List<SpriterVO>(1);
		public List<ColorPrimitiveVO> sColorPrimitives = new List<ColorPrimitiveVO>(1);

		public List<LayerItemVO> layers = new List<LayerItemVO>();

		public CompositeVO() {

		}

		public CompositeVO(CompositeVO vo) {

			if (vo == null) return;

			update(vo);
		}

		public void update(CompositeVO vo) {
			clear();
			for (int i = 0; i < vo.sImages.Count; i++) {
				sImages.Add(new SimpleImageVO(vo.sImages[i]));
			}
			for (int i = 0; i < vo.sImage9patchs.Count; i++) {
				sImage9patchs.Add(new Image9patchVO(vo.sImage9patchs[i]));
			}
			for (int i = 0; i < vo.sTextBox.Count; i++) {
				sTextBox.Add(new TextBoxVO(vo.sTextBox[i]));
			}
			for (int i = 0; i < vo.sLabels.Count; i++) {
				sLabels.Add(new LabelVO(vo.sLabels[i]));
			}
			for (int i = 0; i < vo.sComposites.Count; i++) {
				sComposites.Add(new CompositeItemVO(vo.sComposites[i]));
			}
			for (int i = 0; i < vo.sSelectBoxes.Count; i++) {
				sSelectBoxes.Add(new SelectBoxVO(vo.sSelectBoxes[i]));
			}

			for (int i = 0; i < vo.sParticleEffects.Count; i++) {
				sParticleEffects.Add(new ParticleEffectVO(vo.sParticleEffects[i]));
			}

			for (int i = 0; i < vo.sLights.Count; i++) {
				sLights.Add(new LightVO(vo.sLights[i]));
			}

			for (int i = 0; i < vo.sSpineAnimations.Count; i++) {
				sSpineAnimations.Add(new SpineVO(vo.sSpineAnimations[i]));
			}

			for (int i = 0; i < vo.sSpriteAnimations.Count; i++) {
				sSpriteAnimations.Add(new SpriteAnimationVO(vo.sSpriteAnimations[i]));
			}

			for (int i = 0; i < vo.sSpriterAnimations.Count; i++) {
				sSpriterAnimations.Add(new SpriterVO(vo.sSpriterAnimations[i]));
			}

			for (int i = 0; i < vo.sColorPrimitives.Count; i++) {
				sColorPrimitives.Add(new ColorPrimitiveVO(vo.sColorPrimitives[i]));
			}

			layers.Clear();
			for (int i = 0; i < vo.layers.Count; i++) {
				layers.Add(new LayerItemVO(vo.layers[i]));
			}

		}

		public void addItem(MainItemVO vo) {
			Type classType = vo.GetType ();

			if (classType.Name == "SimpleImageVO") {
				sImages.Add((SimpleImageVO) vo);
			}
			if (classType.Name == "Image9patchVO") {
				sImage9patchs.Add((Image9patchVO) vo);
			}
			if (classType.Name == "TextBoxVO") {
				sTextBox.Add((TextBoxVO) vo);
			}
			if (classType.Name == "LabelVO") {
				sLabels.Add((LabelVO) vo);
			}
			if (classType.Name == "CompositeItemVO") {
				sComposites.Add((CompositeItemVO) vo);
			}
			if (classType.Name == "SelectBoxVO") {
				sSelectBoxes.Add((SelectBoxVO) vo);
			}
			if (classType.Name == "ParticleEffectVO") {
				sParticleEffects.Add((ParticleEffectVO) vo);
			}
			if (classType.Name == "LightVO") {
				sLights.Add((LightVO) vo);
			}
			if (classType.Name == "SpineVO") {
				sSpineAnimations.Add((SpineVO) vo);
			}
			if (classType.Name == "SpriterVO") {
				sSpriterAnimations.Add((SpriterVO) vo);
			}
			if (classType.Name == "SpriteAnimationVO") {
				sSpriteAnimations.Add((SpriteAnimationVO) vo);
			}
			if(classType.Name == "ColorPrimitiveVO") {
				sColorPrimitives.Add((ColorPrimitiveVO) vo);
			}
		}

		public void removeItem(MainItemVO vo) {
			Type classType = vo.GetType ();

			if (classType.Name == "SimpleImageVO") {
				sImages.Remove((SimpleImageVO) vo);
			}
			if (classType.Name == "Image9patchVO") {
				sImage9patchs.Remove((Image9patchVO) vo);
			}
			if (classType.Name == "TextBoxVO") {
				sTextBox.Remove((TextBoxVO) vo);
			}
			if (classType.Name == "LabelVO") {
				sLabels.Remove((LabelVO) vo);
			}
			if (classType.Name == "CompositeItemVO") {
				sComposites.Remove((CompositeItemVO) vo);
			}
			if (classType.Name == "SelectBoxVO") {
				sSelectBoxes.Remove((SelectBoxVO) vo);
			}
			if (classType.Name == "ParticleEffectVO") {
				sParticleEffects.Remove((ParticleEffectVO) vo);
			}
			if (classType.Name == "LightVO") {
				sLights.Remove((LightVO) vo);
			}
			if (classType.Name == "SpineVO") {
				sSpineAnimations.Remove((SpineVO) vo);
			}
			if (classType.Name == "SpriteAnimationVO") {
				sSpriteAnimations.Remove((SpriteAnimationVO) vo);
			}
			if (classType.Name == "SpriterVO") {
				sSpriterAnimations.Remove((SpriterVO) vo);
			}
			if(classType.Name == "ColorPrimitiveVO") {
				sColorPrimitives.Remove((ColorPrimitiveVO) vo);
			}
		}

		public void clear() {
			sImages.Clear();
			sTextBox.Clear();
			sLabels.Clear();
			sComposites.Clear();
			sSelectBoxes.Clear();
			sParticleEffects.Clear();
			sLights.Clear();
			sSpineAnimations.Clear();
			sSpriteAnimations.Clear();
			sSpriterAnimations.Clear();
			sColorPrimitives.Clear();
		}

		public bool isEmpty() {
			return sComposites.Count == 0 &&
				sImage9patchs.Count == 0 &&
				sImages.Count == 0 &&
				sSpriteAnimations.Count == 0 &&
				sLabels.Count == 0 &&
				sLights.Count == 0 &&
				sParticleEffects.Count == 0 &&
				sSpriteAnimations.Count == 0 &&
				sSpriterAnimations.Count == 0 &&
				sSpineAnimations.Count == 0 &&
				sSelectBoxes.Count == 0 &&
				sTextBox.Count == 0 &&
				sColorPrimitives.Count == 0;
		}

		public String[] getRecursiveParticleEffectsList() {
			HashSet<String> list = new HashSet<String>();
			foreach (ParticleEffectVO sParticleEffect in sParticleEffects) {
				list.Add(sParticleEffect.particleName);
			}
			foreach (CompositeItemVO sComposite in sComposites) {
				String[] additionalList = sComposite.composite.getRecursiveParticleEffectsList();
				foreach (var al in additionalList) {
					list.Add (al);
				}
			}
			String[] finalList = new String[list.Count];
			int i = 0;
			foreach (var l in list) {
				finalList [i] = l;
				i++;
			}

			return finalList;
		}

		public String[] getRecursiveSpineAnimationList() {
			HashSet<String> list = new HashSet<String>();
			foreach (SpineVO sSpineAnimation in sSpineAnimations) {
				list.Add(sSpineAnimation.animationName);
			}
			foreach (CompositeItemVO sComposite in sComposites) {
				String[] additionalList = sComposite.composite.getRecursiveSpineAnimationList();
				foreach (var al in additionalList) {
					list.Add (al);
				}
			}
			String[] finalList = new String[list.Count];
			int i = 0;
			foreach (var l in list) {
				finalList [i] = l;
				i++;
			}

			return finalList;
		}

		public String[] getRecursiveSpriteAnimationList() {
			HashSet<String> list = new HashSet<String>();
			foreach (SpriteAnimationVO sSpriteAnimation in sSpriteAnimations) {
				list.Add(sSpriteAnimation.animationName);
			}
			foreach (CompositeItemVO sComposite in sComposites) {
				String[] additionalList = sComposite.composite.getRecursiveSpriteAnimationList();
				foreach (var al in additionalList) {
					list.Add (al);
				}
			}
			String[] finalList = new String[list.Count];
			int i = 0;
			foreach (var l in list) {
				finalList [i] = l;
				i++;
			}

			return finalList;
		}

		public String[] getRecursiveSpriterAnimationList() {
			HashSet<String> list = new HashSet<String>();
			foreach (SpriterVO sSpriterAnimation in sSpriterAnimations) {
				list.Add(sSpriterAnimation.animationName);
			}
			foreach (CompositeItemVO sComposite in sComposites) {
				String[] additionalList = sComposite.composite.getRecursiveSpriterAnimationList();
				foreach (var al in additionalList) {
					list.Add (al);
				}
			}
			String[] finalList = new String[list.Count];
			int i = 0;
			foreach (var l in list) {
				finalList [i] = l;
				i++;
			}

			return finalList;
		}

		/*
		public FontSizePair[] getRecursiveFontList() {
			Dictionary<FontSizePair> list = new Dictionary<FontSizePair>();
			foreach (LabelVO sLabel in sLabels) {
				list.Add(new FontSizePair(sLabel.style.isEmpty() ? "arial" : sLabel.style, sLabel.size == 0 ? 12 : sLabel.size));
			}
			foreach (CompositeItemVO sComposite in sComposites) {
				FontSizePair[] additionalList = sComposite.composite.getRecursiveFontList();
				Collections.addAll(list, additionalList);
			}
			FontSizePair[] finalList = new FontSizePair[list.Count];
			list.toArray(finalList);

			return finalList;
		}
		*/

		public String[] getRecursiveShaderList() {
			HashSet<String> list = new HashSet<String>();
			foreach (MainItemVO item in getAllItems()) {
				if(item.shaderName != null && !String.IsNullOrEmpty(item.shaderName)){
					list.Add(item.shaderName);
				}
			}
			String[] finalList = new String[list.Count];
			int i = 0;
			foreach (var l in list) {
				finalList [i] = l;
				i++;
			}
			return finalList;
		}

		public List<MainItemVO> getAllItems() {
			List<MainItemVO> itemsList = new List<MainItemVO>();
			itemsList = getAllItemsRecursive(itemsList, this);

			return itemsList;
		}

		private List<MainItemVO> getAllItemsRecursive(List<MainItemVO> itemsList, CompositeVO compositeVo) {
			foreach(MainItemVO vo in compositeVo.sImage9patchs) {
				itemsList.Add(vo);
			}
			foreach(MainItemVO vo in compositeVo.sImages) {
				itemsList.Add(vo);
			}
			foreach(MainItemVO vo in compositeVo.sLabels) {
				itemsList.Add(vo);
			}
			foreach(MainItemVO vo in compositeVo.sLights) {
				itemsList.Add(vo);
			}
			foreach(MainItemVO vo in compositeVo.sParticleEffects) {
				itemsList.Add(vo);
			}
			foreach(MainItemVO vo in compositeVo.sSelectBoxes) {
				itemsList.Add(vo);
			}
			foreach(MainItemVO vo in compositeVo.sSpineAnimations) {
				itemsList.Add(vo);
			}
			foreach(MainItemVO vo in compositeVo.sSpriteAnimations) {
				itemsList.Add(vo);
			}
			foreach(MainItemVO vo in compositeVo.sSpriterAnimations) {
				itemsList.Add(vo);
			}
			foreach(MainItemVO vo in compositeVo.sTextBox) {
				itemsList.Add(vo);
			}
			foreach(MainItemVO vo in compositeVo.sColorPrimitives) {
				itemsList.Add(vo);
			}
			foreach(CompositeItemVO vo in compositeVo.sComposites) {
				itemsList = getAllItemsRecursive(itemsList,vo.composite);
				itemsList.Add(vo);
			}

			return itemsList;
		}

		/*
		public void loadFromEntity(Entity compositeEntity) {
			NodeComponent nodeComponent = compositeEntity.getComponent(NodeComponent.class);
			ComponentMapper<MainItemComponent> mainItemComponentMapper = ComponentMapper.getFor(MainItemComponent.class);
			ComponentMapper<LayerMapComponent> layerMainItemComponentComponentMapper = ComponentMapper.getFor(LayerMapComponent.class);

			if(nodeComponent == null) return;
			for(Entity entity: nodeComponent.children) {
				int entityType = mainItemComponentMapper.get(entity).entityType;
				if(entityType == EntityFactory.COMPOSITE_TYPE) {
					CompositeItemVO vo = new CompositeItemVO();
					vo.loadFromEntity(entity);
					sComposites.Add(vo);
				}
				if(entityType == EntityFactory.IMAGE_TYPE) {
					SimpleImageVO vo = new SimpleImageVO();
					vo.loadFromEntity(entity);
					sImages.Add(vo);
				}
				if(entityType == EntityFactory.NINE_PATCH) {
					Image9patchVO vo = new Image9patchVO();
					vo.loadFromEntity(entity);
					sImage9patchs.Add(vo);
				}
				if(entityType == EntityFactory.LABEL_TYPE) {
					LabelVO vo = new LabelVO();
					vo.loadFromEntity(entity);
					sLabels.Add(vo);
				}
				if(entityType == EntityFactory.PARTICLE_TYPE) {
					ParticleEffectVO vo = new ParticleEffectVO();
					vo.loadFromEntity(entity);
					sParticleEffects.Add(vo);
				}
				if(entityType == EntityFactory.SPRITE_TYPE) {
					SpriteAnimationVO vo = new SpriteAnimationVO();
					vo.loadFromEntity(entity);
					sSpriteAnimations.Add(vo);
				}
				if(entityType == EntityFactory.SPRITER_TYPE) {
					SpriterVO vo = new SpriterVO();
					vo.loadFromEntity(entity);
					sSpriterAnimations.Add(vo);
				}
				if(entityType == EntityFactory.SPINE_TYPE) {
					SpineVO vo = new SpineVO();
					vo.loadFromEntity(entity);
					sSpineAnimations.Add(vo);
				}
				if(entityType == EntityFactory.LIGHT_TYPE) {
					LightVO vo = new LightVO();
					vo.loadFromEntity(entity);
					sLights.Add(vo);
				}
				if(entityType == EntityFactory.COLOR_PRIMITIVE) {
					ColorPrimitiveVO vo = new ColorPrimitiveVO();
					vo.loadFromEntity(entity);
					sColorPrimitives.Add(vo);
				}
			}

			LayerMapComponent layerMapComponent = layerMainItemComponentComponentMapper.get(compositeEntity);
			layers = layerMapComponent.getLayers();
		}*/

}
}

