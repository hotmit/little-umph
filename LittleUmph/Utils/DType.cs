using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Xml;
using System.ComponentModel;
using System.Reflection;
#if NET35_OR_GREATER
using System.Linq;
#endif

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
                        "M4A",
                        "M4B",
                        "MID",
                        "MP3",
                        "MPA",
                        "RA",
                        "WAV",
                        "WMA",
                        "FLA",
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
        /// <param name="file">The file.</param>
        /// <returns>
        ///   <c>true</c> if the specified video dir is video; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsVideo(string file)
        {
            return IsVideo(new FileInfo(file));
        }

        /// <summary>
        /// Determines whether the specified video dir is a video dir. (only based on the extension name)
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>
        ///   <c>true</c> if the specified video dir is video; otherwise, <c>false</c>.
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
        /// <param name="file">The file.</param>
        /// <returns>
        ///   <c>true</c> if the specified image dir is an image; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsImage(string file)
        {
            return IsImage(new FileInfo(file));
        }

        /// <summary>
        /// Determines whether the specified image dir is an image.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>
        ///   <c>true</c> if the specified image dir is an image; otherwise, <c>false</c>.
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
        /// <param name="file">The file.</param>
        /// <returns>
        ///   <c>true</c> if the specified ebook dir is ebook; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEbook(string file)
        {
            return IsImage(new FileInfo(file));
        }

        /// <summary>
        /// Determines whether the specified ebook dir is ebook.
        /// </summary>
        /// <param name="ebookFile">The ebook file.</param>
        /// <returns>
        ///   <c>true</c> if the specified ebook dir is ebook; otherwise, <c>false</c>.
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
        /// <param name="file">The file.</param>
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
        /// <param name="file">The file.</param>
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


        #region [ Casting & Converting ]
        /// <summary>
        /// Datatype that should not serialize
        /// </summary>
        private static List<Type> _simpleTypes = new List<Type>(new Type[] { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(float), typeof(double), typeof(decimal), typeof(bool), typeof(string), typeof(byte[]), typeof(Image), typeof(Bitmap), typeof(DateTime), typeof(TimeSpan) });
        private static List<Type> _binarySerializable = new List<Type>(new Type[] {  });

        /// <summary>
        /// Convert object to string representation.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns></returns>
        public static string ObjToString(object o)
        {
            string result = "";
            
            if (o is Image || o is Bitmap)
            {
                o = Img.ToBytes((Image)o);
            }
            else if (o is IEnumerable && !(o is string))
            {
                o = IOFunc.BinarySerialize(o);

                #region [ Old And Wonderfull Generic Methods - Keep here for References ]
                // Check for Dictionary<,>
                //typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>)



                //IDictionary dict = (IDictionary)o;
                //Type[] typeArgs = dict.GetType().GetGenericArguments();

                //MethodInfo method = typeof(DType).GetMethod("DictionaryToList");
                //MethodInfo gen = method.MakeGenericMethod(typeArgs);

                //o = gen.Invoke(null, new[] { dict });
                 
                //Create new generic dictionary
                //Type dictionary = typeof(Dictionary<,>);
                //Type[] typeArgs = dict.GetType().GetGenericArguments();

                //// Construct the type Dictionary<T1, T2>.
                //Type constructed = dictionary.MakeGenericType(typeArgs);
                //IDictionary newDictionary = (IDictionary)Activator.CreateInstance(constructed); 



                //Type[] args = typeof(T).GetGenericArguments();
                //MethodInfo method = typeof(Serializer).GetMethod("DeserializeXML");
                //MethodInfo gen = method.MakeGenericMethod(args);

                //IDictionary dict = (IDictionary)gen.Invoke(null, new[] { str });

                //MethodInfo list2dict = typeof(DType).GetMethod("ListToDictionary");
                //MethodInfo genList2dict = method.MakeGenericMethod(args);

                //return (T)gen.Invoke(null, new[] { dict });
                #endregion
            }


            if (o is DateTime)
            {
                DateTime dt = (DateTime)o;
                result = dt.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
            }
            else if (o is TimeSpan)
            {
                TimeSpan ts = (TimeSpan)o;
                result = ts.Ticks.ToString();
            }
            else if (o is byte[])
            {
                byte[] arr = (byte[])o;
                result = ByteArr.ToBase64(arr);
            }
            else if (o is Color)
            {
                Color c = (Color)o;
                result = ((int)((c.A << 24) | (c.R << 16) | (c.G << 8) | (c.B << 0))).ToString();
            }            
            else if (ShouldXmlSerialize(o.GetType()))
            {                
                XmlNode serializedNode = Serializer.SerializeXMLNode(o);
                result = serializedNode.OuterXml;
            }
            else
            {
                result = o.ToString();
            }

            return result;
        }


        /// <summary>
        /// Determin if the type should be xml serialize
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static bool ShouldXmlSerialize(Type type)
        {
            // if it is not serializable by the xmlserializer
            if (!type.IsSerializable)
            {
                return false;
            }

            // Don't serialize enum and don't serialize simple type
            // ToString() method can represent the data much cleaner
            if (typeof(Enum).IsAssignableFrom(type))
            {
                return false;
            }
            return !_simpleTypes.Contains(type);
        }

        /// <summary>
        /// Convert the string into the specified object.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="str">The string.</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static T Get<T>(string str, T valueOnError)
        {
            if (Str.IsEmpty(str))
            {
                return valueOnError;
            }
            else if (typeof(T) == typeof(DateTime))
            {
                DateTime dt;
                if (DateTime.TryParse(str, out dt))
                {
                    return (T)(object)(dt);
                }
                return valueOnError;
            }
            else if (typeof(T) == typeof(TimeSpan))
            {
                TimeSpan ts = new TimeSpan(Convert.ToInt64(str));
                return (T)(object)(ts);
            }
            else if (typeof(T) == typeof(byte[]))
            {
                byte[] b = ByteArr.FromBase64(str);
                if (b == null)
                {
                    return valueOnError;
                }
                return (T)(object)(b);
            }
            else if (typeof(Image).IsAssignableFrom(typeof(T)))
            {
                byte[] img = ByteArr.FromBase64(str);
                Image image = Img.FromBytes(img);

                return (T)(object)(image);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(typeof(T)) && typeof(T) != typeof(string))
            {                
                byte[] bytes = ByteArr.FromBase64(str);
                T dict = IOFunc.BinaryDeserialize<T>(bytes, valueOnError);
                return dict;
            }
            else if (typeof(T) == typeof(Color))
            {
                Color c = Color.FromArgb(Convert.ToInt32(str));
                return (T)(object)(c);
            }
            else if (typeof(Enum).IsAssignableFrom(typeof(T)))
            {
                object enu = Enum.Parse(typeof(T), str, true);
                return (T)(enu);
            }
            else if (ShouldXmlSerialize(typeof(T)))
            {
                return Serializer.DeserializeXML<T>(str);
            }

            T result = DType.Cast<T>(str, valueOnError);
            return result;
        }

        /// <summary>
        /// Casts the specified object into type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o">The object.</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static T Cast<T>(object o, T valueOnError)
        {
            #region [ Explicit & Implicit Conversion ]
            try
            {
                MethodInfo explitcit = GetExplitcit(o, typeof(T));
                if (explitcit != null)
                {
                    return (T)explitcit.Invoke(null, new[] { o });
                }

                MethodInfo implitcit = GetImplitcit(o, typeof(T));
                if (implitcit != null)
                {
                    return (T)implitcit.Invoke(null, new[] { o });
                }
            }
            catch (Exception xpt)
            {

            }
            #endregion

            #region [ Convert Data Type IConvertable ]
            try
            {
                T result = (T)Convert.ChangeType(o, typeof(T));
                return result;
            }
            catch (Exception xpt)
            {

            }
            #endregion

            #region [ TypeConverter ]
            try
            {
                // Type with TypeConverter implementation   Convert T From String
                TypeConverter destType = TypeDescriptor.GetConverter(typeof(T));
                if (destType != null && destType.CanConvertFrom(o.GetType()))
                {
                    object convertFrom = destType.ConvertFrom(o.GetType());
                    return (T)convertFrom;
                }

                // Type with TypeConverter implementation   Convert String to T
                TypeConverter srcType = TypeDescriptor.GetConverter(o.GetType());
                if (srcType != null && srcType.CanConvertTo(typeof(T)))
                {
                    object convertTo = srcType.ConvertTo(o.GetType(), typeof(T));
                    return (T)convertTo;
                }
            }
            catch (Exception xpt)
            {

            }
            #endregion

            return valueOnError;
        }
        #endregion

        #region [ Get Explicit & Implicit Methods ]
        /// <summary>
        /// Gets the explitcit cast method.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="destination">The destination.</param>
        /// <returns></returns>
        public static MethodInfo GetExplitcit(object o, Type destination)
        {
            return GetMethod(o, destination, "op_Explicit");
        }

        /// <summary>
        /// Gets the implitcit cast method
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="destination">The destination.</param>
        /// <returns></returns>
        public static MethodInfo GetImplitcit(object o, Type destination)
        {
            return GetMethod(o, destination, "op_Implicit");
        }

        private static MethodInfo GetMethod(object o, Type destinationType, string methodName)
        {
            Type srcType = o.GetType();
            var methods = destinationType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (method.Name == methodName)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 1 &&
                        parameters[0].ParameterType == o.GetType())
                    {
                        return method;
                    }
                }
            }
            return null;
        }
        #endregion
    }
}


#region [ Audio & Video Extensions ]
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
#endregion