using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.DeferredLighting
{
	public class DeferredLightEffect : Effect
	{
		EffectPass clearGBufferPass;
		EffectPass pointLightPass;
		EffectPass spotLightPass;
		EffectPass areaLightPass;
		EffectPass directionalLightPass;
		EffectPass finalCombinePass;

		#region EffectParameter caches

		// gBuffer
		EffectParameter _clearColorParam;

		// matrices
		EffectParameter _objectToWorldParam;
		EffectParameter _worldToViewParam;
		EffectParameter _projectionParam;
		EffectParameter _screenToWorldParam;

		// common
		EffectParameter _normalMapParam;
		EffectParameter _lightPositionParam;
		EffectParameter _colorParam;
		EffectParameter _lightRadiusParam;
		EffectParameter _lightIntensityParam;

		// spot
		EffectParameter _lightDirectionParam;
		EffectParameter _coneAngleParam;

		// directional
		EffectParameter _specularIntensityParam;
		EffectParameter _specularPowerParam;
		EffectParameter _dirAreaLightDirectionParam; // shared with area light

		// final combine
		EffectParameter _ambientColorParam;
		EffectParameter _colorMapParam;
		EffectParameter _lightMapParam;

		#endregion


		public DeferredLightEffect() : base( Core.graphicsDevice, EffectResource.deferredLightBytes )
		{
			clearGBufferPass = Techniques["ClearGBuffer"].Passes[0];
			pointLightPass = Techniques["DeferredPointLight"].Passes[0];
			spotLightPass = Techniques["DeferredSpotLight"].Passes[0];
			areaLightPass = Techniques["DeferredAreaLight"].Passes[0];
			directionalLightPass = Techniques["DeferredDirectionalLight"].Passes[0];
			finalCombinePass = Techniques["FinalCombine"].Passes[0];

			cacheEffectParameters();
		}


		void cacheEffectParameters()
		{
			// gBuffer
			_clearColorParam = Parameters["_clearColor"];

			// matrices
			_objectToWorldParam = Parameters["_objectToWorld"];
			_worldToViewParam = Parameters["_worldToView"];
			_projectionParam = Parameters["_projection"];
			_screenToWorldParam = Parameters["_screenToWorld"];

			// common
			_normalMapParam = Parameters["_normalMap"];
			_lightPositionParam = Parameters["_lightPosition"];
			_colorParam = Parameters["_color"];
			_lightRadiusParam = Parameters["_lightRadius"];
			_lightIntensityParam = Parameters["_lightIntensity"];

			// spot
			_lightDirectionParam = Parameters["_lightDirection"];
			_coneAngleParam = Parameters["_coneAngle"];

			// directional
			_specularIntensityParam = Parameters["_specularIntensity"];
			_specularPowerParam = Parameters["_specularPower"];
			_dirAreaLightDirectionParam = Parameters["_dirAreaLightDirection"];

			// final combine
			_ambientColorParam = Parameters["_ambientColor"];
			_colorMapParam = Parameters["_colorMap"];
			_lightMapParam = Parameters["_lightMap"];
		}


		public void prepareClearGBuffer()
		{
			clearGBufferPass.Apply();
		}


		/// <summary>
		/// updates the camera matrixes in the Effect
		/// </summary>
		/// <param name="camera">Camera.</param>
		public void updateForCamera( Camera camera )
		{
			setWorldToViewMatrix( camera.transformMatrix );
			setProjectionMatrix( camera.projectionMatrix );
			setScreenToWorld( Matrix.Invert( camera.viewProjectionMatrix ) );
		}


		/// <summary>
		/// updates the shader values for the light and sets the appropriate CurrentTechnique
		/// </summary>
		/// <param name="light">Light.</param>
		public void updateForLight( DeferredLight light )
		{
			// check SpotLight first because it is a subclass of PointLight!
			if( light is SpotLight )
				updateForLight( light as SpotLight );
			else if( light is PointLight )
				updateForLight( light as PointLight );
			else if( light is AreaLight )
				updateForLight( light as AreaLight );
			else if( light is DirLight )
				updateForLight( light as DirLight );
		}


		/// <summary>
		/// updates the shader values for the light and sets the appropriate CurrentTechnique
		/// </summary>
		/// <param name="light">Light.</param>
		public void updateForLight( PointLight light )
		{
			setLightPosition( new Vector3( light.entity.transform.position + light.localOffset, light.zPosition ) );
			setColor( light.color );
			setLightRadius( light.radius * light.entity.transform.scale.X );
			setLightIntensity( light.intensity );

			var objToWorld = Matrix.CreateScale( light.radius * light.entity.transform.scale.X ) * Matrix.CreateTranslation( light.entity.transform.position.X + light.localOffset.X, light.entity.transform.position.Y + light.localOffset.Y, 0 );
			setObjectToWorldMatrix( objToWorld );

			pointLightPass.Apply();
		}


		/// <summary>
		/// updates the shader values for the light and sets the appropriate CurrentTechnique
		/// </summary>
		/// <param name="light">Light.</param>
		public void updateForLight( SpotLight light )
		{
			updateForLight( light as PointLight );
			setSpotLightDirection( light.direction );
			setSpotConeAngle( light.coneAngle );

			spotLightPass.Apply();
		}


		/// <summary>
		/// updates the shader values for the light and sets the appropriate CurrentTechnique
		/// </summary>
		/// <param name="light">Light.</param>
		public void updateForLight( AreaLight light )
		{
			setColor( light.color );
			setAreaDirectionalLightDirection( light.direction );
			setLightIntensity( light.intensity );

			var objToWorld = Matrix.CreateScale( light.bounds.width * light.entity.transform.scale.X, light.bounds.height * light.entity.transform.scale.Y, 1f ) * Matrix.CreateTranslation( light.bounds.x - light.bounds.width * 0.5f, light.bounds.y - light.bounds.height * 0.5f, 0 );
			setObjectToWorldMatrix( objToWorld );

			areaLightPass.Apply();
		}


		/// <summary>
		/// updates the shader values for the light and sets the appropriate CurrentTechnique
		/// </summary>
		/// <param name="light">Light.</param>
		public void updateForLight( DirLight light )
		{
			setColor( light.color );
			setAreaDirectionalLightDirection( light.direction );
			setSpecularPower( light.specularPower );
			setSpecularIntensity( light.specularIntensity );

			directionalLightPass.Apply();
		}


		#region Matrix properties

		public void setClearColor( Color color )
		{
			_clearColorParam.SetValue( color.ToVector3() );
		}


		public void setObjectToWorldMatrix( Matrix objToWorld )
		{
			_objectToWorldParam.SetValue( objToWorld );
		}


		public void setWorldToViewMatrix( Matrix worldToView )
		{
			_worldToViewParam.SetValue( worldToView );
		}


		public void setProjectionMatrix( Matrix projection )
		{
			_projectionParam.SetValue( projection );
		}


		/// <summary>
		/// inverse of Camera.getViewProjectionMatrix
		/// </summary>
		/// <param name="obj2world">Obj2world.</param>
		public void setScreenToWorld( Matrix screenToWorld )
		{
			_screenToWorldParam.SetValue( screenToWorld );
		}

		#endregion


		#region Point/Spot common properties

		public void setNormalMap( Texture2D normalMap )
		{
			_normalMapParam.SetValue( normalMap );
		}


		public void setLightPosition( Vector3 lightPosition )
		{
			_lightPositionParam.SetValue( lightPosition );
		}


		public void setColor( Color color )
		{
			_colorParam.SetValue( color.ToVector3() );
		}


		public void setLightRadius( float radius )
		{
			_lightRadiusParam.SetValue( radius );
		}


		public void setLightIntensity( float intensity )
		{
			_lightIntensityParam.SetValue( intensity );
		}

		#endregion


		#region Spot properties

		/// <summary>
		/// directly sets the light direction
		/// </summary>
		/// <param name="lightDirection">Light direction.</param>
		public void setSpotLightDirection( Vector2 lightDirection )
		{
			_lightDirectionParam.SetValue( lightDirection );
		}


		/// <summary>
		/// sets the light direction using just an angle in degrees. 0 degrees points to theright, 90 degrees would be straight down, etc
		/// </summary>
		/// <param name="degrees">Degrees.</param>
		public void setSpotLightDirection( float degrees )
		{
			var radians = MathHelper.ToRadians( degrees );
			var dir = new Vector2( (float)Math.Cos( radians ), (float)Math.Sin( radians ) );
			setSpotLightDirection( dir );
		}


		public void setSpotConeAngle( float coneAngle )
		{
			_coneAngleParam.SetValue( coneAngle );
		}

		#endregion


		#region Directional light properties

		public void setSpecularIntensity( float specIntensity )
		{
			_specularIntensityParam.SetValue( specIntensity );
		}


		public void setSpecularPower( float specPower )
		{
			_specularPowerParam.SetValue( specPower );
		}


		public void setAreaDirectionalLightDirection( Vector3 lightDir )
		{
			_dirAreaLightDirectionParam.SetValue( lightDir );
		}

		#endregion


		#region Final combine properties

		public void setAmbientColor( Color color )
		{
			_ambientColorParam.SetValue( color.ToVector3() );
		}


		/// <summary>
		/// sets the two textures required for the final combine and applies the pass
		/// </summary>
		/// <param name="diffuse">Diffuse.</param>
		/// <param name="lightMap">Light map.</param>
		public void prepareForFinalCombine( Texture2D diffuse, Texture2D lightMap, Texture2D normalMap )
		{
			_colorMapParam.SetValue( diffuse );
			_lightMapParam.SetValue( lightMap );
			_normalMapParam.SetValue( normalMap );

			finalCombinePass.Apply();
		}

		#endregion

	}
}

