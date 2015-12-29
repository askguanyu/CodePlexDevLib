//-----------------------------------------------------------------------
// <copyright file="MimeMapping.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost40
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// This class maps document extensions to Content Mime Type.
    /// </summary>
    public static class MimeMapping
    {
        /// <summary>
        /// Field MimeMappingDictionary.
        /// </summary>
        private static readonly Dictionary<string, string> MimeMappingDictionary;

        /// <summary>
        /// Field PathSeparatorChars.
        /// </summary>
        private static readonly char[] PathSeparatorChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar };

        /// <summary>
        /// Initializes static members of the <see cref="MimeMapping" /> class.
        /// </summary>
        static MimeMapping()
        {
            MimeMappingDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            MimeMappingDictionary.Add(".323", "text/h323");
            MimeMappingDictionary.Add(".aaf", "application/octet-stream");
            MimeMappingDictionary.Add(".aca", "application/octet-stream");
            MimeMappingDictionary.Add(".accdb", "application/msaccess");
            MimeMappingDictionary.Add(".accde", "application/msaccess");
            MimeMappingDictionary.Add(".accdt", "application/msaccess");
            MimeMappingDictionary.Add(".acx", "application/internet-property-stream");
            MimeMappingDictionary.Add(".afm", "application/octet-stream");
            MimeMappingDictionary.Add(".ai", "application/postscript");
            MimeMappingDictionary.Add(".aif", "audio/x-aiff");
            MimeMappingDictionary.Add(".aifc", "audio/aiff");
            MimeMappingDictionary.Add(".aiff", "audio/aiff");
            MimeMappingDictionary.Add(".application", "application/x-ms-application");
            MimeMappingDictionary.Add(".art", "image/x-jg");
            MimeMappingDictionary.Add(".asd", "application/octet-stream");
            MimeMappingDictionary.Add(".asf", "video/x-ms-asf");
            MimeMappingDictionary.Add(".asi", "application/octet-stream");
            MimeMappingDictionary.Add(".asm", "text/plain");
            MimeMappingDictionary.Add(".asr", "video/x-ms-asf");
            MimeMappingDictionary.Add(".asx", "video/x-ms-asf");
            MimeMappingDictionary.Add(".atom", "application/atom+xml");
            MimeMappingDictionary.Add(".au", "audio/basic");
            MimeMappingDictionary.Add(".avi", "video/x-msvideo");
            MimeMappingDictionary.Add(".axs", "application/olescript");
            MimeMappingDictionary.Add(".bas", "text/plain");
            MimeMappingDictionary.Add(".bcpio", "application/x-bcpio");
            MimeMappingDictionary.Add(".bin", "application/octet-stream");
            MimeMappingDictionary.Add(".bmp", "image/bmp");
            MimeMappingDictionary.Add(".c", "text/plain");
            MimeMappingDictionary.Add(".cab", "application/octet-stream");
            MimeMappingDictionary.Add(".calx", "application/vnd.ms-office.calx");
            MimeMappingDictionary.Add(".cat", "application/vnd.ms-pki.seccat");
            MimeMappingDictionary.Add(".cdf", "application/x-cdf");
            MimeMappingDictionary.Add(".chm", "application/octet-stream");
            MimeMappingDictionary.Add(".class", "application/x-java-applet");
            MimeMappingDictionary.Add(".clp", "application/x-msclip");
            MimeMappingDictionary.Add(".cmx", "image/x-cmx");
            MimeMappingDictionary.Add(".cnf", "text/plain");
            MimeMappingDictionary.Add(".cod", "image/cis-cod");
            MimeMappingDictionary.Add(".cpio", "application/x-cpio");
            MimeMappingDictionary.Add(".cpp", "text/plain");
            MimeMappingDictionary.Add(".crd", "application/x-mscardfile");
            MimeMappingDictionary.Add(".crl", "application/pkix-crl");
            MimeMappingDictionary.Add(".crt", "application/x-x509-ca-cert");
            MimeMappingDictionary.Add(".csh", "application/x-csh");
            MimeMappingDictionary.Add(".css", "text/css");
            MimeMappingDictionary.Add(".csv", "application/octet-stream");
            MimeMappingDictionary.Add(".cur", "application/octet-stream");
            MimeMappingDictionary.Add(".dcr", "application/x-director");
            MimeMappingDictionary.Add(".deploy", "application/octet-stream");
            MimeMappingDictionary.Add(".der", "application/x-x509-ca-cert");
            MimeMappingDictionary.Add(".dib", "image/bmp");
            MimeMappingDictionary.Add(".dir", "application/x-director");
            MimeMappingDictionary.Add(".disco", "text/xml");
            MimeMappingDictionary.Add(".dll", "application/x-msdownload");
            MimeMappingDictionary.Add(".dll.config", "text/xml");
            MimeMappingDictionary.Add(".dlm", "text/dlm");
            MimeMappingDictionary.Add(".doc", "application/msword");
            MimeMappingDictionary.Add(".docm", "application/vnd.ms-word.document.macroEnabled.12");
            MimeMappingDictionary.Add(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            MimeMappingDictionary.Add(".dot", "application/msword");
            MimeMappingDictionary.Add(".dotm", "application/vnd.ms-word.template.macroEnabled.12");
            MimeMappingDictionary.Add(".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template");
            MimeMappingDictionary.Add(".dsp", "application/octet-stream");
            MimeMappingDictionary.Add(".dtd", "text/xml");
            MimeMappingDictionary.Add(".dvi", "application/x-dvi");
            MimeMappingDictionary.Add(".dwf", "drawing/x-dwf");
            MimeMappingDictionary.Add(".dwp", "application/octet-stream");
            MimeMappingDictionary.Add(".dxr", "application/x-director");
            MimeMappingDictionary.Add(".eml", "message/rfc822");
            MimeMappingDictionary.Add(".emz", "application/octet-stream");
            MimeMappingDictionary.Add(".eot", "application/octet-stream");
            MimeMappingDictionary.Add(".eps", "application/postscript");
            MimeMappingDictionary.Add(".etx", "text/x-setext");
            MimeMappingDictionary.Add(".evy", "application/envoy");
            MimeMappingDictionary.Add(".exe", "application/octet-stream");
            MimeMappingDictionary.Add(".exe.config", "text/xml");
            MimeMappingDictionary.Add(".fdf", "application/vnd.fdf");
            MimeMappingDictionary.Add(".fif", "application/fractals");
            MimeMappingDictionary.Add(".fla", "application/octet-stream");
            MimeMappingDictionary.Add(".flr", "x-world/x-vrml");
            MimeMappingDictionary.Add(".flv", "video/x-flv");
            MimeMappingDictionary.Add(".gif", "image/gif");
            MimeMappingDictionary.Add(".gtar", "application/x-gtar");
            MimeMappingDictionary.Add(".gz", "application/x-gzip");
            MimeMappingDictionary.Add(".h", "text/plain");
            MimeMappingDictionary.Add(".hdf", "application/x-hdf");
            MimeMappingDictionary.Add(".hdml", "text/x-hdml");
            MimeMappingDictionary.Add(".hhc", "application/x-oleobject");
            MimeMappingDictionary.Add(".hhk", "application/octet-stream");
            MimeMappingDictionary.Add(".hhp", "application/octet-stream");
            MimeMappingDictionary.Add(".hlp", "application/winhlp");
            MimeMappingDictionary.Add(".hqx", "application/mac-binhex40");
            MimeMappingDictionary.Add(".hta", "application/hta");
            MimeMappingDictionary.Add(".htc", "text/x-component");
            MimeMappingDictionary.Add(".htm", "text/html");
            MimeMappingDictionary.Add(".html", "text/html");
            MimeMappingDictionary.Add(".htt", "text/webviewhtml");
            MimeMappingDictionary.Add(".hxt", "text/html");
            MimeMappingDictionary.Add(".ico", "image/x-icon");
            MimeMappingDictionary.Add(".ics", "application/octet-stream");
            MimeMappingDictionary.Add(".ief", "image/ief");
            MimeMappingDictionary.Add(".iii", "application/x-iphone");
            MimeMappingDictionary.Add(".inf", "application/octet-stream");
            MimeMappingDictionary.Add(".ins", "application/x-internet-signup");
            MimeMappingDictionary.Add(".isp", "application/x-internet-signup");
            MimeMappingDictionary.Add(".IVF", "video/x-ivf");
            MimeMappingDictionary.Add(".jar", "application/java-archive");
            MimeMappingDictionary.Add(".java", "application/octet-stream");
            MimeMappingDictionary.Add(".jck", "application/liquidmotion");
            MimeMappingDictionary.Add(".jcz", "application/liquidmotion");
            MimeMappingDictionary.Add(".jfif", "image/pjpeg");
            MimeMappingDictionary.Add(".jpb", "application/octet-stream");
            MimeMappingDictionary.Add(".jpe", "image/jpeg");
            MimeMappingDictionary.Add(".jpeg", "image/jpeg");
            MimeMappingDictionary.Add(".jpg", "image/jpeg");
            MimeMappingDictionary.Add(".js", "application/x-javascript");
            MimeMappingDictionary.Add(".jsx", "text/jscript");
            MimeMappingDictionary.Add(".latex", "application/x-latex");
            MimeMappingDictionary.Add(".lit", "application/x-ms-reader");
            MimeMappingDictionary.Add(".lpk", "application/octet-stream");
            MimeMappingDictionary.Add(".lsf", "video/x-la-asf");
            MimeMappingDictionary.Add(".lsx", "video/x-la-asf");
            MimeMappingDictionary.Add(".lzh", "application/octet-stream");
            MimeMappingDictionary.Add(".m13", "application/x-msmediaview");
            MimeMappingDictionary.Add(".m14", "application/x-msmediaview");
            MimeMappingDictionary.Add(".m1v", "video/mpeg");
            MimeMappingDictionary.Add(".m3u", "audio/x-mpegurl");
            MimeMappingDictionary.Add(".man", "application/x-troff-man");
            MimeMappingDictionary.Add(".manifest", "application/x-ms-manifest");
            MimeMappingDictionary.Add(".map", "text/plain");
            MimeMappingDictionary.Add(".mdb", "application/x-msaccess");
            MimeMappingDictionary.Add(".mdp", "application/octet-stream");
            MimeMappingDictionary.Add(".me", "application/x-troff-me");
            MimeMappingDictionary.Add(".mht", "message/rfc822");
            MimeMappingDictionary.Add(".mhtml", "message/rfc822");
            MimeMappingDictionary.Add(".mid", "audio/mid");
            MimeMappingDictionary.Add(".midi", "audio/mid");
            MimeMappingDictionary.Add(".mix", "application/octet-stream");
            MimeMappingDictionary.Add(".mmf", "application/x-smaf");
            MimeMappingDictionary.Add(".mno", "text/xml");
            MimeMappingDictionary.Add(".mny", "application/x-msmoney");
            MimeMappingDictionary.Add(".mov", "video/quicktime");
            MimeMappingDictionary.Add(".movie", "video/x-sgi-movie");
            MimeMappingDictionary.Add(".mp2", "video/mpeg");
            MimeMappingDictionary.Add(".mp3", "audio/mpeg");
            MimeMappingDictionary.Add(".mpa", "video/mpeg");
            MimeMappingDictionary.Add(".mpe", "video/mpeg");
            MimeMappingDictionary.Add(".mpeg", "video/mpeg");
            MimeMappingDictionary.Add(".mpg", "video/mpeg");
            MimeMappingDictionary.Add(".mpp", "application/vnd.ms-project");
            MimeMappingDictionary.Add(".mpv2", "video/mpeg");
            MimeMappingDictionary.Add(".ms", "application/x-troff-ms");
            MimeMappingDictionary.Add(".msi", "application/octet-stream");
            MimeMappingDictionary.Add(".mso", "application/octet-stream");
            MimeMappingDictionary.Add(".mvb", "application/x-msmediaview");
            MimeMappingDictionary.Add(".mvc", "application/x-miva-compiled");
            MimeMappingDictionary.Add(".nc", "application/x-netcdf");
            MimeMappingDictionary.Add(".nsc", "video/x-ms-asf");
            MimeMappingDictionary.Add(".nws", "message/rfc822");
            MimeMappingDictionary.Add(".ocx", "application/octet-stream");
            MimeMappingDictionary.Add(".oda", "application/oda");
            MimeMappingDictionary.Add(".odc", "text/x-ms-odc");
            MimeMappingDictionary.Add(".ods", "application/oleobject");
            MimeMappingDictionary.Add(".one", "application/onenote");
            MimeMappingDictionary.Add(".onea", "application/onenote");
            MimeMappingDictionary.Add(".onetoc", "application/onenote");
            MimeMappingDictionary.Add(".onetoc2", "application/onenote");
            MimeMappingDictionary.Add(".onetmp", "application/onenote");
            MimeMappingDictionary.Add(".onepkg", "application/onenote");
            MimeMappingDictionary.Add(".osdx", "application/opensearchdescription+xml");
            MimeMappingDictionary.Add(".p10", "application/pkcs10");
            MimeMappingDictionary.Add(".p12", "application/x-pkcs12");
            MimeMappingDictionary.Add(".p7b", "application/x-pkcs7-certificates");
            MimeMappingDictionary.Add(".p7c", "application/pkcs7-mime");
            MimeMappingDictionary.Add(".p7m", "application/pkcs7-mime");
            MimeMappingDictionary.Add(".p7r", "application/x-pkcs7-certreqresp");
            MimeMappingDictionary.Add(".p7s", "application/pkcs7-signature");
            MimeMappingDictionary.Add(".pbm", "image/x-portable-bitmap");
            MimeMappingDictionary.Add(".pcx", "application/octet-stream");
            MimeMappingDictionary.Add(".pcz", "application/octet-stream");
            MimeMappingDictionary.Add(".pdf", "application/pdf");
            MimeMappingDictionary.Add(".pfb", "application/octet-stream");
            MimeMappingDictionary.Add(".pfm", "application/octet-stream");
            MimeMappingDictionary.Add(".pfx", "application/x-pkcs12");
            MimeMappingDictionary.Add(".pgm", "image/x-portable-graymap");
            MimeMappingDictionary.Add(".pko", "application/vnd.ms-pki.pko");
            MimeMappingDictionary.Add(".pma", "application/x-perfmon");
            MimeMappingDictionary.Add(".pmc", "application/x-perfmon");
            MimeMappingDictionary.Add(".pml", "application/x-perfmon");
            MimeMappingDictionary.Add(".pmr", "application/x-perfmon");
            MimeMappingDictionary.Add(".pmw", "application/x-perfmon");
            MimeMappingDictionary.Add(".png", "image/png");
            MimeMappingDictionary.Add(".pnm", "image/x-portable-anymap");
            MimeMappingDictionary.Add(".pnz", "image/png");
            MimeMappingDictionary.Add(".pot", "application/vnd.ms-powerpoint");
            MimeMappingDictionary.Add(".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12");
            MimeMappingDictionary.Add(".potx", "application/vnd.openxmlformats-officedocument.presentationml.template");
            MimeMappingDictionary.Add(".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12");
            MimeMappingDictionary.Add(".ppm", "image/x-portable-pixmap");
            MimeMappingDictionary.Add(".pps", "application/vnd.ms-powerpoint");
            MimeMappingDictionary.Add(".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12");
            MimeMappingDictionary.Add(".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow");
            MimeMappingDictionary.Add(".ppt", "application/vnd.ms-powerpoint");
            MimeMappingDictionary.Add(".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12");
            MimeMappingDictionary.Add(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
            MimeMappingDictionary.Add(".prf", "application/pics-rules");
            MimeMappingDictionary.Add(".prm", "application/octet-stream");
            MimeMappingDictionary.Add(".prx", "application/octet-stream");
            MimeMappingDictionary.Add(".ps", "application/postscript");
            MimeMappingDictionary.Add(".psd", "application/octet-stream");
            MimeMappingDictionary.Add(".psm", "application/octet-stream");
            MimeMappingDictionary.Add(".psp", "application/octet-stream");
            MimeMappingDictionary.Add(".pub", "application/x-mspublisher");
            MimeMappingDictionary.Add(".qt", "video/quicktime");
            MimeMappingDictionary.Add(".qtl", "application/x-quicktimeplayer");
            MimeMappingDictionary.Add(".qxd", "application/octet-stream");
            MimeMappingDictionary.Add(".ra", "audio/x-pn-realaudio");
            MimeMappingDictionary.Add(".ram", "audio/x-pn-realaudio");
            MimeMappingDictionary.Add(".rar", "application/octet-stream");
            MimeMappingDictionary.Add(".ras", "image/x-cmu-raster");
            MimeMappingDictionary.Add(".rf", "image/vnd.rn-realflash");
            MimeMappingDictionary.Add(".rgb", "image/x-rgb");
            MimeMappingDictionary.Add(".rm", "application/vnd.rn-realmedia");
            MimeMappingDictionary.Add(".rmi", "audio/mid");
            MimeMappingDictionary.Add(".roff", "application/x-troff");
            MimeMappingDictionary.Add(".rpm", "audio/x-pn-realaudio-plugin");
            MimeMappingDictionary.Add(".rtf", "application/rtf");
            MimeMappingDictionary.Add(".rtx", "text/richtext");
            MimeMappingDictionary.Add(".scd", "application/x-msschedule");
            MimeMappingDictionary.Add(".sct", "text/scriptlet");
            MimeMappingDictionary.Add(".sea", "application/octet-stream");
            MimeMappingDictionary.Add(".setpay", "application/set-payment-initiation");
            MimeMappingDictionary.Add(".setreg", "application/set-registration-initiation");
            MimeMappingDictionary.Add(".sgml", "text/sgml");
            MimeMappingDictionary.Add(".sh", "application/x-sh");
            MimeMappingDictionary.Add(".shar", "application/x-shar");
            MimeMappingDictionary.Add(".sit", "application/x-stuffit");
            MimeMappingDictionary.Add(".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12");
            MimeMappingDictionary.Add(".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide");
            MimeMappingDictionary.Add(".smd", "audio/x-smd");
            MimeMappingDictionary.Add(".smi", "application/octet-stream");
            MimeMappingDictionary.Add(".smx", "audio/x-smd");
            MimeMappingDictionary.Add(".smz", "audio/x-smd");
            MimeMappingDictionary.Add(".snd", "audio/basic");
            MimeMappingDictionary.Add(".snp", "application/octet-stream");
            MimeMappingDictionary.Add(".spc", "application/x-pkcs7-certificates");
            MimeMappingDictionary.Add(".spl", "application/futuresplash");
            MimeMappingDictionary.Add(".src", "application/x-wais-source");
            MimeMappingDictionary.Add(".ssm", "application/streamingmedia");
            MimeMappingDictionary.Add(".sst", "application/vnd.ms-pki.certstore");
            MimeMappingDictionary.Add(".stl", "application/vnd.ms-pki.stl");
            MimeMappingDictionary.Add(".sv4cpio", "application/x-sv4cpio");
            MimeMappingDictionary.Add(".sv4crc", "application/x-sv4crc");
            MimeMappingDictionary.Add(".svg", "image/svg+xml");
            MimeMappingDictionary.Add(".swf", "application/x-shockwave-flash");
            MimeMappingDictionary.Add(".t", "application/x-troff");
            MimeMappingDictionary.Add(".tar", "application/x-tar");
            MimeMappingDictionary.Add(".tcl", "application/x-tcl");
            MimeMappingDictionary.Add(".tex", "application/x-tex");
            MimeMappingDictionary.Add(".texi", "application/x-texinfo");
            MimeMappingDictionary.Add(".texinfo", "application/x-texinfo");
            MimeMappingDictionary.Add(".tgz", "application/x-compressed");
            MimeMappingDictionary.Add(".thmx", "application/vnd.ms-officetheme");
            MimeMappingDictionary.Add(".thn", "application/octet-stream");
            MimeMappingDictionary.Add(".tif", "image/tiff");
            MimeMappingDictionary.Add(".tiff", "image/tiff");
            MimeMappingDictionary.Add(".toc", "application/octet-stream");
            MimeMappingDictionary.Add(".tr", "application/x-troff");
            MimeMappingDictionary.Add(".trm", "application/x-msterminal");
            MimeMappingDictionary.Add(".tsv", "text/tab-separated-values");
            MimeMappingDictionary.Add(".ttf", "application/octet-stream");
            MimeMappingDictionary.Add(".txt", "text/plain");
            MimeMappingDictionary.Add(".u32", "application/octet-stream");
            MimeMappingDictionary.Add(".uls", "text/iuls");
            MimeMappingDictionary.Add(".ustar", "application/x-ustar");
            MimeMappingDictionary.Add(".vbs", "text/vbscript");
            MimeMappingDictionary.Add(".vcf", "text/x-vcard");
            MimeMappingDictionary.Add(".vcs", "text/plain");
            MimeMappingDictionary.Add(".vdx", "application/vnd.ms-visio.viewer");
            MimeMappingDictionary.Add(".vml", "text/xml");
            MimeMappingDictionary.Add(".vsd", "application/vnd.visio");
            MimeMappingDictionary.Add(".vss", "application/vnd.visio");
            MimeMappingDictionary.Add(".vst", "application/vnd.visio");
            MimeMappingDictionary.Add(".vsto", "application/x-ms-vsto");
            MimeMappingDictionary.Add(".vsw", "application/vnd.visio");
            MimeMappingDictionary.Add(".vsx", "application/vnd.visio");
            MimeMappingDictionary.Add(".vtx", "application/vnd.visio");
            MimeMappingDictionary.Add(".wav", "audio/wav");
            MimeMappingDictionary.Add(".wax", "audio/x-ms-wax");
            MimeMappingDictionary.Add(".wbmp", "image/vnd.wap.wbmp");
            MimeMappingDictionary.Add(".wcm", "application/vnd.ms-works");
            MimeMappingDictionary.Add(".wdb", "application/vnd.ms-works");
            MimeMappingDictionary.Add(".wks", "application/vnd.ms-works");
            MimeMappingDictionary.Add(".wm", "video/x-ms-wm");
            MimeMappingDictionary.Add(".wma", "audio/x-ms-wma");
            MimeMappingDictionary.Add(".wmd", "application/x-ms-wmd");
            MimeMappingDictionary.Add(".wmf", "application/x-msmetafile");
            MimeMappingDictionary.Add(".wml", "text/vnd.wap.wml");
            MimeMappingDictionary.Add(".wmlc", "application/vnd.wap.wmlc");
            MimeMappingDictionary.Add(".wmls", "text/vnd.wap.wmlscript");
            MimeMappingDictionary.Add(".wmlsc", "application/vnd.wap.wmlscriptc");
            MimeMappingDictionary.Add(".wmp", "video/x-ms-wmp");
            MimeMappingDictionary.Add(".wmv", "video/x-ms-wmv");
            MimeMappingDictionary.Add(".wmx", "video/x-ms-wmx");
            MimeMappingDictionary.Add(".wmz", "application/x-ms-wmz");
            MimeMappingDictionary.Add(".wps", "application/vnd.ms-works");
            MimeMappingDictionary.Add(".wri", "application/x-mswrite");
            MimeMappingDictionary.Add(".wrl", "x-world/x-vrml");
            MimeMappingDictionary.Add(".wrz", "x-world/x-vrml");
            MimeMappingDictionary.Add(".wsdl", "text/xml");
            MimeMappingDictionary.Add(".wvx", "video/x-ms-wvx");
            MimeMappingDictionary.Add(".x", "application/directx");
            MimeMappingDictionary.Add(".xaf", "x-world/x-vrml");
            MimeMappingDictionary.Add(".xaml", "application/xaml+xml");
            MimeMappingDictionary.Add(".xap", "application/x-silverlight-app");
            MimeMappingDictionary.Add(".xbap", "application/x-ms-xbap");
            MimeMappingDictionary.Add(".xbm", "image/x-xbitmap");
            MimeMappingDictionary.Add(".xdr", "text/plain");
            MimeMappingDictionary.Add(".xht", "application/xhtml+xml");
            MimeMappingDictionary.Add(".xhtml", "application/xhtml+xml");
            MimeMappingDictionary.Add(".xla", "application/vnd.ms-excel");
            MimeMappingDictionary.Add(".xlam", "application/vnd.ms-excel.addin.macroEnabled.12");
            MimeMappingDictionary.Add(".xlc", "application/vnd.ms-excel");
            MimeMappingDictionary.Add(".xlm", "application/vnd.ms-excel");
            MimeMappingDictionary.Add(".xls", "application/vnd.ms-excel");
            MimeMappingDictionary.Add(".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12");
            MimeMappingDictionary.Add(".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12");
            MimeMappingDictionary.Add(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            MimeMappingDictionary.Add(".xlt", "application/vnd.ms-excel");
            MimeMappingDictionary.Add(".xltm", "application/vnd.ms-excel.template.macroEnabled.12");
            MimeMappingDictionary.Add(".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template");
            MimeMappingDictionary.Add(".xlw", "application/vnd.ms-excel");
            MimeMappingDictionary.Add(".xml", "text/xml");
            MimeMappingDictionary.Add(".xof", "x-world/x-vrml");
            MimeMappingDictionary.Add(".xpm", "image/x-xpixmap");
            MimeMappingDictionary.Add(".xps", "application/vnd.ms-xpsdocument");
            MimeMappingDictionary.Add(".xsd", "text/xml");
            MimeMappingDictionary.Add(".xsf", "text/xml");
            MimeMappingDictionary.Add(".xsl", "text/xml");
            MimeMappingDictionary.Add(".xslt", "text/xml");
            MimeMappingDictionary.Add(".xsn", "application/octet-stream");
            MimeMappingDictionary.Add(".xtp", "application/octet-stream");
            MimeMappingDictionary.Add(".xwd", "image/x-xwindowdump");
            MimeMappingDictionary.Add(".z", "application/x-compress");
            MimeMappingDictionary.Add(".zip", "application/x-zip-compressed");
            MimeMappingDictionary.Add(".*", "application/octet-stream");
        }

        /// <summary>
        /// Gets the MIME mapping.
        /// </summary>
        /// <param name="filename">Name of the file.</param>
        /// <returns>Content type.</returns>
        public static string GetMimeMapping(string filename)
        {
            filename = GetFileName(filename);

            for (int i = 0; i < filename.Length; i++)
            {
                if (filename[i] == '.')
                {
                    string mimeType;

                    if (MimeMappingDictionary.TryGetValue(filename.Substring(i), out mimeType))
                    {
                        return mimeType;
                    }
                }
            }

            return MimeMappingDictionary[".*"];
        }

        /// <summary>
        /// Gets the name of the file. This method is similar to Path.GetFileName(), but it doesn't fail on invalid path characters.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The name of the file</returns>
        private static string GetFileName(string path)
        {
            int pathSeparatorIndex = path.LastIndexOfAny(PathSeparatorChars);

            return (pathSeparatorIndex >= 0) ? path.Substring(pathSeparatorIndex) : path;
        }
    }
}
