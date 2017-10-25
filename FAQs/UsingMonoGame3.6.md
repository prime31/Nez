
Using MonoGame 3.6
==========

Nez works fairly well on newer versions of MonoGame, such as 3.6, though it has not been extensively tested at this time with these versions.
Some minor tweaks are necessary to get it working with newer versions of MonoGame.

These instructions are for [MonoGame 3.6.0.1625](https://www.nuget.org/packages/MonoGame.Framework.Portable/3.6.0.1625) on NuGet.

- Rebuild the effects (`.fx` files in `DefaultContentSource/effects`) to `.mgfxo` and replace those in `DefaultContent`
- In `Nez.Portable/Core.cs`, add a `ulong` cast to the line `drawCalls = graphicsDevice.Metrics.DrawCount;`
- In `Nez.Portable/Graphics/Effects/WaterReflectionEffect.cs`, change the return type of `OnApply` to `void` and remove the `return false;`
- Update the references to the desired version of `MonoGame.Framework` as needed.
- Rebuild your project, and Nez should now be running on MonoGame 3.6
