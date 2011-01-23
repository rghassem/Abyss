using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abyss.Code.Screen
{
    /// <summary>
    /// The state of the screen itself
    /// </summary>
    public enum State
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden,
    }

    public abstract class Screen
    {
        public bool Popup { get; set; }
        public TimeSpan TransitionOnTime { get; set; }
        public TimeSpan TransitionOffTime { get; set; }
        public float TransitionPosition { get; set; }
        public byte TransitionAlpha { get; set; }
        public State State { get; set; }
        public bool IsActive
        {
            get
            {
                return !otherScreenHasFocus && (State == State.Active || State == State.TransitionOn);
            }
        }
        bool otherScreenHasFocus;

        ScreenManager manager;
        public ScreenManager Manager
        {
            get { return manager; }
            internal set { manager = value; }
        }
    }
}
