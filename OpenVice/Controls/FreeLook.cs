using OpenTK;
using OpenVice.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Controls {

	/// <summary>
	/// Internal FreeLook controls<para/>
	/// Внутреннее свободное управление
	/// </summary>
	public static class FreeLook {

		static float viewX=0f, viewY=0f;
		
		public static void Control()
		{
			float spdx = (float)Input.MouseSpeed.X*0.2f;
			float spdy = (float)Input.MouseSpeed.Y*0.2f;
			
			viewX+=spdx;
			viewY+=spdy;
			if (Math.Abs(viewY)>90f) {
				viewY = 90f*Math.Sign(viewY);
			}
			
			Vector3 movement = Vector3.Zero;
			if (Input.KeyDown(Key.W)) {
				movement.Z = 1f;
			}else if(Input.KeyDown(Key.S)){
				movement.Z = -1f;
			}
			if (Input.KeyDown(Key.D)) {
				movement.X = 1f;
			}else if(Input.KeyDown(Key.A)){
				movement.X = -1f;
			}
			if (!Input.KeyDown(Key.ShiftLeft) && !Input.KeyDown(Key.ShiftRight)) {
				movement *= 0.1f;
			}

			Camera.Angles = new Vector3(viewY, viewX, 0);
			Camera.Position += Camera.TransformDirection(movement)*5f;
		}


	}
}
