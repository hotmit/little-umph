using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;

namespace LittleUmph
{
    /// <summary>
    /// Functions to deal with delegate.
    /// </summary>
    public class Dlgt
    {
        #region [ Invoke ]
        /// <summary>
        /// Invoke an event in a thread safe manner when it being called from a form.
        /// If it called from a difference place, the normal invoke is called (not thread-safe).
        /// </summary>
        /// <param name="theEvent">The event.</param>
        /// <param name="args">The args.</param>
        /// <returns>Return true if the delegate is invoked successfully.</returns>
        public static bool ThreadSafeInvoke(Delegate theEvent, params object[] args)
        {
            return Invoke(true, theEvent, args);
        }

        /// <summary>
        /// Invokes the specified event without .
        /// </summary>
        /// <param name="theEvent">The event.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static bool Invoke(Delegate theEvent, params object[] args)
        {
            return Invoke(false, theEvent, args);
        }

        /// <summary>
        /// Invokes the event.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> to invoke the delegate in thread safe manager.</param>
        /// <param name="theEvent">The event.</param>
        /// <param name="args">The args.</param>
        /// <returns>Return true if the delegate is invoked successfully.</returns>
        public static bool Invoke(bool threadSafe, Delegate theEvent, params object[] args)
        {
            try
            {
                if (theEvent == null)
                {
                    return true;
                }

                bool noError = true;

                foreach (Delegate singleCast in theEvent.GetInvocationList())
                {
                    try
                    {
                        ISynchronizeInvoke syncInvoke = singleCast.Target as ISynchronizeInvoke;

                        if (syncInvoke != null && syncInvoke.InvokeRequired)
                        {
                            syncInvoke.Invoke(theEvent, args);
                        }
                        else
                        {
                            singleCast.DynamicInvoke(args);
                        }
                    }
                    catch (Exception xpt)
                    {
                        Gs.Log.Error("Dlgt.BeginInvoke()", xpt.Message);
                        noError = false;
                    }
                }
                return noError;
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("Dlgt.BeginInvoke()", xpt.Message);
                return false;
            }
        }
        #endregion


        /// <summary>
        /// Invokes the specified method on the control c (Threadsafe).
        /// </summary>
        /// <param name="c">The control to invoke on (can be a form).</param>
        /// <param name="method">The method.</param>
        public static void Invoke(Control c, Action method)
        {
            if (c.InvokeRequired)
            {
                c.Invoke(new MethodInvoker(method));
                return;
            }

            method.Invoke();
        }
    }
}
