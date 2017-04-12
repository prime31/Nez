using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Particles;
using Nez.Textures;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.ParticleEditor
{
    class ParticleCreate : Component, IUpdatable
    {

        public ParticleEmitter pEmitter;
        public bool _isCollisionEnabled = false;
        public bool _simulateInWorldSpace = true;
        public ParticleEmitterConfig pConfig;

        public void update()
        {
           // pEmitter.collisionConfig.enabled = _isCollisionEnabled;
           // pEmitter.simulateInWorldSpace = _simulateInWorldSpace;

            
        }

        public override void onAddedToEntity()
        {
            pEmitter = entity.addComponent( new ParticleEmitter( pConfig ) );
            
            base.onAddedToEntity();
        }
    }

    class ParticleEditorScene : Scene
    {
        protected const int SCREEN_SPACE = 999;
        UICanvas pCanvas;

        ParticleCreate particle;

        Entity pEntity;

        public ParticleEditorScene()
        {
            addRenderer( new ScreenSpaceRenderer( 100, SCREEN_SPACE ) );

            

            var pConfig = new ParticleEmitterConfig();

            pConfig.duration = -1;
            
            pConfig.subtexture = new Subtexture( content.Load<Texture2D>( "Particle\\JasonChoi_Swirl01.png" ) );


            var pUIEntity = createEntity("ParticleUI");

            pCanvas = pUIEntity.addComponent( new UICanvas());
            pCanvas.isFullScreen = true;
            
            pCanvas.renderLayer = SCREEN_SPACE;

            var table = pCanvas.stage.addElement(new Table());
            table.setDebug( true );
            table.setFillParent( true ).right().top();

            var skin = Skin.createDefaultSkin();

            var stack = new Stack();

            Table tabPane1 = new Table();
            {
                tabPane1.pad( 5 ).top();

                tabPane1.add( new Label( "EMITTER TYPE", skin ) );
                tabPane1.row().pad( 5 ).setPadBottom( 20 );

                {
                    tabPane1.add( new Label( "Emitter Type", skin ) );

                    var sel = new SelectBox<string>( skin );

                    sel.setItems( new string[] { "Gravity", "Radial" } );

                    sel.onChanged += valu => {

                        switch ( sel.getSelectedIndex() )
                        {
                            default:
                            case 0:
                                pConfig.emitterType = ParticleEmitterType.Gravity;
                                break;

                            case 1:
                                pConfig.emitterType = ParticleEmitterType.Radial;
                                break;
                        }

                    };

                    tabPane1.add( sel ).fill();
                    tabPane1.row();
                }

                tabPane1.add( new Label( "CONFIGURATION", skin ) );
                tabPane1.row().pad( 5 );

                {
                    tabPane1.add( new Label( "Max Particles", skin ) );
                    tabPane1.add( new Slider( skin, null, 1, 1000, 5, pConfig.maxParticles = 500 ) ).getElement<Slider>().onChanged+= valu => pConfig.maxParticles = (uint)valu;
                    tabPane1.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    tabPane1.add( new Label( "Lifespan", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 10, 0.1f, pConfig.particleLifespan = 0.7237f ) ).getElement<Slider>().onChanged += valu => pConfig.particleLifespan = valu;
                    tabPane1.row().pad( 5 );
                }
                { 
                    tabPane1.add( new Label( "Lifespan Variance", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 10, 0.1f, pConfig.particleLifespanVariance = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.particleLifespanVariance = valu;
                    tabPane1.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    tabPane1.add( new Label( "Start Size", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 70, 0.8f, pConfig.startParticleSize = 50.95f ) ).getElement<Slider>().onChanged += valu => pConfig.startParticleSize = valu;
                    tabPane1.row().pad( 5 );
                }
                {
                    tabPane1.add( new Label( "Start Size Variance", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 70, 0.8f, pConfig.startParticleSizeVariance = 53.47f ) ).getElement<Slider>().onChanged += valu => pConfig.startParticleSizeVariance = valu;
                    tabPane1.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    tabPane1.add( new Label( "Finish Size", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 70, 0.8f, pConfig.finishParticleSize = 64 ) ).getElement<Slider>().onChanged += valu => pConfig.finishParticleSize = valu;
                    tabPane1.row().pad( 5 );
                }
                {
                    tabPane1.add( new Label( "Finish Size Variance", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 70, 0.8f, pConfig.finishParticleSizeVariance = 64 ) ).getElement<Slider>().onChanged += valu => pConfig.finishParticleSizeVariance = valu;
                    tabPane1.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    tabPane1.add( new Label( "Angle", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 360, 0.8f, pConfig.angle = 244.11f ) ).getElement<Slider>().onChanged += valu => pConfig.angle = valu;
                    tabPane1.row().pad( 5 );
                }
                {
                    tabPane1.add( new Label( "Angle Variance", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 360, 0.8f, pConfig.angleVariance = 142.62f ) ).getElement<Slider>().onChanged += valu => pConfig.angleVariance = valu;
                    tabPane1.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    ;
                    tabPane1.add( new Label( "Start Rotation", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 360, 0.8f, pConfig.rotationStart = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.rotationStart = valu;
                    tabPane1.row().pad( 5 );
                }
                {
                    tabPane1.add( new Label( "Start Rotation Variance", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 360, 0.8f, pConfig.rotationStartVariance = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.rotationStartVariance = valu;
                    tabPane1.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    tabPane1.add( new Label( "End Rotation", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 360, 0.5f, pConfig.rotationEnd = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.rotationEnd = valu;
                    tabPane1.row().pad( 5 );
                }
                {
                    tabPane1.add( new Label( "End Rotation Variance", skin ) );
                    tabPane1.add( new Slider( skin, null, 0, 360, 0.5f, pConfig.rotationEndVariance = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.rotationEndVariance = valu;
                    tabPane1.row().pad( 5 );
                }
                tabPane1.add();
                tabPane1.fillParent = true;
            }

            Table tabPane2 = new Table();
            {
                tabPane2.pad( 5 ).top();

                tabPane2.add( new Label( "GRAVITY EMITTER", skin ) );
                tabPane2.row().pad( 5 );
                {
                    tabPane2.add( new Label( "X Position", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 1000, 25f, pConfig.sourcePosition.X = 150f ) ).getElement<Slider>().onChanged += valu => pConfig.sourcePosition.X = valu;
                    tabPane2.row().pad( 5 );
                }
                {
                    tabPane2.add( new Label( "Y Position", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 1000, 25f, pConfig.sourcePosition.Y = 50f ) ).getElement<Slider>().onChanged += valu => pConfig.sourcePosition.Y = valu;
                    tabPane2.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    tabPane2.add( new Label( "X Position Variance", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 1000, 25f, pConfig.sourcePositionVariance.X = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.sourcePositionVariance.X = valu;
                    tabPane2.row().pad( 5 );
                }
                {
                    tabPane2.add( new Label( "Y Position Variance", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 1000, 25f, pConfig.sourcePositionVariance.Y = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.sourcePositionVariance.Y = valu;
                    tabPane2.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    tabPane2.add( new Label( "Speed", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 500, 5f, pConfig.speed = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.speed = valu;
                    tabPane2.row().pad( 5 );
                }
                {
                    tabPane2.add( new Label( "Speed Variance", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 500, 5f, pConfig.speedVariance = 190.79f ) ).getElement<Slider>().onChanged += valu => pConfig.speedVariance = valu;
                    tabPane2.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    tabPane2.add( new Label( "Gravity X", skin ) );
                    tabPane2.add( new Slider( skin, null, -1000, 1000, 5f, pConfig.gravity.X = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.gravity.X = valu;
                    tabPane2.row().pad( 5 );
                }
                {
                    tabPane2.add( new Label( "Gravity Y", skin ) );
                    tabPane2.add( new Slider( skin, null, -1000, 1000, 5f, pConfig.gravity.Y = 671.05f ) ).getElement<Slider>().onChanged += valu => pConfig.gravity.Y = valu;
                    tabPane2.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    //Tangential Acceleration
                    tabPane2.add( new Label( "Tang. Acc.", skin ) );
                    tabPane2.add( new Slider( skin, null, -500, 500, 5f, pConfig.tangentialAcceleration = 144.74f ) ).getElement<Slider>().onChanged += valu => pConfig.tangentialAcceleration = valu;
                    tabPane2.row().pad( 5 );
                }
                {
                    //Tangential Acceleration Variance
                    tabPane2.add( new Label( "Tang. Acc. Var.", skin ) );
                    tabPane2.add( new Slider( skin, null, -500, 500, 5f, pConfig.tangentialAccelVariance = 92.11f ) ).getElement<Slider>().onChanged += valu => pConfig.tangentialAccelVariance = valu;
                    tabPane2.row().pad( 5 ).setPadBottom( 20 );
                }
                {
                    //Radial Acceleration
                    tabPane2.add( new Label( "Rad. Acc.", skin ) );
                    tabPane2.add( new Slider( skin, null, -500, 500, 5f, pConfig.radialAcceleration = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.radialAcceleration = valu;
                    tabPane2.row().pad( 5 );
                }
                {
                    //Radial Acceleration Variance
                    tabPane2.add( new Label( "Rad. Acc. Var.", skin ) );
                    tabPane2.add( new Slider( skin, null, -500, 500, 5f, pConfig.radialAccelVariance = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.radialAccelVariance = valu;
                    tabPane2.row().pad( 5 );
                }

                tabPane2.add( new Label( "ROTATION EMITTER", skin ) );
                tabPane2.row().pad( 5 ).setPadBottom( 20 );

                {
                    tabPane2.add( new Label( "Max Radius", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 500, 5f, pConfig.maxRadius = 0 ) ).getElement<Slider>().onChanged += valu => pConfig.maxRadius = valu;
                    tabPane2.row().pad( 5 );
                }
                {
                    tabPane2.add( new Label( "Max Radius Variance", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 500, 5f, pConfig.maxRadiusVariance = 72.63f ) ).getElement<Slider>().onChanged += valu => pConfig.maxRadiusVariance = valu;
                    tabPane2.row().pad( 5 );
                }
                { 
                    tabPane2.add( new Label( "Min Radius", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 500, 5f, pConfig.minRadius = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.minRadius = valu;
                    tabPane2.row().pad( 5 );
                }
                {
                    tabPane2.add( new Label( "Min Radius Variance", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 500, 5f, pConfig.minRadiusVariance = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.minRadiusVariance = valu;
                    tabPane2.row().pad( 5 );
                }
                {
                    tabPane2.add( new Label( "Rotate per Second", skin ) );
                    tabPane2.add( new Slider( skin, null, -360, 360, 5f, pConfig.rotatePerSecond = 0f ) ).getElement<Slider>().onChanged += valu => pConfig.rotatePerSecond = valu;
                    tabPane2.row().pad( 5 );
                }
                {
                    tabPane2.add( new Label( "Rotate per Second Variance", skin ) );
                    tabPane2.add( new Slider( skin, null, 0, 500, 5f, pConfig.rotatePerSecondVariance = 153.95f ) ).getElement<Slider>().onChanged += valu => pConfig.rotatePerSecondVariance = valu;
                    tabPane2.row().pad( 5 );
                }
            }

            pConfig.startColor = new Color( 0.84f, 0.3f, 0f, 1f );
            pConfig.startColorVariance = new Color( 0f, 0f, 0f, 1f );
            pConfig.finishColor = new Color( 1f, 0.54f, 0.37f, 0f );
            pConfig.finishColorVariance = new Color( 0f, 0f, 0f, 0f );

            Table tabPane3 = new Table();
            {
                tabPane3.fillParent = true;
                {
                    tabPane3.add( new Label( "Start Color", skin ) );
                    tabPane3.row();

                    tabPane3.add( new Label( "R", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0.84f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.startColor = RecolorR( pConfig.startColor, valu);
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "G", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0.3f) ).getElement<Slider>().onChanged += valu =>
                    pConfig.startColor = RecolorG( pConfig.startColor, valu );

                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "B", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.startColor = RecolorB( pConfig.startColor, valu );
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "A", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 1f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.startColor = RecolorA( pConfig.startColor, valu );
                    tabPane3.row().pad( 5 ).setPadBottom( 20 );
                }

                {
                    tabPane3.add( new Label( "Start Color Variance", skin ) );
                    tabPane3.row();

                    tabPane3.add( new Label( "R", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.startColorVariance = RecolorR( pConfig.startColorVariance, valu );
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "G", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.startColorVariance = RecolorG( pConfig.startColorVariance, valu );
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "B", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.startColorVariance = RecolorB( pConfig.startColorVariance, valu );
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "A", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 1f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.startColorVariance = RecolorA( pConfig.startColorVariance, valu );
                    tabPane3.row().pad( 5 ).setPadBottom( 20 );
                }

                {
                    tabPane3.add( new Label( "Finish Color", skin ) );
                    tabPane3.row();

                    tabPane3.add( new Label( "R", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 1f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.finishColor = RecolorR( pConfig.finishColor, valu );
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "G", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0.54f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.finishColor = RecolorG( pConfig.finishColor, valu );
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "B", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0.37f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.finishColor = RecolorB( pConfig.finishColor, valu );
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "A", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.finishColor = RecolorA( pConfig.finishColor, valu );
                    tabPane3.row().pad( 5 ).setPadBottom( 20 );
                }
                
                {
                    tabPane3.add( new Label( "Finish Color Variance", skin ) );
                    tabPane3.row();

                    tabPane3.add( new Label( "R", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.finishColorVariance = RecolorR( pConfig.finishColorVariance, valu );
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "G", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.finishColorVariance = RecolorG( pConfig.finishColorVariance, valu );
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "B", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.finishColorVariance = RecolorB( pConfig.finishColorVariance, valu );
                    tabPane3.row().pad( 5 );

                    tabPane3.add( new Label( "A", skin ) );
                    tabPane3.add( new Slider( skin, null, 0, 1, 0.05f, 0f ) ).getElement<Slider>().onChanged += valu =>
                    pConfig.finishColorVariance = RecolorA( pConfig.finishColorVariance, valu );
                    tabPane3.row().pad( 5 ).setPadBottom( 20 );
                }

                tabPane3.add( new Label( "BLEND FUNCTION", skin ) );
                tabPane3.row().pad( 5 );

                string[] blendSelect = new string[] {
                        "Zero",
                        "One",

                        "SourceColor",
                        "InverseSourceColor",

                        "SourceAlpha",
                        "InverseSourceAlpha",

                        "DestinationAlpha",
                        "InverseDestinationAlpha",

                        "DestinationColor",
                        "InverseDestinationColor",

                        "SourceAlphaSaturation"
                    };

                {
                    pConfig.blendFuncSource = Blend.SourceAlpha;

                    tabPane3.add( new Label( "Source", skin ) );

                    var sel = new SelectBox<string>( skin );

                    sel.setItems( blendSelect );

                    sel.setSelected( "SourceAlpha" );

                    sel.onChanged += valu => {
                        pConfig.blendFuncSource = getBlend( sel.getSelectedIndex() );
                    };

                    tabPane3.add( sel );
                    tabPane3.row();
                }

                {
                    pConfig.blendFuncDestination = Blend.One;

                    tabPane3.add( new Label( "Destination", skin ) );

                    var sel = new SelectBox<string>( skin );

                    sel.setItems( blendSelect );

                    sel.setSelected( "One" );

                    sel.onChanged += valu => {
                        pConfig.blendFuncDestination = getBlend( sel.getSelectedIndex() );
                    };

                    tabPane3.add( sel );
                    tabPane3.row();
                }

            }

            ScrollPane pane1 = new ScrollPane( tabPane1, skin );

            ScrollPane pane2 = new ScrollPane( tabPane2, skin );
            pane2.setIsVisible( false );

            ScrollPane pane3 = new ScrollPane( tabPane3, skin );
            pane3.setIsVisible( false );

            stack.addElement( pane1 );
            stack.addElement( pane2 );
            stack.addElement( pane3 );

            var tabButton = new HorizontalGroup();

            Button tabButton1 = new TextButton( "PARTICLES", skin );
            tabButton1.pad( 10 );

            Button tabButton2 = new TextButton( "PARTICLE BEHAVIOR", skin );
            tabButton2.pad( 10 );

            Button tabButton3 = new TextButton( "PARTICLE COLOR", skin );
            tabButton3.pad( 10 );

            var act = new Action( delegate ()
            {
                pane1.setVisible( tabButton1.isChecked );
                pane2.setVisible( tabButton2.isChecked );
                pane3.setVisible( tabButton3.isChecked );
            } );

            tabButton.addElement( tabButton1 ).onChanged += bo => act();
            tabButton.addElement( tabButton2 ).onChanged += bo => act();
            tabButton.addElement( tabButton3 ).onChanged += bo => act();

            ButtonGroup tabs = new ButtonGroup();
            tabs.setMinCheckCount( 1 );
            tabs.setMaxCheckCount( 1 );
            tabs.add( tabButton1 );
            tabs.add( tabButton2 );
            tabs.add( tabButton3 );

            table.add( tabButton );
            table.row().setExpandY().setAlign( Align.topLeft );

            table.add( stack );

            addRenderer( new RenderLayerExcludeRenderer( 0, SCREEN_SPACE ) );
            
            pEntity = createEntity( "Particle" );
            
            pEntity.setPosition( Screen.center );

            particle = pEntity.addComponent( new ParticleCreate() );


            pConfig.emissionRate = pConfig.maxParticles / pConfig.particleLifespan;

            if ( float.IsInfinity( pConfig.emissionRate ) )
                pConfig.emissionRate = 10000;

            particle.pConfig = pConfig;

            
        }

        Color RecolorR ( Color color, float add )
        {
            return new Color( add, color.G, color.B, color.A );
        }

        Color RecolorG( Color color, float add )
        {
            return new Color( color.R, add, color.B, color.A );
        }

        Color RecolorB( Color color, float add )
        {
            return new Color( color.R, color.G, add, color.A );
        }

        Color RecolorA( Color color, float add )
        {
            return new Color( color.R, color.G, color.B, add );
        }

        public override void update()
        {
            base.update();
        }


        Blend getBlend( int index )
        {
            switch ( index )
            {
                default:
                case 0:
                    return Blend.Zero;
                case 1:
                    return Blend.One;

                case 0x0300:
                case 2:
                    return Blend.SourceColor;

                case 0x0301:
                case 3:
                    return Blend.InverseSourceColor;

                case 0x0302:
                case 4:
                    return Blend.SourceAlpha;

                case 0x0303:
                case 5:
                    return Blend.InverseSourceAlpha;

                case 0x0304:
                case 6:
                    return Blend.DestinationAlpha;

                case 0x0305:
                case 7:
                    return Blend.InverseDestinationAlpha;

                case 0x0306:
                case 8:
                    return Blend.DestinationColor;

                case 0x0307:
                case 9:
                    return Blend.InverseDestinationColor;

                case 0x0308:
                case 10:
                    return Blend.SourceAlphaSaturation;
            }
        }

        public override void initialize()
        {
            base.initialize();

            clearColor = Color.Black;

            setDesignResolution( 1280, 720, Scene.SceneResolutionPolicy.None );
            Screen.setSize( 1280, 720 );
        }
    }
}
