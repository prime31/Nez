using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Common
{
	/// <summary>
	/// Serialize the world into an XML file
	/// </summary>
	public static class WorldSerializer
	{
		/// <summary>
		/// Serialize the world to an XML file
		/// </summary>
		/// <param name="world"></param>
		/// <param name="fileStream"></param>
		public static void serialize( World world, Stream fileStream )
		{
			WorldXmlSerializer.Serialize( world, fileStream );
		}

		/// <summary>
		/// Deserialize the world from an XML file
		/// </summary>
		/// <param name="fileStream"></param>
		public static World deserialize( Stream fileStream )
		{
			return WorldXmlDeserializer.Deserialize( fileStream );
		}
	
	}


	internal static class WorldXmlSerializer
	{
		static XmlWriter _writer;

		static void SerializeShape( Shape shape )
		{
			_writer.WriteStartElement( "Shape" );
			_writer.WriteAttributeString( "Type", shape.shapeType.ToString() );
			_writer.WriteAttributeString( "Density", shape.density.ToString() );

			switch( shape.shapeType )
			{
				case ShapeType.Circle:
					{
						var circle = (CircleShape)shape;
						_writer.WriteElementString( "Radius", circle.radius.ToString() );
						WriteElement( "Position", circle.position );
					}
					break;
				case ShapeType.Polygon:
					{
						var poly = (PolygonShape)shape;

						_writer.WriteStartElement( "Vertices" );
						foreach( Vector2 v in poly.vertices )
							WriteElement( "Vertex", v );
						_writer.WriteEndElement();

						WriteElement( "Centroid", poly.massData.centroid );
					}
					break;
				case ShapeType.Edge:
					{
						var poly = (EdgeShape)shape;
						WriteElement( "Vertex1", poly.vertex1 );
						WriteElement( "Vertex2", poly.vertex2 );
					}
					break;
				case ShapeType.Chain:
					{
						var chain = (ChainShape)shape;

						_writer.WriteStartElement( "Vertices" );
						foreach( Vector2 v in chain.vertices )
							WriteElement( "Vertex", v );
						_writer.WriteEndElement();

						WriteElement( "NextVertex", chain.nextVertex );
						WriteElement( "PrevVertex", chain.prevVertex );
					}
					break;
				default:
					throw new Exception();
			}

			_writer.WriteEndElement();
		}

		static void SerializeFixture( Fixture fixture )
		{
			_writer.WriteStartElement( "Fixture" );
			_writer.WriteAttributeString( "Id", fixture.fixtureId.ToString() );

			_writer.WriteStartElement( "FilterData" );
			_writer.WriteElementString( "CategoryBits", ( (int)fixture.collisionCategories ).ToString() );
			_writer.WriteElementString( "MaskBits", ( (int)fixture.collidesWith ).ToString() );
			_writer.WriteElementString( "GroupIndex", fixture.collisionGroup.ToString() );
			_writer.WriteElementString( "CollisionIgnores", Join( "|", fixture._collisionIgnores ) );
			_writer.WriteEndElement();

			_writer.WriteElementString( "Friction", fixture.friction.ToString() );
			_writer.WriteElementString( "IsSensor", fixture.isSensor.ToString() );
			_writer.WriteElementString( "Restitution", fixture.restitution.ToString() );

			if( fixture.userData != null )
			{
				_writer.WriteStartElement( "UserData" );
				WriteDynamicType( fixture.userData.GetType(), fixture.userData );
				_writer.WriteEndElement();
			}

			_writer.WriteEndElement();
		}

		static void SerializeBody( List<Fixture> fixtures, List<Shape> shapes, Body body )
		{
			_writer.WriteStartElement( "Body" );
			_writer.WriteAttributeString( "Type", body.bodyType.ToString() );
			_writer.WriteElementString( "Active", body.enabled.ToString() );
			_writer.WriteElementString( "AllowSleep", body.isSleepingAllowed.ToString() );
			_writer.WriteElementString( "Angle", body.rotation.ToString() );
			_writer.WriteElementString( "AngularDamping", body.angularDamping.ToString() );
			_writer.WriteElementString( "AngularVelocity", body.angularVelocity.ToString() );
			_writer.WriteElementString( "Awake", body.isAwake.ToString() );
			_writer.WriteElementString( "Bullet", body.isBullet.ToString() );
			_writer.WriteElementString( "FixedRotation", body.fixedRotation.ToString() );
			_writer.WriteElementString( "LinearDamping", body.linearDamping.ToString() );
			WriteElement( "LinearVelocity", body.linearVelocity );
			WriteElement( "Position", body.position );

			if( body.userData != null )
			{
				_writer.WriteStartElement( "UserData" );
				WriteDynamicType( body.userData.GetType(), body.userData );
				_writer.WriteEndElement();
			}

			_writer.WriteStartElement( "Bindings" );
			for( int i = 0; i < body.fixtureList.Count; i++ )
			{
				_writer.WriteStartElement( "Pair" );
				_writer.WriteAttributeString( "FixtureId", FindIndex( fixtures, body.fixtureList[i] ).ToString() );
				_writer.WriteAttributeString( "ShapeId", FindIndex( shapes, body.fixtureList[i].shape ).ToString() );
				_writer.WriteEndElement();
			}

			_writer.WriteEndElement();
			_writer.WriteEndElement();
		}

		static void SerializeJoint( List<Body> bodies, Joint joint )
		{
			_writer.WriteStartElement( "Joint" );
			_writer.WriteAttributeString( "Type", joint.jointType.ToString() );

			WriteElement( "BodyA", FindIndex( bodies, joint.bodyA ) );
			WriteElement( "BodyB", FindIndex( bodies, joint.bodyB ) );

			WriteElement( "CollideConnected", joint.collideConnected );

			WriteElement( "Breakpoint", joint.breakpoint );

			if( joint.userData != null )
			{
				_writer.WriteStartElement( "UserData" );
				WriteDynamicType( joint.userData.GetType(), joint.userData );
				_writer.WriteEndElement();
			}

			switch( joint.jointType )
			{
				case JointType.Distance:
					{
						var distanceJoint = (DistanceJoint)joint;
						WriteElement( "DampingRatio", distanceJoint.dampingRatio );
						WriteElement( "FrequencyHz", distanceJoint.frequency );
						WriteElement( "Length", distanceJoint.length );
						WriteElement( "LocalAnchorA", distanceJoint.localAnchorA );
						WriteElement( "LocalAnchorB", distanceJoint.localAnchorB );
					}
					break;
				case JointType.Friction:
					{
						var frictionJoint = (FrictionJoint)joint;
						WriteElement( "LocalAnchorA", frictionJoint.localAnchorA );
						WriteElement( "LocalAnchorB", frictionJoint.localAnchorB );
						WriteElement( "MaxForce", frictionJoint.maxForce );
						WriteElement( "MaxTorque", frictionJoint.maxTorque );
					}
					break;
				case JointType.Gear:
					throw new Exception( "Gear joint not supported by serialization" );
				case JointType.Wheel:
					{
						var wheelJoint = (WheelJoint)joint;
						WriteElement( "EnableMotor", wheelJoint.motorEnabled );
						WriteElement( "LocalAnchorA", wheelJoint.localAnchorA );
						WriteElement( "LocalAnchorB", wheelJoint.localAnchorB );
						WriteElement( "MotorSpeed", wheelJoint.motorSpeed );
						WriteElement( "DampingRatio", wheelJoint.dampingRatio );
						WriteElement( "MaxMotorTorque", wheelJoint.maxMotorTorque );
						WriteElement( "FrequencyHz", wheelJoint.frequency );
						WriteElement( "Axis", wheelJoint.axis );
					}
					break;
				case JointType.Prismatic:
					{
						//NOTE: Does not conform with Box2DScene

						var prismaticJoint = (PrismaticJoint)joint;
						WriteElement( "EnableLimit", prismaticJoint.limitEnabled );
						WriteElement( "EnableMotor", prismaticJoint.motorEnabled );
						WriteElement( "LocalAnchorA", prismaticJoint.localAnchorA );
						WriteElement( "LocalAnchorB", prismaticJoint.localAnchorB );
						WriteElement( "Axis", prismaticJoint.axis );
						WriteElement( "LowerTranslation", prismaticJoint.lowerLimit );
						WriteElement( "UpperTranslation", prismaticJoint.upperLimit );
						WriteElement( "MaxMotorForce", prismaticJoint.maxMotorForce );
						WriteElement( "MotorSpeed", prismaticJoint.motorSpeed );
					}
					break;
				case JointType.Pulley:
					{
						var pulleyJoint = (PulleyJoint)joint;
						WriteElement( "WorldAnchorA", pulleyJoint.worldAnchorA );
						WriteElement( "WorldAnchorB", pulleyJoint.worldAnchorB );
						WriteElement( "LengthA", pulleyJoint.lengthA );
						WriteElement( "LengthB", pulleyJoint.lengthB );
						WriteElement( "LocalAnchorA", pulleyJoint.localAnchorA );
						WriteElement( "LocalAnchorB", pulleyJoint.localAnchorB );
						WriteElement( "Ratio", pulleyJoint.ratio );
						WriteElement( "Constant", pulleyJoint.constant );
					}
					break;
				case JointType.Revolute:
					{
						var revoluteJoint = (RevoluteJoint)joint;
						WriteElement( "EnableLimit", revoluteJoint.limitEnabled );
						WriteElement( "EnableMotor", revoluteJoint.motorEnabled );
						WriteElement( "LocalAnchorA", revoluteJoint.localAnchorA );
						WriteElement( "LocalAnchorB", revoluteJoint.localAnchorB );
						WriteElement( "LowerAngle", revoluteJoint.lowerLimit );
						WriteElement( "MaxMotorTorque", revoluteJoint.maxMotorTorque );
						WriteElement( "MotorSpeed", revoluteJoint.motorSpeed );
						WriteElement( "ReferenceAngle", revoluteJoint.referenceAngle );
						WriteElement( "UpperAngle", revoluteJoint.upperLimit );
					}
					break;
				case JointType.Weld:
					{
						var weldJoint = (WeldJoint)joint;
						WriteElement( "LocalAnchorA", weldJoint.localAnchorA );
						WriteElement( "LocalAnchorB", weldJoint.localAnchorB );
					}
					break;
				//
				// Not part of Box2DScene
				//
				case JointType.Rope:
					{
						var ropeJoint = (RopeJoint)joint;
						WriteElement( "LocalAnchorA", ropeJoint.localAnchorA );
						WriteElement( "LocalAnchorB", ropeJoint.localAnchorB );
						WriteElement( "MaxLength", ropeJoint.maxLength );
					}
					break;
				case JointType.Angle:
					{
						var angleJoint = (AngleJoint)joint;
						WriteElement( "BiasFactor", angleJoint.biasFactor );
						WriteElement( "MaxImpulse", angleJoint.maxImpulse );
						WriteElement( "Softness", angleJoint.softness );
						WriteElement( "TargetAngle", angleJoint.targetAngle );
					}
					break;
				case JointType.Motor:
					{
						var motorJoint = (MotorJoint)joint;
						WriteElement( "AngularOffset", motorJoint.angularOffset );
						WriteElement( "LinearOffset", motorJoint.linearOffset );
						WriteElement( "MaxForce", motorJoint.maxForce );
						WriteElement( "MaxTorque", motorJoint.maxTorque );
						WriteElement( "CorrectionFactor", motorJoint.correctionFactor );
					}
					break;
				default:
					throw new Exception( "Joint not supported" );
			}

			_writer.WriteEndElement();
		}

		static void WriteDynamicType( Type type, object val )
		{
			_writer.WriteElementString( "Type", type.AssemblyQualifiedName );

			_writer.WriteStartElement( "Value" );
			var serializer = new XmlSerializer( type );

			//var xmlnsEmpty = new XmlSerializerNamespaces();
			//xmlnsEmpty.Add( "", "" );

			serializer.Serialize( _writer, val );
			_writer.WriteEndElement();
		}

		static void WriteElement( string name, Vector2 vec )
		{
			_writer.WriteElementString( name, vec.X + " " + vec.Y );
		}

		static void WriteElement( string name, int val )
		{
			_writer.WriteElementString( name, val.ToString() );
		}

		static void WriteElement( string name, bool val )
		{
			_writer.WriteElementString( name, val.ToString() );
		}

		static void WriteElement( string name, float val )
		{
			_writer.WriteElementString( name, val.ToString() );
		}

		static int FindIndex( List<Body> list, Body item )
		{
			for( int i = 0; i < list.Count; ++i )
				if( list[i] == item )
					return i;

			return -1;
		}

		static int FindIndex( List<Fixture> list, Fixture item )
		{
			for( int i = 0; i < list.Count; ++i )
				if( list[i].compareTo( item ) )
					return i;

			return -1;
		}

		static int FindIndex( List<Shape> list, Shape item )
		{
			for( int i = 0; i < list.Count; ++i )
				if( list[i].CompareTo( item ) )
					return i;

			return -1;
		}

		static String Join<T>( String separator, IEnumerable<T> values )
		{
			using( IEnumerator<T> en = values.GetEnumerator() )
			{
				if( !en.MoveNext() )
					return String.Empty;

				var result = new StringBuilder();
				if( en.Current != null )
				{
					// handle the case that the enumeration has null entries
					// and the case where their ToString() override is broken
					string value = en.Current.ToString();
					if( value != null )
						result.Append( value );
				}

				while( en.MoveNext() )
				{
					result.Append( separator );
					if( en.Current != null )
					{
						// handle the case that the enumeration has null entries
						// and the case where their ToString() override is broken
						string value = en.Current.ToString();
						if( value != null )
							result.Append( value );
					}
				}
				return result.ToString();
			}
		}

		internal static void Serialize( World world, Stream stream )
		{
			var bodies = new List<Body>();
			var fixtures = new List<Fixture>();
			var shapes = new List<Shape>();

			var settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = false;
			settings.OmitXmlDeclaration = true;

			using( _writer = XmlWriter.Create( stream, settings ) )
			{

				_writer.WriteStartElement( "World" );
				_writer.WriteAttributeString( "Version", "3" );
				WriteElement( "Gravity", world.gravity );

				_writer.WriteStartElement( "Shapes" );

				foreach( Body body in world.bodyList )
				{
					foreach( Fixture fixture in body.fixtureList )
					{
						if( !shapes.Any( s2 => fixture.shape.CompareTo( s2 ) ) )
						{
							SerializeShape( fixture.shape );
							shapes.Add( fixture.shape );
						}
					}
				}

				_writer.WriteEndElement();
				_writer.WriteStartElement( "Fixtures" );

				foreach( Body body in world.bodyList )
				{
					foreach( Fixture fixture in body.fixtureList )
					{
						if( !fixtures.Any( f2 => fixture.compareTo( f2 ) ) )
						{
							SerializeFixture( fixture );
							fixtures.Add( fixture );
						}
					}
				}

				_writer.WriteEndElement();
				_writer.WriteStartElement( "Bodies" );

				foreach( var body in world.bodyList )
				{
					bodies.Add( body );
					SerializeBody( fixtures, shapes, body );
				}

				_writer.WriteEndElement();
				_writer.WriteStartElement( "Joints" );

				foreach( Joint joint in world.jointList )
				{
					SerializeJoint( bodies, joint );
				}

				_writer.WriteEndElement();
				_writer.WriteEndElement();

				_writer.Flush();
			}
		}
	}


	internal static class WorldXmlDeserializer
	{
		internal static World Deserialize( Stream stream )
		{
			var world = new World( Vector2.Zero );
			Deserialize( world, stream );
			return world;
		}

		static void Deserialize( World world, Stream stream )
		{
			var bodies = new List<Body>();
			var fixtures = new List<Fixture>();
			var joints = new List<Joint>();
			var shapes = new List<Shape>();

			XMLFragmentElement root = XMLFragmentParser.LoadFromStream( stream );

			if( root.Name.ToLower() != "world" )
				throw new Exception();

			//Read gravity
			foreach( XMLFragmentElement element in root.Elements )
			{
				if( element.Name.ToLower() == "gravity" )
				{
					world.gravity = ReadVector( element );
					break;
				}
			}

			//Read shapes
			foreach( XMLFragmentElement shapeElement in root.Elements )
			{
				if( shapeElement.Name.ToLower() == "shapes" )
				{
					foreach( XMLFragmentElement element in shapeElement.Elements )
					{
						if( element.Name.ToLower() != "shape" )
							throw new Exception();

						var type = (ShapeType)Enum.Parse( typeof( ShapeType ), element.Attributes[0].Value, true );
						float density = float.Parse( element.Attributes[1].Value );

						switch(type)
						{
							case ShapeType.Circle:
								{
									var shape = new CircleShape();
									shape._density = density;

									foreach( XMLFragmentElement sn in element.Elements )
									{
										switch( sn.Name.ToLower() )
										{
											case "radius":
												shape.radius = float.Parse( sn.Value );
												break;
											case "position":
												shape.position = ReadVector( sn );
												break;
											default:
												throw new Exception();
										}
									}

									shapes.Add( shape );
								}
								break;
							case ShapeType.Polygon:
								{
									var shape = new PolygonShape();
									shape._density = density;

									foreach( XMLFragmentElement sn in element.Elements )
									{
										switch( sn.Name.ToLower() )
										{
											case "vertices":
												{
													var verts = new List<Vector2>( sn.Elements.Count );

													foreach( XMLFragmentElement vert in sn.Elements )
														verts.Add( ReadVector( vert ) );

													shape.vertices = new Vertices( verts );
												}
												break;
											case "centroid":
												shape.massData.centroid = ReadVector( sn );
												break;
										}
									}

									shapes.Add( shape );
								}
								break;
							case ShapeType.Edge:
								{
									var shape = new EdgeShape();
									shape._density = density;

									foreach( XMLFragmentElement sn in element.Elements )
									{
										switch( sn.Name.ToLower() )
										{
											case "hasvertex0":
												shape.hasVertex0 = bool.Parse( sn.Value );
												break;
											case "hasvertex3":
												shape.hasVertex0 = bool.Parse( sn.Value );
												break;
											case "vertex0":
												shape.vertex0 = ReadVector( sn );
												break;
											case "vertex1":
												shape.vertex1 = ReadVector( sn );
												break;
											case "vertex2":
												shape.vertex2 = ReadVector( sn );
												break;
											case "vertex3":
												shape.vertex3 = ReadVector( sn );
												break;
											default:
												throw new Exception();
										}
									}
									shapes.Add( shape );
								}
								break;
							case ShapeType.Chain:
								{
									var shape = new ChainShape();
									shape._density = density;

									foreach( XMLFragmentElement sn in element.Elements )
									{
										switch( sn.Name.ToLower() )
										{
											case "vertices":
												{
													var verts = new List<Vector2>( sn.Elements.Count );

													foreach( XMLFragmentElement vert in sn.Elements )
														verts.Add( ReadVector( vert ) );

													shape.vertices = new Vertices( verts );
												}
												break;
											case "nextvertex":
												shape.nextVertex = ReadVector( sn );
												break;
											case "prevvertex":
												shape.prevVertex = ReadVector( sn );
												break;

											default:
												throw new Exception();
										}
									}
									shapes.Add( shape );
								}
								break;
						}
					}
				}
			}

			//Read fixtures
			foreach( XMLFragmentElement fixtureElement in root.Elements )
			{
				if( fixtureElement.Name.ToLower() == "fixtures" )
				{
					foreach( XMLFragmentElement element in fixtureElement.Elements )
					{
						var fixture = new Fixture();

						if( element.Name.ToLower() != "fixture" )
							throw new Exception();

						fixture.fixtureId = int.Parse( element.Attributes[0].Value );

						foreach( XMLFragmentElement sn in element.Elements )
						{
							switch( sn.Name.ToLower() )
							{
								case "filterdata":
									foreach( XMLFragmentElement ssn in sn.Elements )
									{
										switch( ssn.Name.ToLower() )
										{
											case "categorybits":
												fixture._collisionCategories = (Category)int.Parse( ssn.Value );
												break;
											case "maskbits":
												fixture._collidesWith = (Category)int.Parse( ssn.Value );
												break;
											case "groupindex":
												fixture._collisionGroup = short.Parse( ssn.Value );
												break;
											case "CollisionIgnores":
												string[] split = ssn.Value.Split( '|' );
												foreach( string s in split )
												{
													fixture._collisionIgnores.Add( int.Parse( s ) );
												}
												break;
										}
									}

									break;
								case "friction":
									fixture.friction = float.Parse( sn.Value );
									break;
								case "issensor":
									fixture.isSensor = bool.Parse( sn.Value );
									break;
								case "restitution":
									fixture.restitution = float.Parse( sn.Value );
									break;
								case "userdata":
									fixture.userData = ReadSimpleType( sn, null, false );
									break;
							}
						}

						fixtures.Add( fixture );
					}
				}
			}

			//Read bodies
			foreach( XMLFragmentElement bodyElement in root.Elements )
			{
				if( bodyElement.Name.ToLower() == "bodies" )
				{
					foreach( XMLFragmentElement element in bodyElement.Elements )
					{
						var body = new Body( world );

						if( element.Name.ToLower() != "body" )
							throw new Exception();

						body.bodyType = (BodyType)Enum.Parse( typeof( BodyType ), element.Attributes[0].Value, true );

						foreach( XMLFragmentElement sn in element.Elements )
						{
							switch( sn.Name.ToLower() )
							{
								case "active":
									body._enabled = bool.Parse( sn.Value );
									break;
								case "allowsleep":
									body.isSleepingAllowed = bool.Parse( sn.Value );
									break;
								case "angle":
									{
										Vector2 position = body.position;
										body.setTransformIgnoreContacts( ref position, float.Parse( sn.Value ) );
									}
									break;
								case "angulardamping":
									body.angularDamping = float.Parse( sn.Value );
									break;
								case "angularvelocity":
									body.angularVelocity = float.Parse( sn.Value );
									break;
								case "awake":
									body.isAwake = bool.Parse( sn.Value );
									break;
								case "bullet":
									body.isBullet = bool.Parse( sn.Value );
									break;
								case "fixedrotation":
									body.fixedRotation = bool.Parse( sn.Value );
									break;
								case "lineardamping":
									body.linearDamping = float.Parse( sn.Value );
									break;
								case "linearvelocity":
									body.linearVelocity = ReadVector( sn );
									break;
								case "position":
									{
										float rotation = body.rotation;
										Vector2 position = ReadVector( sn );
										body.setTransformIgnoreContacts( ref position, rotation );
									}
									break;
								case "userdata":
									body.userData = ReadSimpleType( sn, null, false );
									break;
								case "bindings":
									{
										foreach( XMLFragmentElement pair in sn.Elements )
										{
											Fixture fix = fixtures[int.Parse( pair.Attributes[0].Value )];
											fix.shape = shapes[int.Parse( pair.Attributes[1].Value )].clone();
											fix.cloneOnto( body );
										}
										break;
									}
							}
						}

						bodies.Add( body );
					}
				}
			}

			//Read joints
			foreach( XMLFragmentElement jointElement in root.Elements )
			{
				if( jointElement.Name.ToLower() == "joints" )
				{
					foreach( XMLFragmentElement n in jointElement.Elements )
					{
						Joint joint;

						if( n.Name.ToLower() != "joint" )
							throw new Exception();

						var type = (JointType)Enum.Parse( typeof( JointType ), n.Attributes[0].Value, true );

						int bodyAIndex = -1, bodyBIndex = -1;
						bool collideConnected = false;
						object userData = null;

						foreach( XMLFragmentElement sn in n.Elements )
						{
							switch( sn.Name.ToLower() )
							{
								case "bodya":
									bodyAIndex = int.Parse( sn.Value );
									break;
								case "bodyb":
									bodyBIndex = int.Parse( sn.Value );
									break;
								case "collideconnected":
									collideConnected = bool.Parse( sn.Value );
									break;
								case "userdata":
									userData = ReadSimpleType( sn, null, false );
									break;
							}
						}

						Body bodyA = bodies[bodyAIndex];
						Body bodyB = bodies[bodyBIndex];

						switch(type)
						{
							case JointType.Distance:
								joint = new DistanceJoint();
								break;
							case JointType.Friction:
								joint = new FrictionJoint();
								break;
							case JointType.Wheel:
								joint = new WheelJoint();
								break;
							case JointType.Prismatic:
								joint = new PrismaticJoint();
								break;
							case JointType.Pulley:
								joint = new PulleyJoint();
								break;
							case JointType.Revolute:
								joint = new RevoluteJoint();
								break;
							case JointType.Weld:
								joint = new WeldJoint();
								break;
							case JointType.Rope:
								joint = new RopeJoint();
								break;
							case JointType.Angle:
								joint = new AngleJoint();
								break;
							case JointType.Motor:
								joint = new MotorJoint();
								break;
							case JointType.Gear:
								throw new Exception( "GearJoint is not supported." );
							default:
								throw new Exception( "Invalid or unsupported joint." );
						}

						joint.collideConnected = collideConnected;
						joint.userData = userData;
						joint.bodyA = bodyA;
						joint.bodyB = bodyB;
						joints.Add( joint );
						world.addJoint( joint );

						foreach( XMLFragmentElement sn in n.Elements)
						{
							// check for specific nodes
							switch( type )
							{
								case JointType.Distance:
									{
										switch( sn.Name.ToLower() )
										{
											case "dampingratio":
												( (DistanceJoint)joint ).dampingRatio = float.Parse( sn.Value );
												break;
											case "frequencyhz":
												( (DistanceJoint)joint ).frequency = float.Parse( sn.Value );
												break;
											case "length":
												( (DistanceJoint)joint ).length = float.Parse( sn.Value );
												break;
											case "localanchora":
												( (DistanceJoint)joint ).localAnchorA = ReadVector( sn );
												break;
											case "localanchorb":
												( (DistanceJoint)joint ).localAnchorB = ReadVector( sn );
												break;
										}
									}
									break;
								case JointType.Friction:
									{
										switch( sn.Name.ToLower() )
										{
											case "localanchora":
												( (FrictionJoint)joint ).localAnchorA = ReadVector( sn );
												break;
											case "localanchorb":
												( (FrictionJoint)joint ).localAnchorB = ReadVector( sn );
												break;
											case "maxforce":
												( (FrictionJoint)joint ).maxForce = float.Parse( sn.Value );
												break;
											case "maxtorque":
												( (FrictionJoint)joint ).maxTorque = float.Parse( sn.Value );
												break;
										}
									}
									break;
								case JointType.Wheel:
									{
										switch( sn.Name.ToLower() )
										{
											case "enablemotor":
												( (WheelJoint)joint ).motorEnabled = bool.Parse( sn.Value );
												break;
											case "localanchora":
												( (WheelJoint)joint ).localAnchorA = ReadVector( sn );
												break;
											case "localanchorb":
												( (WheelJoint)joint ).localAnchorB = ReadVector( sn );
												break;
											case "motorspeed":
												( (WheelJoint)joint ).motorSpeed = float.Parse( sn.Value );
												break;
											case "dampingratio":
												( (WheelJoint)joint ).dampingRatio = float.Parse( sn.Value );
												break;
											case "maxmotortorque":
												( (WheelJoint)joint ).maxMotorTorque = float.Parse( sn.Value );
												break;
											case "frequencyhz":
												( (WheelJoint)joint ).frequency = float.Parse( sn.Value );
												break;
											case "axis":
												( (WheelJoint)joint ).axis = ReadVector( sn );
												break;
										}
									}
									break;
								case JointType.Prismatic:
									{
										switch( sn.Name.ToLower() )
										{
											case "enablelimit":
												( (PrismaticJoint)joint ).limitEnabled = bool.Parse( sn.Value );
												break;
											case "enablemotor":
												( (PrismaticJoint)joint ).motorEnabled = bool.Parse( sn.Value );
												break;
											case "localanchora":
												( (PrismaticJoint)joint ).localAnchorA = ReadVector( sn );
												break;
											case "localanchorb":
												( (PrismaticJoint)joint ).localAnchorB = ReadVector( sn );
												break;
											case "axis":
												( (PrismaticJoint)joint ).axis = ReadVector( sn );
												break;
											case "maxmotorforce":
												( (PrismaticJoint)joint ).maxMotorForce = float.Parse( sn.Value );
												break;
											case "motorspeed":
												( (PrismaticJoint)joint ).motorSpeed = float.Parse( sn.Value );
												break;
											case "lowertranslation":
												( (PrismaticJoint)joint ).lowerLimit = float.Parse( sn.Value );
												break;
											case "uppertranslation":
												( (PrismaticJoint)joint ).upperLimit = float.Parse( sn.Value );
												break;
											case "referenceangle":
												( (PrismaticJoint)joint ).referenceAngle = float.Parse( sn.Value );
												break;
										}
									}
									break;
								case JointType.Pulley:
									{
										switch( sn.Name.ToLower() )
										{
											case "worldanchora":
												( (PulleyJoint)joint ).worldAnchorA = ReadVector( sn );
												break;
											case "worldanchorb":
												( (PulleyJoint)joint ).worldAnchorB = ReadVector( sn );
												break;
											case "lengtha":
												( (PulleyJoint)joint ).lengthA = float.Parse( sn.Value );
												break;
											case "lengthb":
												( (PulleyJoint)joint ).lengthB = float.Parse( sn.Value );
												break;
											case "localanchora":
												( (PulleyJoint)joint ).localAnchorA = ReadVector( sn );
												break;
											case "localanchorb":
												( (PulleyJoint)joint ).localAnchorB = ReadVector( sn );
												break;
											case "ratio":
												( (PulleyJoint)joint ).ratio = float.Parse( sn.Value );
												break;
											case "constant":
												( (PulleyJoint)joint ).constant = float.Parse( sn.Value );
												break;
										}
									}
									break;
								case JointType.Revolute:
									{
										switch( sn.Name.ToLower() )
										{
											case "enablelimit":
												( (RevoluteJoint)joint ).limitEnabled = bool.Parse( sn.Value );
												break;
											case "enablemotor":
												( (RevoluteJoint)joint ).motorEnabled = bool.Parse( sn.Value );
												break;
											case "localanchora":
												( (RevoluteJoint)joint ).localAnchorA = ReadVector( sn );
												break;
											case "localanchorb":
												( (RevoluteJoint)joint ).localAnchorB = ReadVector( sn );
												break;
											case "maxmotortorque":
												( (RevoluteJoint)joint ).maxMotorTorque = float.Parse( sn.Value );
												break;
											case "motorspeed":
												( (RevoluteJoint)joint ).motorSpeed = float.Parse( sn.Value );
												break;
											case "lowerangle":
												( (RevoluteJoint)joint ).lowerLimit = float.Parse( sn.Value );
												break;
											case "upperangle":
												( (RevoluteJoint)joint ).upperLimit = float.Parse( sn.Value );
												break;
											case "referenceangle":
												( (RevoluteJoint)joint ).referenceAngle = float.Parse( sn.Value );
												break;
										}
									}
									break;
								case JointType.Weld:
									{
										switch( sn.Name.ToLower() )
										{
											case "localanchora":
												( (WeldJoint)joint ).localAnchorA = ReadVector( sn );
												break;
											case "localanchorb":
												( (WeldJoint)joint ).localAnchorB = ReadVector( sn );
												break;
										}
									}
									break;
								case JointType.Rope:
									{
										switch( sn.Name.ToLower() )
										{
											case "localanchora":
												( (RopeJoint)joint ).localAnchorA = ReadVector( sn );
												break;
											case "localanchorb":
												( (RopeJoint)joint ).localAnchorB = ReadVector( sn );
												break;
											case "maxlength":
												( (RopeJoint)joint ).maxLength = float.Parse( sn.Value );
												break;
										}
									}
									break;
								case JointType.Gear:
									throw new Exception( "Gear joint is unsupported" );
								case JointType.Angle:
									{
										switch( sn.Name.ToLower() )
										{
											case "biasfactor":
												( (AngleJoint)joint ).biasFactor = float.Parse( sn.Value );
												break;
											case "maximpulse":
												( (AngleJoint)joint ).maxImpulse = float.Parse( sn.Value );
												break;
											case "softness":
												( (AngleJoint)joint ).softness = float.Parse( sn.Value );
												break;
											case "targetangle":
												( (AngleJoint)joint ).targetAngle = float.Parse( sn.Value );
												break;
										}
									}
									break;
								case JointType.Motor:
									switch( sn.Name.ToLower() )
									{
										case "angularoffset":
											( (MotorJoint)joint ).angularOffset = float.Parse( sn.Value );
											break;
										case "linearoffset":
											( (MotorJoint)joint ).linearOffset = ReadVector( sn );
											break;
										case "maxforce":
											( (MotorJoint)joint ).maxForce = float.Parse( sn.Value );
											break;
										case "maxtorque":
											( (MotorJoint)joint ).maxTorque = float.Parse( sn.Value );
											break;
										case "correctionfactor":
											( (MotorJoint)joint ).correctionFactor = float.Parse( sn.Value );
											break;
									}
									break;
							}
						}
					}
				}
			}

			world.processChanges();
		}

		static Vector2 ReadVector( XMLFragmentElement node )
		{
			string[] values = node.Value.Split( ' ' );
			return new Vector2( float.Parse( values[0] ), float.Parse( values[1] ) );
		}

		static object ReadSimpleType( XMLFragmentElement node, Type type, bool outer )
		{
			if( type == null )
				return ReadSimpleType( node.Elements[1], Type.GetType( node.Elements[0].Value ), outer );

			var serializer = new XmlSerializer( type );
			using( var stream = new MemoryStream() )
			{
				var writer = new StreamWriter( stream );
				{
					writer.Write( ( outer ) ? node.OuterXml : node.InnerXml );
					writer.Flush();
					stream.Position = 0;
				}
				var settings = new XmlReaderSettings();
				settings.ConformanceLevel = ConformanceLevel.Fragment;

				return serializer.Deserialize( XmlReader.Create( stream, settings ) );
			}
		}
	}


	#region XMLFragment

	internal class XMLFragmentAttribute
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}

	internal class XMLFragmentElement
	{
		List<XMLFragmentAttribute> _attributes = new List<XMLFragmentAttribute>();
		List<XMLFragmentElement> _elements = new List<XMLFragmentElement>();

		public IList<XMLFragmentElement> Elements
		{
			get { return _elements; }
		}

		public IList<XMLFragmentAttribute> Attributes
		{
			get { return _attributes; }
		}

		public string Name { get; set; }
		public string Value { get; set; }
		public string OuterXml { get; set; }
		public string InnerXml { get; set; }
	}

	internal class XMLFragmentException : Exception
	{
		public XMLFragmentException( string message )
			: base( message )
		{
		}
	}

	internal class FileBuffer
	{
		public FileBuffer( Stream stream )
		{
			using( StreamReader sr = new StreamReader( stream ) )
				Buffer = sr.ReadToEnd();

			Position = 0;
		}

		public string Buffer { get; set; }

		public int Position { get; set; }

		int Length
		{
			get { return Buffer.Length; }
		}

		public char Next
		{
			get
			{
				char c = Buffer[Position];
				Position++;
				return c;
			}
		}

		public bool EndOfBuffer
		{
			get { return Position == Length; }
		}
	}


	internal class XMLFragmentParser
	{
		static List<char> _punctuation = new List<char> { '/', '<', '>', '=' };
		FileBuffer _buffer;
		XMLFragmentElement _rootNode;

		public XMLFragmentParser( Stream stream )
		{
			Load( stream );
		}

		public XMLFragmentElement RootNode
		{
			get { return _rootNode; }
		}

		public void Load( Stream stream )
		{
			_buffer = new FileBuffer( stream );
		}

		public static XMLFragmentElement LoadFromStream( Stream stream )
		{
			var x = new XMLFragmentParser( stream );
			x.Parse();
			return x.RootNode;
		}

		string NextToken()
		{
			string str = "";
			bool _done = false;

			while(true)
			{
				char c = _buffer.Next;

				if( _punctuation.Contains( c ) )
				{
					if( str != "" )
					{
						_buffer.Position--;
						break;
					}

					_done = true;
				}
				else if( char.IsWhiteSpace( c ) )
				{
					if( str != "" )
						break;
					else
						continue;
				}

				str += c;

				if(_done)
					break;
			}

			str = TrimControl( str );

			// Trim quotes from start and end
			if( str[0] == '\"' )
				str = str.Remove( 0, 1 );

			if( str[str.Length - 1] == '\"' )
				str = str.Remove( str.Length - 1, 1 );

			return str;
		}

		string PeekToken()
		{
			int oldPos = _buffer.Position;
			string str = NextToken();
			_buffer.Position = oldPos;
			return str;
		}

		string ReadUntil( char c )
		{
			string str = "";

			while(true)
			{
				char ch = _buffer.Next;

				if( ch == c )
				{
					_buffer.Position--;
					break;
				}

				str += ch;
			}

			// Trim quotes from start and end
			if( str[0] == '\"' )
				str = str.Remove( 0, 1 );

			if( str[str.Length - 1] == '\"' )
				str = str.Remove( str.Length - 1, 1 );

			return str;
		}

		string TrimControl( string str )
		{
			string newStr = str;

			// Trim control characters
			int i = 0;
			while(true)
			{
				if( i == newStr.Length )
					break;

				if( char.IsControl( newStr[i] ) )
					newStr = newStr.Remove( i, 1 );
				else
					i++;
			}

			return newStr;
		}

		string TrimTags( string outer )
		{
			int start = outer.IndexOf( '>' ) + 1;
			int end = outer.LastIndexOf( '<' );

			return TrimControl( outer.Substring( start, end - start ) );
		}

		public XMLFragmentElement TryParseNode()
		{
			if( _buffer.EndOfBuffer )
				return null;

			int startOuterXml = _buffer.Position;
			string token = NextToken();

			if( token != "<" )
				throw new XMLFragmentException( "Expected \"<\", got " + token );

			var element = new XMLFragmentElement();
			element.Name = NextToken();

			while( true )
			{
				token = NextToken();

				if( token == ">" )
					break;
				else if( token == "/" ) // quick-exit case
				{
					NextToken();

					element.OuterXml =
						TrimControl( _buffer.Buffer.Substring( startOuterXml, _buffer.Position - startOuterXml ) ).Trim();
					element.InnerXml = "";

					return element;
				}
				else
				{
					var attribute = new XMLFragmentAttribute();
					attribute.Name = token;
					if( ( token = NextToken() ) != "=" )
						throw new XMLFragmentException( "Expected \"=\", got " + token );
					attribute.Value = NextToken();

					element.Attributes.Add( attribute );
				}
			}

			while( true )
			{
				int oldPos = _buffer.Position; // for restoration below
				token = NextToken();

				if( token == "<" )
				{
					token = PeekToken();

					if( token == "/" ) // finish element
					{
						NextToken(); // skip the / again
						token = NextToken();
						NextToken(); // skip >

						element.OuterXml = TrimControl( _buffer.Buffer.Substring( startOuterXml, _buffer.Position - startOuterXml ) ).Trim();
						element.InnerXml = TrimTags( element.OuterXml );

						if( token != element.Name )
							throw new XMLFragmentException( "Mismatched element pairs: \"" + element.Name + "\" vs \"" +
														   token + "\"" );

						break;
					}
					else
					{
						_buffer.Position = oldPos;
						element.Elements.Add( TryParseNode() );
					}
				}
				else
				{
					// value, probably
					_buffer.Position = oldPos;
					element.Value = ReadUntil( '<' );
				}
			}

			return element;
		}

		void Parse()
		{
			_rootNode = TryParseNode();

			if( _rootNode == null )
				throw new XMLFragmentException( "Unable to load root node" );
		}
	}

	#endregion

}