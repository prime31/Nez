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

namespace Nez.Overlap2D.Runtime
{
	public class PhysicsBodyDataVO
	{
		public int bodyType = 0;

		public float mass;
		public Vector2 centerOfMass;
		public float rotationalInertia;
		public float damping;
		public float gravityScale;
		public bool allowSleep;
		public bool awake;
		public bool bullet;
		public bool sensor;

		public float density;
		public float friction;
		public float restitution;

		public PhysicsBodyDataVO(){
			centerOfMass = new Vector2();
		}

		public PhysicsBodyDataVO(PhysicsBodyDataVO vo){
			bodyType = vo.bodyType;
			mass = vo.mass;
			centerOfMass = vo.centerOfMass;
			rotationalInertia = vo.rotationalInertia;
			damping = vo.damping;
			gravityScale = vo.gravityScale;
			allowSleep = vo.allowSleep;
			sensor = vo.sensor;
			awake = vo.awake;
			bullet = vo.bullet;
			density = vo.density;
			friction = vo.friction;
			restitution = vo.restitution;
		}

		/*
		public void loadFromComponent(PhysicsBodyComponent physicsComponent) {
			bodyType = physicsComponent.bodyType;
			mass = physicsComponent.mass;
			centerOfMass = physicsComponent.centerOfMass.cpy();
			rotationalInertia = physicsComponent.rotationalInertia;
			damping = physicsComponent.damping;
			gravityScale = physicsComponent.gravityScale;
			allowSleep = physicsComponent.allowSleep;
			sensor = physicsComponent.sensor;
			awake = physicsComponent.awake;
			bullet = physicsComponent.bullet;
			density = physicsComponent.density;
			friction = physicsComponent.friction;
			restitution = physicsComponent.restitution;
		}*/
	}
}

