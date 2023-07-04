using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRPG
{
    public class BattleBeat
    {
        public Action effect;
        public string dialog;
        public float duration;

        private double timeStamp;
        public bool Started { get; private set; }

        public BattleBeat(string dialog, Action effect = null)
        {
            this.effect = effect;
            this.dialog = dialog;
            this.duration = 0;
            this.timeStamp = -1;

            Started = false;
        }

        public void Start(GameTime gameTime, float duration)
        {
            this.duration = duration;
            Started = true;
            timeStamp = gameTime.TotalGameTime.TotalSeconds;
        }

        public bool IsComplete(GameTime gameTime)
        {
            bool durationCheck = false;

            if (Started && duration > 0)
                durationCheck = gameTime.TotalGameTime.TotalSeconds - timeStamp >= duration;

            return durationCheck;
        }
    }
}
