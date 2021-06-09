module.exports = {
  someSidebar: {
    'Docusaurus': ['about/introduction','about/Samples'],
    'Getting Started': [
      'getting-started/installation',
      'getting-started/FNACompat'
    ],
    'Features': [
      'features/Core',
      'features/ContentManagement',
      'features/RuntimeInspector',
      'features/Tiled',
      {'Utils': [
        'features/Utils/Tweening',
        'features/Utils/Collections',
        'features/Utils/Pooling',
      ]},
      {'Graphics': [
        {
          'Lighting': [
            'features/Graphics/Lighting/DeferredLighting',
            'features/Graphics/Lighting/SpriteLights',
          ]
        },
        'features/Graphics/Rendering',
        'features/Graphics/SVG',
        'features/Graphics/Stencil'
      ]},
      {'UI': [
        'features/UI/NezUI',
        'features/UI/DearImGui',
      ]},
      {
        "AI":[
          'features/AI/Behavior',
          'features/AI/Pathfinding'
        ]
      },
      {
        "Physics":[
          'features/Physics/NezPhysics',
          'features/Physics/Verlet',
          'features/Physics/FarseerPhysics'
        ]
      },
      {
        'ECS':[
          'features/ECS/SceneEntityComponentSystem',
          'features/ECS/SceneTransitions',
          'features/ECS/EntitySystems'
        ]
      },
    ]
  },
};
