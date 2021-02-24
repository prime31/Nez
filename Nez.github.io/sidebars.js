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
      {'Graphics': [
        'features/Graphics/Rendering',
        'features/Graphics/DeferredLighting',
        'features/Graphics/SVG'
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
