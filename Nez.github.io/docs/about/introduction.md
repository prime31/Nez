---
id: introduction
title: Introduction
sidebar_label: Introduction
slug: /
---

Nez aims to be a feature-rich 2D framework that sits on top of MonoGame/FNA. It provides a solid base for you to build a 2D game on. Some of the many features it includes are:

- Scene/Entity/Component system with Component render layer tracking
- SpatialHash for super fast broadphase physics lookups. You won't ever see it since it works behind the scenes but you'll love it nonetheless since it makes finding everything in your proximity crazy fast via raycasts or overlap checks.
- AABB, circle and polygon collision/trigger detection
- Farseer Physics (based on Box2D) integration for when you need a full physics simulation
- efficient coroutines for breaking up large tasks across multiple frames or animation timing (Core.startCoroutine)
- in-game debug console extendable by adding an attribute to any static method. Just press the tilde key like in the old days with Quake. Out of the box, it includes a visual physics debugging system, asset tracker, basic profiler and more. Just type 'help' to see all the commands or type 'help COMMAND' to see specific hints.
- Dear ImGui in-game debug panels with the ability to wire up your own ImGui windows via attributes
- in-game Component inspector. Open the debug console and use the command `inspect ENTITY_NAME` to display and edit fields/properties and call methods with a button click.
- Nez.Persistence JSON, NSON (strongly typed, human readable JSON-like syntax) and binary serialization. JSON/NSON includes the ability to automatically resolve references and deal with polymorphic classes
- extensible rendering system. Add/remove Renderers and PostProcessors as needed. Renderables are sorted by render layer first then layer depth for maximum flexibility out of the box with the ability to add your own custom sorter.
- pathfinding support via Astar and Breadth First Search for tilemaps or your own custom format
- deferred lighting engine with normal map support and both runtime and offline normal map generation
- tween system. Tween any int/float/Vector/quaternion/color/rectangle field or property.
- sprites with sprite animations, scrolling sprites, repeating sprites and sprite trails
- flexible line renderer with configurable end caps including super smooth rounded edges or lightning bolt-like sharp edges
- powerful particle system with added support for importing Particle Designer files at runtime
- optimized event emitter (`Emitter` class) for core events that you can also add to any class of your own
- scheduler for delayed and repeating tasks (`Core.schedule` method)
- per-Scene content managers. Load your scene-specific content then forget about it. Nez will unload it for you when you change scenes.
- customizable Scene transition system with several built in transitions
- Verlet physics bodies for super fun, constraint-to-particle squishy physics
- tons more stuff