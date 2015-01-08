using System;
using System.Collections.Generic;
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// Ascii helper class.
    /// </summary>
    public class Ascii
    {
        /// <summary>
        /// The caret return
        /// </summary>
        public const int CaretReturn = 13;
        /// <summary>
        /// The new line
        /// </summary>
        public const int NewLine = 10;
        /// <summary>
        /// The space
        /// </summary>
        public const int Space = 32;

        /// <summary>
        /// Only for KeyUp and KeyDown (no KeyPress)
        /// </summary>
        public const int Backspace = 8;


        /// <summary>
        /// Don't use this, use KeyEventArgs.KeyCode instead
        /// </summary>
        public struct KeyDown {}

        ///<summary>
        /// Common KeyPress values.
        ///</summary>
        public struct KeyPress
        {
            public const int MinusSign = 45;
            public const int PlusSign = 43;
        }

        /// <summary>
        /// Don't use this, use KeyEventArgs.KeyCode instead
        /// </summary>
        public struct KeyUp {}

    }
}
