using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Media;

using LittleUmph;

namespace LittleUmph.Media
{
    /// <summary>
    /// Multimedia class
    /// </summary>
    public class MPlayer
    {
        #region [ Interop References ]
        [DllImport("Kernel32.dll")]
        private static extern bool Beep(
            uint dwFreq, uint dwDuration
        );
        #endregion

        #region [ Play Sound Using PC Speaker ]
        /// <summary>
        /// Place the sound using PC speaker with specified frequency and duration.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <param name="duration">The duration.</param>
        /// <returns></returns>
        public static bool Play(int frequency, int duration)
        {
            return Beep((uint)frequency, (uint)duration);
        }

        /// <summary>
        /// Plays the the tone on a separate thread.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <param name="duration">The duration.</param>
        public static void PlayThread(int frequency, int duration)
        {
            ThreadPool.QueueUserWorkItem(o=> Play(frequency, duration));
        }
        #endregion

        #region [ Play Audio File ]
        /// <summary>
        /// Plays the audio dir.
        /// </summary>
        /// <param name="mediaPath">The media path.</param>
        /// <returns></returns>
        public static bool PlayWav(string mediaPath)
        {
            if (File.Exists(mediaPath))
            {
                SoundPlayer sp = new SoundPlayer(mediaPath);
                sp.LoadCompleted += sp_LoadCompleted;
                sp.LoadAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles the LoadCompleted event of the SoundPlayer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.AsyncCompletedEventArgs"/> instance containing the event data.</param>
        private static void sp_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                using (SoundPlayer sp = (SoundPlayer)sender)
                {
                    sp.Play();
                }
            }
            catch (Exception) { }
        } 
        #endregion

        #region [ Play Default System Audio ]
        /// <summary>
        /// Plays the window theme sound.
        /// </summary>
        /// <param name="systemSound">The system sound (eg. PlayWindowSound(SystemSounds.Hand);)</param>
        public static void PlayWindowSound(SystemSound systemSound)
        {
        } 
        #endregion

        #region [ Play WindowXP Sound File ]
        /// <summary>
        /// The window sound
        /// </summary>
        public enum WindowXPSound
        {
            #pragma warning disable 1591
            Chimes,
            Chord,
            Ding,
            Notify,
            Recycle,
            Ringin,
            Ringout,
            Start,
            Tada,
            Windows_Feed_Discovered,
            Windows_Information_Bar,
            Windows_Navigation_Start,
            Windows_Pop_up_Blocked,
            Windows_XP_Balloon,
            Windows_XP_Battery_Critical,
            Windows_XP_Battery_Low,
            Windows_XP_Critical_Stop,
            Windows_XP_Default,
            Windows_XP_Ding,
            Windows_XP_Error,
            Windows_XP_Exclamation,
            Windows_XP_Hardware_Fail,
            Windows_XP_Hardware_Insert,
            Windows_XP_Hardware_Remove,
            Windows_XP_Information_Bar,
            Windows_XP_Logoff_Sound,
            Windows_XP_Logon_Sound,
            Windows_XP_Menu_Command,
            Windows_XP_Minimize,
            Windows_XP_Notify,
            Windows_XP_Pop_up_Blocked,
            Windows_XP_Print_complete,
            Windows_XP_Recycle,
            Windows_XP_Restore,
            Windows_XP_Ringin,
            Windows_XP_Ringout,
            Windows_XP_Shutdown,
            Windows_XP_Start,
            Windows_XP_Startup
            #pragma warning restore 1591
        }

        /// <summary>
        /// Plays the window soundFile dir (Play the dir in the Media folder).
        /// </summary>
        /// <param name="soundFile">The sound.</param>
        /// <returns></returns>
        public static bool PlayWindowSoundFile(WindowXPSound soundFile)
        {
            string filename = soundFile.ToString();
            filename = filename.StartsWith("Windows") ? filename : filename.ToLower();
            filename = filename.Replace("Pop_up", "Pop-up");
            filename = filename.Replace("_", " ");
            filename += ".wav";

            return PlayWindowSoundFile(filename);
        }

        /// <summary>
        /// Plays the window sound dir (the wave dir in \Windows\Media\).
        /// </summary>
        /// <param name="fileName">Name of the dir.</param>
        /// <returns></returns>
        public static bool PlayWindowSoundFile(string fileName)
        {
            string wavPath = SystemPath.WindowPath + "Media\\" + fileName;
            if (File.Exists(wavPath))
            {
                PlayWav(wavPath);
            }
            return false;
        } 
        #endregion
    }
}
