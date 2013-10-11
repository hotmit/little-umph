using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace LittleUmph
{
    #region [ ControlLocation Enum ]
    /// <summary>
    /// The location on the control.
    /// </summary>
    public enum ControlLocation
    {
        /// <summary>
        /// Top left corner
        /// </summary>
        TopLeft,
        /// <summary>
        /// Top right corner
        /// </summary>
        TopRight,
        /// <summary>
        /// Bottom left corner
        /// </summary>
        BottomLeft,
        /// <summary>
        /// Bottom right corner
        /// </summary>
        BottomRight,
        /// <summary>
        /// Center point
        /// </summary>
        Center
    }
    #endregion

    /// <summary>
    /// Helper to assist with layout form and control.
    /// </summary>
    public class Layout
    {
        /// <summary>
        /// Center the form to the primary screen
        /// </summary>
        /// <param name="form"></param>
        public static void CenterScreen(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            Screen src = Screen.PrimaryScreen;
            form.Left = (src.Bounds.Width - form.Width) / 2;
            form.Top = (src.Bounds.Height - form.Height) / 2;
        }

        /// <summary>
        /// Center the childForm to the parentForm
        /// </summary>
        /// <param name="parentForm">The parent form.</param>
        /// <param name="childForm">The child form.</param>
        public static void CenterForm(Form parentForm, Form childForm)
        {
            childForm.Location = GetAbsoluteCenter(parentForm, childForm);
        }

        /// <summary>
        /// Add child to the PARENT of frame and make it so the child is on top and in the center of frame.
        /// The child control is not inside of the frame
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="child">The child.</param>
        /// <returns></returns>
        public static bool CenterControl(Control frame, Control child)
        {
            Control parent = frame.Parent;
            if (parent == null)
            {
                return false;
            }
            parent.SuspendLayout();

            if (!parent.Controls.Contains(child))
            {
                parent.Controls.Add(child);
            }

            child.Location = GetRelativeCenter(frame, child);

            ////Todo: find a better way to move the child in front of frame
            child.BringToFront();

            parent.ResumeLayout();
            parent.Refresh();

            return true;
        }

        /// <summary>
        /// Add the child control to the frame and center it (child is inside of the parent).
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="child">The child.</param>
        public static void AddToCenter(Control frame, Control child)
        {
            frame.SuspendLayout();
            if (!frame.Controls.Contains(child))
            {
                frame.Controls.Add(child);
            }

            Point center = GetRelativeCenter(frame, child);

            ////Todo: find the proper way to compensate for the titlebar and the frame
            // [-30] Roughly compensate for the title bar
            int formCompensator = frame is Form ? 30 : 0;

            child.Location = new Point(center.X, center.Y - formCompensator);

            frame.ResumeLayout();
            frame.Refresh();
        }

        /// <summary>
        /// Gets the point location on a control
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public static Point GetLocation(Control control, ControlLocation location)
        {
            return GetLocation(control.Bounds, location);
        }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public static Point GetLocation(Rectangle rectangle, ControlLocation location)
        {
            switch (location)
            {
                case ControlLocation.TopLeft:
                    return rectangle.Location;
                case ControlLocation.TopRight:
                    return new Point(rectangle.Location.X + rectangle.Width, rectangle.Location.Y);
                case ControlLocation.BottomLeft:
                    return new Point(rectangle.Location.X, rectangle.Location.Y + rectangle.Height);
                case ControlLocation.BottomRight:
                    return new Point(rectangle.Location.X + rectangle.Width, rectangle.Location.Y + rectangle.Height);
                case ControlLocation.Center:
                    return new Point(rectangle.Location.X + (rectangle.Width / 2), rectangle.Location.Y + (rectangle.Height / 2));
                default:
                    return Point.Empty;
            }
        }

        /// <summary>
        /// Get the position where the c2 is in the center of the c2.
        /// The position of c2 is NOT relative to the c1 (ie c2 is not inside of c2). 
        /// </summary>
        /// <param name="c1">The reference control.</param>
        /// <param name="c2">The control you want to center.</param>
        /// <returns></returns>
        public static Point GetAbsoluteCenter(Control c1, Control c2)
        {
            Point center = new Point(
                                c1.Location.X + (c1.Width - c2.Width) / 2,
                                c1.Location.Y + (c1.Height - c2.Height) / 2
                            );
            return center;
        }

        /// <summary>
        /// Gets coordinate where inner is in the center of r1.
        /// </summary>
        /// <param name="r1">The r1.</param>
        /// <param name="inner">The inner.</param>
        /// <returns></returns>
        public static Point GetAbsoluteCenter(Rectangle r1, Size inner)
        {
            Point center = new Point(
                                r1.Location.X + (r1.Width - inner.Width) / 2,
                                r1.Location.Y + (r1.Height - inner.Height) / 2
                            );
            return center;
        }

        /// <summary>
        /// Get the center location of the child inside of the parent control
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="child">The child.</param>
        /// <returns></returns>
        public static Point GetRelativeCenter(Control parent, Control child)
        {
            Point center = new Point(
                                (parent.Width - child.Width) / 2,
                                (parent.Height - child.Height) / 2
                            );
            return center;
        }

        /// <summary>
        /// Gets the distance between the two points (Pythagorean theorem).
        /// </summary>
        /// <param name="a">Point A.</param>
        /// <param name="b">Point B.</param>
        /// <returns></returns>
        public static double GetDistance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
    }
}
