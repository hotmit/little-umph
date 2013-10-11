using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LittleUmph
{
    /// <summary>
    /// Determine the type of dir.
    /// </summary>
    public class DType
    {
        #region [ Private Variables ]
        private static List<string> _videoExtList;
        private static List<string> _imageExtList;
        private static List<string> _ebookExtList;
        private static List<string> _audioExtList;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets the video extension list.
        /// </summary>
        /// <value>The video oldExt list.</value>
        public static List<string> VideoExtList
        {
            get
            {
                #region [ Video Extensions List Declaration ]
                if (_videoExtList == null)
                {
                    _videoExtList = new List<string>();
                    _videoExtList.AddRange(new[] { 
                        "AAF", // mostly intended to hold edit decisions and rendering information, but can also contain compressed media essence
                        "3GP", // the most common video format for cell phones
                        "GIF", // Animated GIF (simple animation; until recently often avoided because of patent problems
                        "ASF", // ASF is a shell, which enables any form of compression to be used; MPEG-4 is common. Video in ASF-containers is also called Windows Media Video (WMV)
                        "AVCHD", // - Advanced Video Codec High Definition
                        "AVI", // (AVI is a shell, which enables any form of compression to be used;
                        "CAM", // An aMSN webcam log dir.
                        "DAT", // DAT is a Video standard data dir; Automatically created when we attempted to burn as video dir on the CD)
                        "DSH", 
                        "FLV", // (*.flv) A video dir encoded to run in a flash animation.
                        "M1V", // MPEG-1 Video dir
                        "M2V", // MPEG-2 Video dir
                        "SWF", // Macromedia Flash (.swf for viewing,
                        "FLA", // Macromedia Flash for producing)
                        "FLR", // A text dir which contains scripts extracted from SWF by a free ActionScript decompiler named FLARE
                        "MKV", // Matroska (*.mkv) (Matroska is a container format, which enables any video format such as MPEG-4 or DivX/XviD to be used along with other content such as subtitles and detailed meta information)
                        "WRAP", // MediaForge (*.wrap)
                        "MNG", // (mainly simple animation containing PNG and JPEG objects, often somewhat more complex than animated GIF)
                        "MOV", // (QuickTime, a container format, which enables any form of compression to be used; Sorenson codec is the most common. QTCH is the filetype for cached video and audio streams.)
                        "MP4", //-4, shortened "MP4", a popular video format most often used for Sony's PlayStation Portable and Apple's iPod.
                        "MPEG", // (.mpeg, .mpg, .mpe)
                        "MPG",
                        "MPE",
                        "MXF", // Material Exchange Format is a standardized wrapper format for audio/visual material developed by SMPTE
                        "NSV", // Nullsoft Streaming Video is a media container designed for streaming video content over the internet.
                        "OGM", // (OGM is a container format created so that Ogg Vorbis could be used for the audio of a video as this could not be done with AVI)
                        "TARKIN", // (Ogg project, all Tarkin files are Ogg files)
                        "TTHEORA", // (Ogg project, all Theora files are Ogg files)
                        "RM", // RealMedia
                        "SVI", // Samsung video format for portable players
                        "SMI", // SAMI Caption dir. (HTML like subtitle for movie files)
                        "WMV", // Windows Media Video (See ASF)
                        "DIVX", // Most commonly using the .avi or the .divx container
                        "XVID", // Most commonly using the .avi container
                        "RMVB"
                    });
                }
                #endregion

                return _videoExtList;
            }
        }

        /// <summary>
        /// Gets the image extension list.
        /// </summary>
        /// <value>The image oldExt list.</value>
        public static List<string> ImageExtList
        {
            get
            {
                #region [ Image Extensions List Declaration ]
                if (_imageExtList == null)
                {
                    _imageExtList = new List<string>();
                    _imageExtList.AddRange(new[]{
                        "JPG",
                        "JPEG",
                        "BMP",
                        "PNG",
                        "GIF",
                        "TIFF",
                        "TIF",
                        "RAW",

                        "PPM",
                        "PGM",
                        "PBM",
                        "PNM",
         
                        "PSD",
                        "CGM",
                        "SVG",
                        "WMF",
                        "EMF"
                    });
                }
                #endregion

                return _imageExtList;
            }
        }

        /// <summary>
        /// Gets the eBook extension list.
        /// </summary>
        /// <value>The image oldExt list.</value>
        public static List<string> EbookExtList
        {
            get
            {
                #region [ Ebook Extensions List Declaration ]
                if (_ebookExtList == null)
                {
                    _ebookExtList = new List<string>();
                    _ebookExtList.AddRange(new[]{
                        "PDF",
                        "CHM",
                        "AZW",
                        "AZW1",
                        "AZW4",
                        "EPUB",
                        "KF8",
                        "MOBI",
                        "PDB",
                        "PRC",
                        "TPZ"
                     });
                }
                #endregion

                return _ebookExtList;
            }
        }

        /// <summary>
        /// Gets the audio extension list.
        /// </summary>
        /// <value>The image oldExt list.</value>
        public static List<string> AudioExtList
        {
            get
            {
                #region [ Audio Extensions List Declaration ]
                if (_audioExtList == null)
                {
                    _audioExtList = new List<string>();
                    _audioExtList.AddRange(new[]{
                        "AIF",
                        "IFF",
                        "M3U",
                        "M4A",
                        "MID",
                        "MP3",
                        "MPA",
                        "RA",
                        "WAV",
                        "WMA",
                        "FLAC",
                        "APE"
                    });
                }
                #endregion

                return _audioExtList;
            }
        }
        #endregion

        #region [ Static Constructor ]
        /// <summary>
        /// Initializes the <see cref="DType"/> class.
        /// </summary>
        static DType()
        {
            
        }
        #endregion

        #region [ IsVideo() ]
        /// <summary>
        /// Determines whether the specified video dir is video.
        /// </summary>
        /// <param name="dir">The video dir.</param>
        /// <returns>
        /// 	<c>true</c> if the specified video dir is video; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsVideo(string file)
        {
            return IsVideo(new FileInfo(file));
        }

        /// <summary>
        /// Determines whether the specified video dir is a video dir. (only based on the extension name)
        /// </summary>
        /// <param name="dir">The video dir.</param>
        /// <returns>
        /// 	<c>true</c> if the specified video dir is video; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsVideo(FileInfo file)
        {           
            string ext = file.Extension.ToUpper().Substring(1);
            return VideoExtList.Contains(ext);
        }
        #endregion

        #region [ IsImage() ]
        /// <summary>
        /// Determines whether the specified image dir is an image.
        /// </summary>
        /// <param name="dir">The image dir.</param>
        /// <returns>
        /// 	<c>true</c> if the specified image dir is an image; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsImage(string file)
        {
            return IsImage(new FileInfo(file));
        }

        /// <summary>
        /// Determines whether the specified image dir is an image.
        /// </summary>
        /// <param name="dir">The image dir.</param>
        /// <returns>
        /// 	<c>true</c> if the specified image dir is an image; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsImage(FileInfo file)
        {
            string ext = file.Extension.ToUpper().Substring(1);
            return ImageExtList.Contains(ext);
        } 
        #endregion

        #region [ IsEbook() ]
        /// <summary>
        /// Determines whether the specified ebook dir is ebook.
        /// </summary>
        /// <param name="dir">The ebook dir.</param>
        /// <returns>
        /// 	<c>true</c> if the specified ebook dir is ebook; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEbook(string file)
        {
            return IsImage(new FileInfo(file));
        }

        /// <summary>
        /// Determines whether the specified ebook dir is ebook.
        /// </summary>
        /// <param name="dir">The ebook dir.</param>
        /// <returns>
        /// 	<c>true</c> if the specified ebook dir is ebook; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEbook(FileInfo ebookFile)
        {
            string ext = ebookFile.Extension.ToUpper().Substring(1);
            return EbookExtList.Contains(ext);
        }
        #endregion

        #region [ IsAudio() ]
        /// <summary>
        /// Determines whether the specified dir is audio.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>
        ///   <c>true</c> if the specified dir is audio; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAudio(string file)
        {
            return IsAudio(new FileInfo(file));
        }

        /// <summary>
        /// Determines whether the specified dir is audio.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>
        ///   <c>true</c> if the specified dir is audio; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAudio(FileInfo file)
        {
            try
            {
                string ext = file.Extension.ToUpper().Substring(1);
                return AudioExtList.Contains(ext);
            }
            catch (Exception xpt)
            {
                return false;
            }
        }
        #endregion
    }
}


/*
What video or subtitles extensions do you know?

REASON WHY I ASKING: I want to make an extension filter to my video converter application; for example to when you insert .doc then will ignore it

i know video extensions:
avi
mp4
m4v
264
divx
xvid
mkv
mpg
mpeg
flv
3gp
no extension (this sometimes happen when you dowload it from youtube.com)
vob (used at DVDs)
ifo (used at DVDs)
bup (used at DVDs)
ts (digital television "transport stream")
mov
qt
asf
wmv
wm
ogm
rm
ram
rmvb
mjpeg
mjpg
dvr-ms (recordings from Microsoft`s video recorders)
swf
ivf (Indeo Video File)
3mm (Microsoft 3D Movie Maker video dir)
flc
bik
bay (Kodak/Roper Bayer Picture Sequence)
mp2v



and subtitles:
srt
sub
ssa
txt

and this subtitles types have found in mencoder help dir:
(but i didnt found extension to them)

OGM
CC (closed caption) / most probably extension is cc
SubRip
SubViewer
Sami
VPlayer
RT
PJS (Phoenix Japanimation Society)
MPsub
AQTitle
JACOsub

Do you know more video/subtitles extensions?

Here are the extentions for the subtitle types you've listed:

OGM
CC (closed caption) / most probably extension is cc
SubRip = .srt
SubViewer = .sub
Sami (Sami Captioning) = .smi or .sami
VPlayer = .txt (Arbitrary)
RT Real Text = .rt
PJS (Phoenix Japanimation Society) = .pjs
MPsub = .sub
AQTitle = .aqt
JACOsub = .js or .jss


I have also found these extentions used for various subtitle files:

 
 
AQTitle = .aqt
Advanced SubStation Alpha = .ass
Captions DAT = .dat
Cheetah = .asc
DKS Subtitle Format = .dks
Karaoke Lyrics = .lrc
Karaoke Lyrics VKT = .vkt
MacSUB = .scr
MPEG-4 Timed Text = .ttxt
MPlayer = .mpl
OVR Script = .ovr
Panimator = .pan
Phoenix Subtitle = .pjs
PowerDivX = .psb
Sasami Script = .s2k
SBT = .sbt
Softitler RTF = .rtf
Sonic Scenartist = .sst
Stream Sub Text Player = .sst
Stream Sub Text Script = .ssts
Turbo Titler = .tts
ViPlay Subtitle File = .vsf
ZeroG = .zeg
Subtitle Report File = .srf
VobSub = .sub + .idx
 
 
 
 
 
 
 
 
 WOW!!!

thanks Snoopyboy

this subtitles extensions cant be added because arent supported in Menocoder:
Captions DAT = .dat
Cheetah = .asc
DKS Subtitle Format = .dks
Karaoke Lyrics = .lrc
Karaoke Lyrics VKT = .vkt
MacSUB = .scr
MPEG-4 Timed Text = .ttxt
MPlayer = .mpl
OVR Script = .ovr
Panimator = .pan
Phoenix Subtitle = .pjs
PowerDivX = .psb
Sasami Script = .s2k
SBT = .sbt
Softitler RTF = .rtf
Sonic Scenartist = .sst
Stream Sub Text Player = .sst
Stream Sub Text Script = .ssts
Turbo Titler = .tts
ViPlay Subtitle File = .vsf
ZeroG = .zeg
Subtitle Report File = .srf
 
 
 
 
 .aif	Audio Interchange File Format
.iff	Interchange File Format
.m3u	Media Playlist File
.m4a	MPEG-4 Audio File
.mid	MIDI File
.mp3	MP3 Audio File
.mpa	MPEG-2 Audio File
.ra	Real Audio File
.wav	WAVE Audio File
.wma	Windows Media Audio File
 
 
 
 

 

 
Main formats

These are the formats most commonly available commercially.

    AZW - An Amazon proprietary format. This is very close to the MOBI format sometimes with and sometimes without DRM. The DRM is unique to the Amazon Kindle.
    AZW1 - An Amazon proprietary format. It is the TPZ format always with a custom DRM.
    AZW4 - An Amazon proprietary format. It is the PDF format in a PDB wrapper, and usually (always?) with DRM.
    EPUB An open format defined by the Open eBook Forum of the International Digital Publishing Forum (<idpf>). It is based on XHTML and XML. It is an evolving standard. Current specifications are found at the idpf web site. Adobe, Barnes & Noble and Apple all have their own (incompatible) DRM systems for this format. There is now a new version of this format called ePub 3 but it is not in wide use.
    KF8 - Kindle Fire format from Amazon. It is basically ePub compiled in the PDB wrapper with Amazon DRM. This format is expected to roll out for other Amazon readers.
    MOBI - MobiPocket format, usable with MobiPocket's own reading software on almost any PDA and Smartphones. Mobipocket's Windows PC software can convert .chm, .doc, .html, .ocf, .pdf, .rtf, and .txt files to this format. Kindle uses this format, as well.
    PDB - Palm Database File. Can hold several different e-book formats targeting Palm-enabled devices, commonly used for PalmDOC (AportisDoc) e-books and eReader formats as well and many others.
    PDF - Portable Document Format created by Adobe for their Acrobat products. It is the defacto standard for document interchange. Software support exists for almost every computer platform and handheld device. Some devices have problems with PDF since most content available is scaled for either A4 or letter format, both of which are not easily readable when reduced to fit on small screens. Some Readers can reflow some PDF documents, including the Sony PRS505, to accommodate the small screen. Some eBook readers, including the iRex iLiad, have a pan-and-zoom feature that aids readability, but extracts a price in ergonomics.
    PRC - Palm Resource File. Often holds a Mobipocket eBook but occasionally holds an eReader or AportisDoc eBook.
    TPZ - Topaz dir extension used on Amazon Kindle. Topaz is a collection of glyphs arrange on pages, along with an unproofed OCR text version. An Amazon proprietary format, used to make older books available quickly, since conversion is essentially automatic from scans of the pages of a book, but it reflows very well.  
 
 */