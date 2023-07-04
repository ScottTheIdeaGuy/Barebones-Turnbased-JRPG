using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;

namespace JRPG
{
	public enum Button { None = 0, Up = 1, Down = 2, Left = 3, Right = 4, Action = 5, Secondary = 6, Start = 7, Select = 8}
	public static class InputHandler
	{
		private static bool[] inputsHeld = new bool[9];
		private static bool[] inputsPressed = new bool[9];
		private static bool[] inputsReleased = new bool[9];

        private static Dictionary<Keys, Button> keyboardMapping;

		public static void Initialize()
		{
			inputsHeld[0] = true;
			for (int i = 1; i <= 8; i++)
				inputsHeld[i] = false;

            keyboardMapping = new Dictionary<Keys, Button>()		// the default mapping for keyboard input
			{
				{ Keys.Up, Button.Up },
				{ Keys.W, Button.Up },

				{ Keys.Down, Button.Down },
				{ Keys.S, Button.Down },

				{ Keys.Left, Button.Left },
				{ Keys.A, Button.Left },

				{ Keys.Right, Button.Right },
				{ Keys.D, Button.Right },

				{ Keys.Space, Button.Action },
				{ Keys.Z, Button.Action },
				{ Keys.J, Button.Action },

				{ Keys.X, Button.Secondary },
				{ Keys.K, Button.Secondary },
				{ Keys.LeftControl, Button.Secondary },
				{ Keys.RightControl, Button.Secondary },

				{ Keys.Enter, Button.Start },
				{ Keys.P, Button.Start},

				{ Keys.LeftShift, Button.Select},
				{ Keys.RightShift, Button.Select},
				{ Keys.Back, Button.Select}
			};
        }

		public static void Update() // not tested yet
		{
            bool[] previousInputsHeld = inputsHeld.Clone() as bool[];

			for (int i = 0; i < inputsHeld.Length; i++)
			{
				inputsHeld[i] = false;
			}

            foreach (Keys key in keyboardMapping.Keys)
			{
				int index = (int)keyboardMapping[key];
				if (!inputsHeld[index])
				{
					if (Keyboard.GetState().IsKeyDown(key))
						inputsHeld[index] = true;
                }
			}
			
			for (int i = 0; i < inputsHeld.Length; i++)
			{
				inputsPressed[i] = inputsHeld[i] && !previousInputsHeld[i];
                inputsReleased[i] = !inputsHeld[i] && previousInputsHeld[i];
			}
        }

		public static bool IsPressed(Button button, bool oneshot)
		{
			bool result = inputsPressed[(int)button];

			if (oneshot)	// if oneshot is true, make it so anything else that checks for inputPressed later this frame return false
				inputsPressed[(int)button] = false;

			return result;
		}
		public static bool IsHeld(Button button) => inputsHeld[(int)button];
		public static bool IsReleased(Button button) => inputsReleased[(int)button];
	}
}
