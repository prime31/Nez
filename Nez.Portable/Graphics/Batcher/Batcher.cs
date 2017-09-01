// based on the FNA SpriteBatch implementation by Ethan Lee: https://github.com/FNA-XNA/FNA
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;
using System.Runtime.CompilerServices;
using Nez.Textures;

namespace Nez
{
	public class Batcher : GraphicsResource
	{
		public Matrix transformMatrix { get { return _transformMatrix; } }

		#region variables

		// Buffer objects used for actual drawing
		DynamicVertexBuffer _vertexBuffer;
		IndexBuffer _indexBuffer;

		// Local data stored before buffering to GPU
		VertexPositionColorTexture4[] _vertexInfo;
		Texture2D[] _textureInfo;

		// Default SpriteEffect
		SpriteEffect _spriteEffect;
		EffectPass _spriteEffectPass;

		// Tracks Begin/End calls
		bool _beginCalled;

		// Keep render state for non-Immediate modes.
		BlendState _blendState;
		SamplerState _samplerState;
		DepthStencilState _depthStencilState;
		RasterizerState _rasterizerState;
		bool _disableBatching;

		// How many sprites are in the current batch?
		int _numSprites;

		// Matrix to be used when creating the projection matrix
		Matrix _transformMatrix;

		// Matrix used internally to calculate the cameras projection
		Matrix _projectionMatrix;

		// this is the calculated MatrixTransform parameter in sprite shaders
		Matrix _matrixTransformMatrix;

		// User-provided Effect, if applicable
		Effect _customEffect;

		#endregion


		#region static variables and constants

		const int MAX_SPRITES = 2048;
		const int MAX_VERTICES = MAX_SPRITES * 4;
		const int MAX_INDICES = MAX_SPRITES * 6;

		// Used to calculate texture coordinates
		static readonly float[] _cornerOffsetX = new float[] { 0.0f, 1.0f, 0.0f, 1.0f };
		static readonly float[] _cornerOffsetY = new float[] { 0.0f, 0.0f, 1.0f, 1.0f };
		static readonly short[] _indexData = generateIndexArray();

		#endregion


		public Batcher( GraphicsDevice graphicsDevice )
		{
			Assert.isTrue( graphicsDevice != null );

			this.graphicsDevice = graphicsDevice;

			_vertexInfo = new VertexPositionColorTexture4[MAX_SPRITES];
			_textureInfo = new Texture2D[MAX_SPRITES];
			_vertexBuffer = new DynamicVertexBuffer( graphicsDevice, typeof( VertexPositionColorTexture ), MAX_VERTICES, BufferUsage.WriteOnly );
			_indexBuffer = new IndexBuffer( graphicsDevice, IndexElementSize.SixteenBits, MAX_INDICES, BufferUsage.WriteOnly );
			_indexBuffer.SetData( _indexData );

			_spriteEffect = new SpriteEffect();
			_spriteEffectPass = _spriteEffect.CurrentTechnique.Passes[0];

			_projectionMatrix = new Matrix(
				0f, //(float)( 2.0 / (double)viewport.Width ) is the actual value we will use
				0.0f,
				0.0f,
				0.0f,
				0.0f,
				0f, //(float)( -2.0 / (double)viewport.Height ) is the actual value we will use
				0.0f,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
				0.0f,
				-1.0f,
				1.0f,
				0.0f,
				1.0f
			);
		}


		protected override void Dispose( bool disposing )
		{
			if( !isDisposed && disposing )
			{
				_spriteEffect.Dispose();
				_indexBuffer.Dispose();
				_vertexBuffer.Dispose();
			}
			base.Dispose( disposing );
		}


		#region Public begin/end methods

		public void begin()
		{
			begin( BlendState.AlphaBlend, Core.defaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity, false );
		}


		public void begin( Effect effect )
		{
			begin( BlendState.AlphaBlend, Core.defaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, effect, Matrix.Identity, false );
		}


		public void begin( Material material )
		{
			begin( material.blendState, material.samplerState, material.depthStencilState, RasterizerState.CullCounterClockwise, material.effect );
		}


		public void begin( Matrix transformationMatrix )
		{
			begin( BlendState.AlphaBlend, Core.defaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, transformationMatrix, false );
		}


		public void begin( BlendState blendState )
		{
			begin( blendState, Core.defaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity, false );
		}


		public void begin( Material material, Matrix transformationMatrix )
		{
			begin( material.blendState, material.samplerState, material.depthStencilState, RasterizerState.CullCounterClockwise, material.effect, transformationMatrix, false );
		}


		public void begin( BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState )
		{
			begin(
				blendState,
				samplerState,
				depthStencilState,
				rasterizerState,
				null,
				Matrix.Identity,
				false
			);
		}


		public void begin( BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect )
		{
			begin(
				blendState,
				samplerState,
				depthStencilState,
				rasterizerState,
				effect,
				Matrix.Identity,
				false
			);
		}


		public void begin( BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState,
			Effect effect, Matrix transformationMatrix )
		{
			begin(
				blendState,
				samplerState,
				depthStencilState,
				rasterizerState,
				effect,
				transformationMatrix,
				false
			);
		}


		public void begin( BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState,
			Effect effect, Matrix transformationMatrix, bool disableBatching )
		{
			Assert.isFalse( _beginCalled, "Begin has been called before calling End after the last call to Begin. Begin cannot be called again until End has been successfully called." );
			_beginCalled = true;

			_blendState = blendState ?? BlendState.AlphaBlend;
			_samplerState = samplerState ?? Core.defaultSamplerState;
			_depthStencilState = depthStencilState ?? DepthStencilState.None;
			_rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;

			_customEffect = effect;
			_transformMatrix = transformationMatrix;
			_disableBatching = disableBatching;

			if( _disableBatching )
				prepRenderState();
		}


		public void end()
		{
			Assert.isTrue( _beginCalled, "End was called, but Begin has not yet been called. You must call Begin successfully before you can call End." );
			_beginCalled = false;

			if( !_disableBatching )
				flushBatch();

			_customEffect = null;
		}

		#endregion


		#region Public draw methods

		public void draw( Texture2D texture, Vector2 position )
		{
			checkBegin();
			pushSprite( texture, null, position.X, position.Y, 1.0f, 1.0f,
				Color.White, Vector2.Zero, 0.0f, 0.0f, 0, false, 0, 0, 0, 0 );
		}


		public void draw( Texture2D texture, Vector2 position, Color color )
		{
			checkBegin();
			pushSprite( texture, null, position.X, position.Y, 1.0f, 1.0f,
				color, Vector2.Zero, 0.0f, 0.0f, 0, false, 0, 0, 0, 0 );
		}


		public void draw( Texture2D texture, Rectangle destinationRectangle )
		{
			checkBegin();
			pushSprite( texture, null, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height,
				Color.White, Vector2.Zero, 0.0f, 0.0f, 0, true, 0, 0, 0, 0 );
		}


		public void draw( Texture2D texture, Rectangle destinationRectangle, Color color )
		{
			checkBegin();
			pushSprite( texture, null, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height,
				color, Vector2.Zero, 0.0f, 0.0f, 0, true, 0, 0, 0, 0 );
		}


		public void draw( Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color )
		{
			checkBegin();
			pushSprite( texture, sourceRectangle, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height,
				color, Vector2.Zero, 0.0f, 0.0f, 0, true, 0, 0, 0, 0 );
		}

		public void draw( Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, SpriteEffects effects )
		{
			checkBegin();
			pushSprite( texture, sourceRectangle, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height,
				color, Vector2.Zero, 0.0f, 0.0f, (byte)( effects & (SpriteEffects)0x03 ), true, 0, 0, 0, 0 );
		}


		public void draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			SpriteEffects effects,
			float layerDepth,
			float skewTopX, float skewBottomX, float skewLeftY, float skewRightY
		)
		{
			checkBegin();
			pushSprite(
				texture,
				sourceRectangle,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				Vector2.Zero,
				rotation,
				layerDepth,
				(byte)( effects & (SpriteEffects)0x03 ),
				true,
				skewTopX, skewBottomX, skewLeftY, skewRightY
			);
		}


		public void draw( Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color )
		{
			checkBegin();
			pushSprite(
				texture,
				sourceRectangle,
				position.X,
				position.Y,
				1.0f,
				1.0f,
				color,
				Vector2.Zero,
				0.0f,
				0.0f,
				0,
				false,
				0, 0, 0, 0
			);
		}


		public void draw(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float layerDepth
		)
		{
			checkBegin();
			pushSprite(
				texture,
				sourceRectangle,
				position.X,
				position.Y,
				scale,
				scale,
				color,
				origin,
				rotation,
				layerDepth,
				(byte)( effects & (SpriteEffects)0x03 ),
				false,
				0, 0, 0, 0
			);
		}


		public void draw(
			Subtexture subtexture,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float layerDepth
		)
		{
			checkBegin();
			pushSprite(
				subtexture,
				position.X,
				position.Y,
				scale,
				scale,
				color,
				origin,
				rotation,
				layerDepth,
				(byte)( effects & (SpriteEffects)0x03 ),
				0, 0, 0, 0
			);
		}


		public void draw(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float layerDepth
		)
		{
			checkBegin();
			pushSprite(
				texture,
				sourceRectangle,
				position.X,
				position.Y,
				scale.X,
				scale.Y,
				color,
				origin,
				rotation,
				layerDepth,
				(byte)( effects & (SpriteEffects)0x03 ),
				false,
				0, 0, 0, 0
			);
		}


		public void draw(
			Subtexture subtexture,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float layerDepth
		)
		{
			checkBegin();
			pushSprite(
				subtexture,
				position.X,
				position.Y,
				scale.X,
				scale.Y,
				color,
				origin,
				rotation,
				layerDepth,
				(byte)( effects & (SpriteEffects)0x03 ),
				0, 0, 0, 0
			);
		}


		public void draw(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float layerDepth,
			float skewTopX, float skewBottomX, float skewLeftY, float skewRightY
		)
		{
			checkBegin();
			pushSprite(
				texture,
				sourceRectangle,
				position.X,
				position.Y,
				scale.X,
				scale.Y,
				color,
				origin,
				rotation,
				layerDepth,
				(byte)( effects & (SpriteEffects)0x03 ),
				false,
				skewTopX, skewBottomX, skewLeftY, skewRightY
			);
		}


		public void draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effects,
			float layerDepth
		)
		{
			checkBegin();
			pushSprite(
				texture,
				sourceRectangle,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				origin,
				rotation,
				layerDepth,
				(byte)( effects & (SpriteEffects)0x03 ),
				true,
				0, 0, 0, 0
			);
		}


		/// <summary>
		/// direct access to setting vert positions, UVs and colors. The order of elements is top-left, top-right, bottom-left, bottom-right
		/// </summary>
		/// <returns>The raw.</returns>
		/// <param name="texture">Texture.</param>
		/// <param name="verts">Verts.</param>
		/// <param name="textureCoords">Texture coords.</param>
		/// <param name="colors">Colors.</param>
		public void drawRaw( Texture2D texture, Vector3[] verts, Vector2[] textureCoords, Color[] colors )
		{
			Assert.isTrue( verts.Length == 4, "there must be only 4 verts" );
			Assert.isTrue( textureCoords.Length == 4, "there must be only 4 texture coordinates" );
			Assert.isTrue( colors.Length == 4, "there must be only 4 colors" );

			// we're out of space, flush
			if( _numSprites >= MAX_SPRITES )
				flushBatch();

			_vertexInfo[_numSprites].position0 = verts[0];
			_vertexInfo[_numSprites].position1 = verts[1];
			_vertexInfo[_numSprites].position2 = verts[2];
			_vertexInfo[_numSprites].position3 = verts[3];

			_vertexInfo[_numSprites].textureCoordinate0 = textureCoords[0];
			_vertexInfo[_numSprites].textureCoordinate1 = textureCoords[1];
			_vertexInfo[_numSprites].textureCoordinate2 = textureCoords[2];
			_vertexInfo[_numSprites].textureCoordinate3 = textureCoords[3];

			_vertexInfo[_numSprites].color0 = colors[0];
			_vertexInfo[_numSprites].color1 = colors[1];
			_vertexInfo[_numSprites].color2 = colors[2];
			_vertexInfo[_numSprites].color3 = colors[3];

			if( _disableBatching )
			{
				_vertexBuffer.SetData( 0, _vertexInfo, 0, 1, VertexPositionColorTexture4.realStride, SetDataOptions.None );
				drawPrimitives( texture, 0, 1 );
			}
			else
			{
				_textureInfo[_numSprites] = texture;
				_numSprites += 1;
			}
		}


		/// <summary>
		/// direct access to setting vert positions, UVs and colors. The order of elements is top-left, top-right, bottom-left, bottom-right
		/// </summary>
		/// <returns>The raw.</returns>
		/// <param name="texture">Texture.</param>
		/// <param name="verts">Verts.</param>
		/// <param name="textureCoords">Texture coords.</param>
		/// <param name="color">Color.</param>
		public void drawRaw( Texture2D texture, Vector3[] verts, Vector2[] textureCoords, Color color )
		{
			Assert.isTrue( verts.Length == 4, "there must be only 4 verts" );
			Assert.isTrue( textureCoords.Length == 4, "there must be only 4 texture coordinates" );

			// we're out of space, flush
			if( _numSprites >= MAX_SPRITES )
				flushBatch();

			_vertexInfo[_numSprites].position0 = verts[0];
			_vertexInfo[_numSprites].position1 = verts[1];
			_vertexInfo[_numSprites].position2 = verts[2];
			_vertexInfo[_numSprites].position3 = verts[3];

			_vertexInfo[_numSprites].textureCoordinate0 = textureCoords[0];
			_vertexInfo[_numSprites].textureCoordinate1 = textureCoords[1];
			_vertexInfo[_numSprites].textureCoordinate2 = textureCoords[2];
			_vertexInfo[_numSprites].textureCoordinate3 = textureCoords[3];

			_vertexInfo[_numSprites].color0 = color;
			_vertexInfo[_numSprites].color1 = color;
			_vertexInfo[_numSprites].color2 = color;
			_vertexInfo[_numSprites].color3 = color;

			if( _disableBatching )
			{
				_vertexBuffer.SetData( 0, _vertexInfo, 0, 1, VertexPositionColorTexture4.realStride, SetDataOptions.None );
				drawPrimitives( texture, 0, 1 );
			}
			else
			{
				_textureInfo[_numSprites] = texture;
				_numSprites += 1;
			}
		}

		#endregion


		[System.Obsolete( "SpriteFont is too locked down to use directly. Wrap it in a NezSpriteFont" )]
		public void DrawString( SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation,
			Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth )
		{
			throw new NotImplementedException( "SpriteFont is too locked down to use directly. Wrap it in a NezSpriteFont" );
		}


		static short[] generateIndexArray()
		{
			var result = new short[MAX_INDICES];
			for( int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4 )
			{
				result[i] = (short)( j );
				result[i + 1] = (short)( j + 1 );
				result[i + 2] = (short)( j + 2 );
				result[i + 3] = (short)( j + 3 );
				result[i + 4] = (short)( j + 2 );
				result[i + 5] = (short)( j + 1 );
			}
			return result;
		}


		#region Methods

		/// <summary>
		/// the meat of the Batcher. This is where it all goes down
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void pushSprite( Texture2D texture, Rectangle? sourceRectangle, float destinationX, float destinationY, float destinationW, float destinationH, Color color, Vector2 origin,
						float rotation, float depth, byte effects, bool destSizeInPixels, float skewTopX, float skewBottomX, float skewLeftY, float skewRightY )
		{
			// out of space, flush
			if( _numSprites >= MAX_SPRITES )
				flushBatch();

			// Source/Destination/Origin Calculations
			float sourceX, sourceY, sourceW, sourceH;
			float originX, originY;
			if( sourceRectangle.HasValue )
			{
				var inverseTexW = 1.0f / (float)texture.Width;
				var inverseTexH = 1.0f / (float)texture.Height;

				sourceX = sourceRectangle.Value.X * inverseTexW;
				sourceY = sourceRectangle.Value.Y * inverseTexH;
				sourceW = sourceRectangle.Value.Width * inverseTexW;
				sourceH = sourceRectangle.Value.Height * inverseTexH;

				originX = ( origin.X / sourceW ) * inverseTexW;
				originY = ( origin.Y / sourceH ) * inverseTexH;

				if( !destSizeInPixels )
				{
					destinationW *= sourceRectangle.Value.Width;
					destinationH *= sourceRectangle.Value.Height;
				}
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;

				originX = origin.X * ( 1.0f / texture.Width );
				originY = origin.Y * ( 1.0f / texture.Height );

				if( !destSizeInPixels )
				{
					destinationW *= texture.Width;
					destinationH *= texture.Height;
				}
			}

			// Rotation Calculations
			float rotationMatrix1X;
			float rotationMatrix1Y;
			float rotationMatrix2X;
			float rotationMatrix2Y;
			if( !Mathf.withinEpsilon( rotation, 0 ) )
			{
				var sin = Mathf.sin( rotation );
				var cos = Mathf.cos( rotation );
				rotationMatrix1X = cos;
				rotationMatrix1Y = sin;
				rotationMatrix2X = -sin;
				rotationMatrix2Y = cos;
			}
			else
			{
				rotationMatrix1X = 1.0f;
				rotationMatrix1Y = 0.0f;
				rotationMatrix2X = 0.0f;
				rotationMatrix2Y = 1.0f;
			}


			// flip our skew values if we have a flipped sprite
			if( effects != 0 )
			{
				skewTopX *= -1;
				skewBottomX *= -1;
				skewLeftY *= -1;
				skewRightY *= -1;
			}

			// calculate vertices
			// top-left
			var cornerX = ( _cornerOffsetX[0] - originX ) * destinationW + skewTopX;
			var cornerY = ( _cornerOffsetY[0] - originY ) * destinationH - skewLeftY;
			_vertexInfo[_numSprites].position0.X = (
				( rotationMatrix2X * cornerY ) +
				( rotationMatrix1X * cornerX ) +
				destinationX
			);
			_vertexInfo[_numSprites].position0.Y = (
				( rotationMatrix2Y * cornerY ) +
				( rotationMatrix1Y * cornerX ) +
				destinationY
			);

			// top-right
			cornerX = ( _cornerOffsetX[1] - originX ) * destinationW + skewTopX;
			cornerY = ( _cornerOffsetY[1] - originY ) * destinationH - skewRightY;
			_vertexInfo[_numSprites].position1.X = (
				( rotationMatrix2X * cornerY ) +
				( rotationMatrix1X * cornerX ) +
				destinationX
			);
			_vertexInfo[_numSprites].position1.Y = (
				( rotationMatrix2Y * cornerY ) +
				( rotationMatrix1Y * cornerX ) +
				destinationY
			);

			// bottom-left
			cornerX = ( _cornerOffsetX[2] - originX ) * destinationW + skewBottomX;
			cornerY = ( _cornerOffsetY[2] - originY ) * destinationH - skewLeftY;
			_vertexInfo[_numSprites].position2.X = (
				( rotationMatrix2X * cornerY ) +
				( rotationMatrix1X * cornerX ) +
				destinationX
			);
			_vertexInfo[_numSprites].position2.Y = (
				( rotationMatrix2Y * cornerY ) +
				( rotationMatrix1Y * cornerX ) +
				destinationY
			);

			// bottom-right
			cornerX = ( _cornerOffsetX[3] - originX ) * destinationW + skewBottomX;
			cornerY = ( _cornerOffsetY[3] - originY ) * destinationH - skewRightY;
			_vertexInfo[_numSprites].position3.X = (
				( rotationMatrix2X * cornerY ) +
				( rotationMatrix1X * cornerX ) +
				destinationX
			);
			_vertexInfo[_numSprites].position3.Y = (
				( rotationMatrix2Y * cornerY ) +
				( rotationMatrix1Y * cornerX ) +
				destinationY
			);

			_vertexInfo[_numSprites].textureCoordinate0.X = ( _cornerOffsetX[0 ^ effects] * sourceW ) + sourceX;
			_vertexInfo[_numSprites].textureCoordinate0.Y = ( _cornerOffsetY[0 ^ effects] * sourceH ) + sourceY;
			_vertexInfo[_numSprites].textureCoordinate1.X = ( _cornerOffsetX[1 ^ effects] * sourceW ) + sourceX;
			_vertexInfo[_numSprites].textureCoordinate1.Y = ( _cornerOffsetY[1 ^ effects] * sourceH ) + sourceY;
			_vertexInfo[_numSprites].textureCoordinate2.X = ( _cornerOffsetX[2 ^ effects] * sourceW ) + sourceX;
			_vertexInfo[_numSprites].textureCoordinate2.Y = ( _cornerOffsetY[2 ^ effects] * sourceH ) + sourceY;
			_vertexInfo[_numSprites].textureCoordinate3.X = ( _cornerOffsetX[3 ^ effects] * sourceW ) + sourceX;
			_vertexInfo[_numSprites].textureCoordinate3.Y = ( _cornerOffsetY[3 ^ effects] * sourceH ) + sourceY;
			_vertexInfo[_numSprites].position0.Z = depth;
			_vertexInfo[_numSprites].position1.Z = depth;
			_vertexInfo[_numSprites].position2.Z = depth;
			_vertexInfo[_numSprites].position3.Z = depth;
			_vertexInfo[_numSprites].color0 = color;
			_vertexInfo[_numSprites].color1 = color;
			_vertexInfo[_numSprites].color2 = color;
			_vertexInfo[_numSprites].color3 = color;

			if( _disableBatching )
			{
				_vertexBuffer.SetData( 0, _vertexInfo, 0, 1, VertexPositionColorTexture4.realStride, SetDataOptions.None );
				drawPrimitives( texture, 0, 1 );
			}
			else
			{
				_textureInfo[_numSprites] = texture;
				_numSprites += 1;
			}
		}


		/// <summary>
		/// Subtexture alternative to the old SpriteBatch pushSprite
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void pushSprite( Subtexture subtexture, float destinationX, float destinationY, float destinationW, float destinationH, Color color, Vector2 origin,
				float rotation, float depth, byte effects, float skewTopX, float skewBottomX, float skewLeftY, float skewRightY )
		{
			// out of space, flush
			if( _numSprites >= MAX_SPRITES )
				flushBatch();

			// Source/Destination/Origin Calculations. destinationW/H is the scale value so we multiply by the size of the texture region
			var originX = ( origin.X / subtexture.uvs.width ) / subtexture.texture2D.Width;
			var originY = ( origin.Y / subtexture.uvs.height ) / subtexture.texture2D.Height;
			destinationW *= subtexture.sourceRect.Width;
			destinationH *= subtexture.sourceRect.Height;

			// Rotation Calculations
			float rotationMatrix1X;
			float rotationMatrix1Y;
			float rotationMatrix2X;
			float rotationMatrix2Y;
			if( !Mathf.withinEpsilon( rotation, 0 ) )
			{
				var sin = Mathf.sin( rotation );
				var cos = Mathf.cos( rotation );
				rotationMatrix1X = cos;
				rotationMatrix1Y = sin;
				rotationMatrix2X = -sin;
				rotationMatrix2Y = cos;
			}
			else
			{
				rotationMatrix1X = 1.0f;
				rotationMatrix1Y = 0.0f;
				rotationMatrix2X = 0.0f;
				rotationMatrix2Y = 1.0f;
			}


			// flip our skew values if we have a flipped sprite
			if( effects != 0 )
			{
				skewTopX *= -1;
				skewBottomX *= -1;
				skewLeftY *= -1;
				skewRightY *= -1;
			}

			// calculate vertices
			// top-left
			var cornerX = ( _cornerOffsetX[0] - originX ) * destinationW + skewTopX;
			var cornerY = ( _cornerOffsetY[0] - originY ) * destinationH - skewLeftY;
			_vertexInfo[_numSprites].position0.X = (
				( rotationMatrix2X * cornerY ) +
				( rotationMatrix1X * cornerX ) +
				destinationX
			);
			_vertexInfo[_numSprites].position0.Y = (
				( rotationMatrix2Y * cornerY ) +
				( rotationMatrix1Y * cornerX ) +
				destinationY
			);

			// top-right
			cornerX = ( _cornerOffsetX[1] - originX ) * destinationW + skewTopX;
			cornerY = ( _cornerOffsetY[1] - originY ) * destinationH - skewRightY;
			_vertexInfo[_numSprites].position1.X = (
				( rotationMatrix2X * cornerY ) +
				( rotationMatrix1X * cornerX ) +
				destinationX
			);
			_vertexInfo[_numSprites].position1.Y = (
				( rotationMatrix2Y * cornerY ) +
				( rotationMatrix1Y * cornerX ) +
				destinationY
			);

			// bottom-left
			cornerX = ( _cornerOffsetX[2] - originX ) * destinationW + skewBottomX;
			cornerY = ( _cornerOffsetY[2] - originY ) * destinationH - skewLeftY;
			_vertexInfo[_numSprites].position2.X = (
				( rotationMatrix2X * cornerY ) +
				( rotationMatrix1X * cornerX ) +
				destinationX
			);
			_vertexInfo[_numSprites].position2.Y = (
				( rotationMatrix2Y * cornerY ) +
				( rotationMatrix1Y * cornerX ) +
				destinationY
			);

			// bottom-right
			cornerX = ( _cornerOffsetX[3] - originX ) * destinationW + skewBottomX;
			cornerY = ( _cornerOffsetY[3] - originY ) * destinationH - skewRightY;
			_vertexInfo[_numSprites].position3.X = (
				( rotationMatrix2X * cornerY ) +
				( rotationMatrix1X * cornerX ) +
				destinationX
			);
			_vertexInfo[_numSprites].position3.Y = (
				( rotationMatrix2Y * cornerY ) +
				( rotationMatrix1Y * cornerX ) +
				destinationY
			);

			_vertexInfo[_numSprites].textureCoordinate0.X = ( _cornerOffsetX[0 ^ effects] * subtexture.uvs.width ) + subtexture.uvs.x;
			_vertexInfo[_numSprites].textureCoordinate0.Y = ( _cornerOffsetY[0 ^ effects] * subtexture.uvs.height ) + subtexture.uvs.y;
			_vertexInfo[_numSprites].textureCoordinate1.X = ( _cornerOffsetX[1 ^ effects] * subtexture.uvs.width ) + subtexture.uvs.x;
			_vertexInfo[_numSprites].textureCoordinate1.Y = ( _cornerOffsetY[1 ^ effects] * subtexture.uvs.height ) + subtexture.uvs.y;
			_vertexInfo[_numSprites].textureCoordinate2.X = ( _cornerOffsetX[2 ^ effects] * subtexture.uvs.width ) + subtexture.uvs.x;
			_vertexInfo[_numSprites].textureCoordinate2.Y = ( _cornerOffsetY[2 ^ effects] * subtexture.uvs.height ) + subtexture.uvs.y;
			_vertexInfo[_numSprites].textureCoordinate3.X = ( _cornerOffsetX[3 ^ effects] * subtexture.uvs.width ) + subtexture.uvs.x;
			_vertexInfo[_numSprites].textureCoordinate3.Y = ( _cornerOffsetY[3 ^ effects] * subtexture.uvs.height ) + subtexture.uvs.y;
			_vertexInfo[_numSprites].position0.Z = depth;
			_vertexInfo[_numSprites].position1.Z = depth;
			_vertexInfo[_numSprites].position2.Z = depth;
			_vertexInfo[_numSprites].position3.Z = depth;
			_vertexInfo[_numSprites].color0 = color;
			_vertexInfo[_numSprites].color1 = color;
			_vertexInfo[_numSprites].color2 = color;
			_vertexInfo[_numSprites].color3 = color;

			if( _disableBatching )
			{
				_vertexBuffer.SetData( 0, _vertexInfo, 0, 1, VertexPositionColorTexture4.realStride, SetDataOptions.None );
				drawPrimitives( subtexture, 0, 1 );
			}
			else
			{
				_textureInfo[_numSprites] = subtexture;
				_numSprites += 1;
			}
		}


		public void flushBatch()
		{
			if( _numSprites == 0 )
				return;

			var offset = 0;
			Texture2D curTexture = null;

			prepRenderState();

			_vertexBuffer.SetData( 0, _vertexInfo, 0, _numSprites, VertexPositionColorTexture4.realStride, SetDataOptions.None );

			curTexture = _textureInfo[0];
			for( var i = 1; i < _numSprites; i += 1 )
			{
				if( _textureInfo[i] != curTexture )
				{
					drawPrimitives( curTexture, offset, i - offset );
					curTexture = _textureInfo[i];
					offset = i;
				}
			}
			drawPrimitives( curTexture, offset, _numSprites - offset );

			_numSprites = 0;
		}


		/// <summary>
		/// enables/disables scissor testing. If the RasterizerState changes it will cause a batch flush.
		/// </summary>
		/// <returns>The scissor test.</returns>
		/// <param name="shouldEnable">Should enable.</param>
		public void enableScissorTest( bool shouldEnable )
		{
			var currentValue = _rasterizerState.ScissorTestEnable;
			if( currentValue == shouldEnable )
				return;

			flushBatch();

			_rasterizerState = new RasterizerState
			{
				CullMode = _rasterizerState.CullMode,
				DepthBias = _rasterizerState.DepthBias,
				FillMode = _rasterizerState.FillMode,
				MultiSampleAntiAlias = _rasterizerState.MultiSampleAntiAlias,
				SlopeScaleDepthBias = _rasterizerState.SlopeScaleDepthBias,
				ScissorTestEnable = shouldEnable
			};
		}


		void prepRenderState()
		{
			graphicsDevice.BlendState = _blendState;
			graphicsDevice.SamplerStates[0] = _samplerState;
			graphicsDevice.DepthStencilState = _depthStencilState;
			graphicsDevice.RasterizerState = _rasterizerState;

			graphicsDevice.SetVertexBuffer( _vertexBuffer );
			graphicsDevice.Indices = _indexBuffer;

			var viewport = graphicsDevice.Viewport;

			// inlined CreateOrthographicOffCenter
#if FNA
			_projectionMatrix.M11 = (float)( 2.0 / (double) ( viewport.Width / 2 * 2 - 1 ) );
			_projectionMatrix.M22 = (float)( -2.0 / (double) ( viewport.Height / 2 * 2 - 1 ) );
#else
			_projectionMatrix.M11 = (float)( 2.0 / (double)viewport.Width );
			_projectionMatrix.M22 = (float)( -2.0 / (double)viewport.Height );
#endif

			_projectionMatrix.M41 = -1 - 0.5f * _projectionMatrix.M11;
			_projectionMatrix.M42 = 1 - 0.5f * _projectionMatrix.M22;

			Matrix.Multiply( ref _transformMatrix, ref _projectionMatrix, out _matrixTransformMatrix );
			_spriteEffect.setMatrixTransform( ref _matrixTransformMatrix );

			// we have to Apply here because custom effects often wont have a vertex shader and we need the default SpriteEffect's
			_spriteEffectPass.Apply();
		}


		void drawPrimitives( Texture texture, int baseSprite, int batchSize )
		{
			if( _customEffect != null )
			{
				foreach( var pass in _customEffect.CurrentTechnique.Passes )
				{
					pass.Apply();

					// Whatever happens in pass.Apply, make sure the texture being drawn ends up in Textures[0].
					graphicsDevice.Textures[0] = texture;
					graphicsDevice.DrawIndexedPrimitives( PrimitiveType.TriangleList, baseSprite * 4, 0, batchSize * 2 );
				}
			}
			else
			{
				graphicsDevice.Textures[0] = texture;
				graphicsDevice.DrawIndexedPrimitives( PrimitiveType.TriangleList, baseSprite * 4, 0, batchSize * 2 );
			}
		}


		[System.Diagnostics.Conditional( "DEBUG" )]
		void checkBegin()
		{
			if( !_beginCalled )
				throw new InvalidOperationException( "Begin has not been called. Begin must be called before you can draw" );
		}

		#endregion


		#region Sprite Data Container Class

		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		struct VertexPositionColorTexture4 : IVertexType
		{
			public const int realStride = 96;

			VertexDeclaration IVertexType.VertexDeclaration { get { throw new NotImplementedException(); } }

			public Vector3 position0;
			public Color color0;
			public Vector2 textureCoordinate0;
			public Vector3 position1;
			public Color color1;
			public Vector2 textureCoordinate1;
			public Vector3 position2;
			public Color color2;
			public Vector2 textureCoordinate2;
			public Vector3 position3;
			public Color color3;
			public Vector2 textureCoordinate3;
		}

		#endregion

	}
}
