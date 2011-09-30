﻿/*This file is part of TikzEdt.
 
TikzEdt is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
 
TikzEdt is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.
 
You should have received a copy of the GNU General Public License
along with TikzEdt.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security;
using Microsoft.Win32;
using System.Globalization;

//using System.Drawing;

namespace TikzEdt
{
    /// <summary>
    /// Please put all global constants that are not in the Properties.Settings into this static class.
    /// </summary>
    static class Consts
    {
        public const double cmperin = 2.54;
        public const double ptspertikzunit = 28.45;//72.0 / cmperin; // 28.3464567
        public const double cmperem = 10 / Consts.ptspertikzunit;   // this is a hack
        public const double cmperex = 4.3 / Consts.ptspertikzunit;  // this too
        public const double TikzDefaultLinewidth = 0;
        public const string TikzDefaultFont = "Times";
        public const double TikzDefaultFontSize = 8;
        public const int TikzImgResolution = 300; // resolution in dpi with which images are compiled

        //these files will be deleted, when file is closed. (by DeleteTemporaryFiles())
        public static string[] TemporaryFileExt = new string[] { ".aux", ".tmp", ".log", ".svn", ".toc", ".out" };
        public static string[] PreviewFileExt = new string[] { ".tex", ".pdf", ".aux", ".tmp", ".log", "_BB.txt", ".svn", ".toc", ".out" };

        //public static string[] TikzArrowTipCodes = new string[] { "", ">", "<" };
        //public static DashStyle[] TikzToSystemDashStyle = new DashStyle[] { DashStyle.Solid, DashStyle.Dot, DashStyle.Dash };
        //public static float[][] TikzToSystemDashPattern = new float[][] {
        //    new float[]{0.1F}, new float[]{.1F,.1F}, new float[]{.2F,.1F}
        //};
        public const string StdFileDialogFilter = "Tex Files|*.tex|Tikz files|*.tikz|All Files|*.*";
        public const string cSettingsDir = "Editor"; //this path is relative for GettAppdataPath().
        public const string cCompletionsFile = "CodeCompletions.xml";
        public const string cSettingsFile = "T2Gsettings.xml";
        public const string cSyntaxFile = "TikzSyntax.xshd";
        public const string cSnippetsFile = "TheSnippets.xml";
        public const string cSnippetThumbsDir = "img";
        public const string cMRUFile = "T2GMRU.xml";    // not used
        public const int MaxMRU = 10;// not used
       // public const string cStyleRepoFile = "StyleRepo.dat";

        //public const string CoordinateVertexStyleName = "helper";

        public const string DoubleFormat = "#.####";

        public const float selecttoler = .01F;
        public const float drawXsize = .1F; // drawn for invisible vertices
        public const float coordvertexsize = .25F; // size of size zero vertex (to ease selection)

        //const string cLatex = "Pdflatex";
        //string cLatexPath = "pdflatex"; //@"C:\Program Files\MiKTeX 2.8\miktex\bin\pdfplatex.exe";
        public const string cTempFile = "TE_temp_preview";         // for preview
        public const string cTempExportFile = "TE_export";         // for export
        public const string cTempImgFile = "temp_previewtexts"; // for equation rendering
        public const string defaultCurFile = "New Tikzfile";
        public const string PreviewHeader =
        @"\documentclass[tight]{article}
\usepackage{tikz,amsmath, amssymb,bm,color}
\usepackage[margin=0cm,nohead]{geometry}
\usepackage[active,tightpage]{preview}
\usetikzlibrary{shapes,arrows}
";

        public const string ImgHeader =
        @"\documentclass[fleqn]{article}
\usepackage{amsmath, amssymb,bm,color}
";

        /// <summary>
        /// The starting code for a new File 
        /// </summary>
        public const string DefaultTikzCode =
@"\begin{tikzpicture}

\end{tikzpicture}";
        /// <summary>
        ///  The tikz code that is inserted before \end{tikzpicture}.
        ///  It writes the bounding box to the auxiliary file ..._BB.txt.
        ///  The "unnecessary" invisible node at the beginning is inserted since for some strange reason
        ///  Tikz outputs a very large bounding box if the tikzpicture is empty.  
        /// </summary> 
        public const string CodeToWriteBB =
@"
\usetikzlibrary{calc}
\pgftransformreset
\node[inner sep=0pt,outer sep=0pt,minimum size=0pt,line width=0pt,text width=0pt,text height=0pt] at (current bounding box) {};
%add border to avoid cropping by pdflibnet
\foreach \border in {0.1}
  \useasboundingbox (current bounding box.south west)+(-\border,-\border) rectangle (current bounding box.north east)+(\border,\border);
\newwrite\metadatafile
\immediate\openout\metadatafile=\jobname_BB.txt
\path
  let
    \p1=(current bounding box.south west),
    \p2=(current bounding box.north east)
  in
  node[inner sep=0pt,outer sep=0pt,minimum size=0pt,line width=0pt,text width=0pt,text height=0pt,draw=white] at (current bounding box) {
\immediate\write\metadatafile{\p1,\p2}
};
\immediate\closeout\metadatafile
";

        //public const string precompilation_args = "-ini -job-name=\"" + cTempFile + "\" \"&pdflatex " + cTempFile + "pre.tex\\dump\"";
        //public const string precompilation_args_img = "-ini -job-name=\"" + cTempImgFile + "\" \"&latex " + cTempImgFile + "pre.tex\\dump\"";

        //Todo: the following creates a problem on first load
        public static string SystaxFileFullPath { get { return System.Windows.Forms.Application.UserAppDataPath + "\\" + Consts.cSettingsDir + "\\" + Consts.cSyntaxFile; } }
        public static string CompletionsFileFullPath { get { return System.Windows.Forms.Application.UserAppDataPath + "\\" + Consts.cSettingsDir + "\\" + Consts.cCompletionsFile; } }

        // TE preprocessor commands
        public static string PreProc_Comment = "%!TE%";
        public static string PreProc_CompilerOptions = "%!TEO";
        public static string PreProc_Include = "%!TEI";
    }

    /// <summary>
    /// This purely static class is host to functions of global interest (or those which have no home).
    /// </summary>
    static class Helper
    {
        /// <summary>
        /// This function takes a string and removes all trailing and leading and all multiple whitespace.
        /// E.g. "  brown    fox  " -> "brown fox"
        /// </summary>
        /// <param name="inputString">the string</param>
        /// <returns>the same string, with the whitespace removed</returns>
        public static string RemoveMultipleWhitespace(string inputString)
        {
            string[] parts = inputString.Trim().Split(new char[] { ' ', '\n', '\t', '\r', '\f', '\v' }, 
                StringSplitOptions.RemoveEmptyEntries);
            return String.Join(" ", parts);
        }
        /// <summary>
        /// Options set current working directory can be set to.        
        /// </summary>
        public enum WorkingDirOptions { DirFromFile, TempDir };
        /// <summary>
        /// Set the current working directory. 
        /// </summary>
        /// <param name="option">Option to set work dir to.</param>
        /// <param name="file">If WorkingDirOptions.DirFromFile, specify file that shall be in the working dir afterwards.</param>
        public static void SetCurrentWorkingDir(WorkingDirOptions option, string file = "")
        { 
            if(option==WorkingDirOptions.TempDir)
                Environment.CurrentDirectory = System.IO.Path.GetTempPath();
            else if (option == WorkingDirOptions.DirFromFile)
            {
                if (file == "")
                    throw new Exception("Parameter file in SetCurrentWorkingDir() must not be empty when option==WorkingDirOptions.DirFromFile!");

                String dir = System.IO.Path.GetDirectoryName(file);
                if (dir == "")
                    throw new Exception("Parameter file in SetCurrentWorkingDir() must contain the full path!");
                else
                    Environment.CurrentDirectory = dir;
            }
        }
        public static string GetCurrentWorkingDir()
        {
            return Environment.CurrentDirectory;
        }

        public static string GetLayoutConfigFilepath()
        {
            return GetAppdataPath() + "\\TikzEdtLayout.xml";
        }
        public enum AppdataPathOptions { AppData, ExeDir };
        private static string _AppdataPath = "";
        public static void SetAppdataPath(AppdataPathOptions option)
        {
            if (option == AppdataPathOptions.AppData)
                _AppdataPath = System.Windows.Forms.Application.UserAppDataPath; //created automatically by .NET!
            else
                _AppdataPath = GetAppDir();
        }
        public static string GetAppdataPath()
        {
            if (_AppdataPath == "")
            {

                //throw new Exception("AppdataPath not set yet! Do it using SetAppdataPath() before calling GetAppdataPath().");
                //int DOESNOTLETDESIGNSHOW = 3; //that is why this line is disabled.
            }
            return _AppdataPath;
        }

        public static bool IsAppDirWritable()
        {           
            return HasWritePermissionOnDir(GetAppDir());
        }

        public static bool HasWritePermissionOnDir(string path)
        {
            //**** This is still not  working well on my machine *******

            // stupid method: try to write to a file 
            string cFile = System.IO.Path.Combine(path, "TikzEdt_temp_todelete" + ".txt"); // DateTime.Now.Ticks.ToString()+
            //string cFile2 = System.IO.Path.Combine(path, "TikzEdt_temp_todelete" + ".txt"); // DateTime.Now.Ticks.ToString()+
            StreamWriter sw = null;
            StreamReader sr = null;
            string secret = DateTime.Now.Ticks.ToString();
            try
            {
                sw = new StreamWriter(cFile);
                sw.WriteLine(secret);
                sw.Close();

                sr = new StreamReader(cFile);
                string s = sr.ReadLine();
                sr.Close();
                
                //MainWindow.AddStatusLine("Testen " + cFile);
                File.Delete(cFile);

                if (secret == s)
                    return true;                
            }
            catch (Exception)
            {

            }
            finally
            {
                if (sw != null)
                    sw.Close();
                if (sr != null)
                    sr.Close();
            }
            return false;

            // this doesn't seem to work on my machine
      /*      var writeAllow = false;
            var writeDeny = false;
            var accessControlList = Directory.GetAccessControl(path);
            var accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));

            foreach (System.Security.AccessControl.FileSystemAccessRule rule in accessRules)
            {
                if ((System.Security.AccessControl.FileSystemRights.Write & rule.FileSystemRights) != System.Security.AccessControl.FileSystemRights.Write) continue;

                if (rule.AccessControlType == System.Security.AccessControl.AccessControlType.Allow)
                    writeAllow = true;
                else if (rule.AccessControlType == System.Security.AccessControl.AccessControlType.Deny)
                    writeDeny = true;
            }

            return writeAllow && !writeDeny; */
        }

        
        
        public static string GetAppDir() // w/o trailing backslash 
        {
            return System.AppDomain.CurrentDomain.BaseDirectory; ;
            /*throw new Exception("GetAppDir() is obsolete! Use GetAppdataPath() or GetCurrentWorkingDir() instead.");
            string appPath = "";
            try
            {
                appPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                //since CodeBase returns a URI formatted string, character '#' has special meaning
                //(separating base URI from parameters)
                //however, we have a directory here. '#' is a valid, normal character here.
                appPath = appPath.Replace("#", "%23");
                appPath = System.IO.Path.GetDirectoryName(appPath);
                Uri uriAddress2 = (new Uri(appPath));
                appPath = uriAddress2.LocalPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.ToString());
            }
            return appPath;*/
        }

        /// <summary>
        /// Path where is configuration files are stored (usually \\Editor)
        /// </summary>
        /// <returns></returns>
        public static string GetSettingsPath()
        {
            return GetAppdataPath() + "\\" + Consts.cSettingsDir + "\\";
        }
        /// <summary>
        /// Path where the snippetes are stored (usually \\img)
        /// </summary>
        /// <returns></returns>
        public static string GetSnippetsPath()
        {
            return GetAppdataPath() + "\\" + Consts.cSnippetThumbsDir  + "\\";
        }
        public static string GetSnippetsExt()
        {
            return ".tex";
        }

        //this is where the .fmt is created.
        public static string GetPrecompiledHeaderPath()
        {
            string s = GetAppdataPath();
            if (s.EndsWith("\\"))
                return s;
            else
                return s + "\\";
        }
        public static string GetPrecompiledHeaderFilename()
        {
            return "temp_header.tex";
        }

        public static string GetTempFileName()
        {
            return Consts.cTempFile + Process.GetCurrentProcess().Id;
        }

        public static string GetPreviewFilename()
        { 
            return ".preview";
        }
        public static string GetPreviewFilenameExt()
        {
            return ".tex";
        }

       /* public static void GeneratePrecompiledHeaders()
        {
            //StreamWriter s = new StreamWriter(Consts.cTempImgFile + "pre.tex");
            //s.WriteLine(Consts.ImgHeader);
            //s.Close();bool ImgHeader=false

            //System.Diagnostics.Process p = new System.Diagnostics.Process();
            //System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("latex");
            //psi.Arguments = Consts.precompilation_args_img;
            //psi.CreateNoWindow = true;
            //p.StartInfo = psi;
            //p.Start();
            
            StreamWriter s = new StreamWriter(Consts.cTempFile + "pre.tex");
            s.WriteLine(Properties.Settings.Default.Tex_Preamble);
            s.Close();

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("pdflatex");
            psi.Arguments = Consts.precompilation_args;
            psi.CreateNoWindow = true;
            p.StartInfo = psi;
            //p.Exited +=new EventHandler(Helper.precompilation_Exited);
            //needs non-static callback function.
            //however, since this function is static it cannot reach anything non-static.
            p.Start();
        } */

        public static string RemoveFileExtension(string file)
        {            
            string ext = System.IO.Path.GetExtension(file);
            return file.Remove(file.Length - ext.Length, ext.Length);
        }

        /// <summary>
        /// Delete all temporary files named FileName in working dir,
        /// </summary>
        /// <param name="FileName"></param>
        public static void DeleteTemporaryFiles(string FileName, bool IsTempFile = false)
        {
            List<String> FilesToDelete = new List<string>();
            if (IsTempFile)
            {
                //if this is a temp file delete all files created by preview (incl. .tex and .pdf)
                foreach (string ext in Consts.PreviewFileExt)
                    FilesToDelete.Add(RemoveFileExtension(FileName) + ext);
            }
            else
            {
                //this is not a temp file so only delete tempary files (filename + log,aux, ...)
                foreach (string ext in Consts.TemporaryFileExt)
                    FilesToDelete.Add(RemoveFileExtension(FileName) + ext);
                //delete all preview files.
                foreach (string ext in Consts.PreviewFileExt)
                    FilesToDelete.Add(FileName + GetPreviewFilename() + ext);
            }

            foreach (String file in FilesToDelete)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (IOException)
                { 
                    //PDF file is still loading and cannot be deleted.
                }
            }
        }

        public static Brush GetHatchBrush()
        {
            VisualBrush vb = new VisualBrush();

            vb.TileMode = TileMode.Tile;

            vb.Viewport = new Rect(0, 0, 5, 5);
            vb.ViewportUnits = BrushMappingMode.Absolute;

            vb.Viewbox = new Rect(0, 0, 6, 6);
            vb.ViewboxUnits = BrushMappingMode.Absolute;

            Line l = new Line();
            l.X1 = 0; l.X2 = 6; l.Y1 = 6; l.Y2 = 0;
            l.Stroke = Brushes.Black;
            vb.Visual = l;

            return vb;
        }

        /// <summary>
        /// Finds the point in (angle + 2 Pi \Z) closest to (closeto)
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="closeto"></param>
        /// <returns></returns>
        public static double ClosestPt(double angle, double closeto)
        {
            double diff = angle - closeto;
            return angle - Math.Round(diff / (2 * Math.PI)) * 2 * Math.PI;
        }
        public static double ClosestPtDeg(double angle, double closeto)
        {
            double diff = angle - closeto;
            return angle - Math.Round(diff / (360)) * 360;
        }

        /// <summary>
        /// Count occurrences of strings.
        /// </summary>
        public static int CountStringOccurrences(string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

       // [DllImport("User32.dll")]
       // public static extern bool UpdateWindow(IntPtr HWND);

    }

    public class FileAssociation
    {
        // Associate file extension with progID, description, icon and application
        public static void Associate(string extension,
               string progID, string description, string icon, string application)
        {
            Registry.ClassesRoot.CreateSubKey(extension).SetValue("", progID);
            if (progID != null && progID.Length > 0)
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progID))
                {
                    if (description != null)
                        key.SetValue("", description);
                    if (icon != null)
                        key.CreateSubKey("DefaultIcon").SetValue("", ToShortPathName(icon));
                    if (application != null)
                        key.CreateSubKey(@"Shell\Open\Command").SetValue("",
                                    ToShortPathName(application) + " \"%1\"");
                }
        }

        // Return true if extension already associated in registry
        public static bool IsAssociated(string extension)
        {
            return (Registry.ClassesRoot.OpenSubKey(extension, false) != null);
        }

        [DllImport("Kernel32.dll")]
        private static extern uint GetShortPathName(string lpszLongPath,
            [Out] StringBuilder lpszShortPath, uint cchBuffer);

        // Return short path format of a file name
        private static string ToShortPathName(string longName)
        {
            StringBuilder s = new StringBuilder(1000);
            uint iSize = (uint)s.Capacity;
            uint iRet = GetShortPathName(longName, s, iSize);
            return s.ToString();
        }
    }

    /*public class BBGatherer
    {
        public Rect r;
        public void Add(Point p)
        {
            if (r == null)
            {
                r = new Rect(p.X, p.Y, 0, 0);
            }
            else
            {
                r = Rect.Union(r, p);
            }
        }
        public void Add(Rect tr)
        {
            if (r == null)
            {
                r = tr;
            }
            else
            {
                r = Rect.Union(r, tr);
            }
        }
        public Rect GetRect(double margin)
        {
            return Rect.Inflate(r, margin, margin);
            //.new Rect(r.X - margin, r.Y - margin, r.Width + 2 * margin, r.Height + 2 * margin);
            //return rr;
        }
    }*/
    /*
    public static class Rasterizer
    {

        public static Point rasterizeEucl(Point p, double GridWidth, Rect BB)
        {
            return new Point(Math.Round(p.X / GridWidth) * GridWidth, Math.Round(p.Y / GridWidth) * GridWidth);
        }
        // input: p in Eucl. coordinates
        //public static Point rasterizePolar(Point p, Point center, double rstep, int nsectors)
        //{
        //Point pp = Point.eucltopolar(p, center);
        //pp.X = Math.Round(pp.X / rstep) * rstep;
        //pp.Y = Math.Round(pp.Y * nsectors / (2 * Math.PI)) * 2 * Math.PI / nsectors;
        //return Point.polartoeucl(pp, center);
        //}

    }*/

    /// <summary>
    /// The purpose of this class is to channel the (limited) Viewmodel user interaction,
    /// so that the viewmodel can be tested in unit tests. TODO
    /// </summary>
    public static class GlobalUI
    {
        
    }


    #region Converters
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility");

            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    [ValueConversion(typeof(Nullable<bool>), typeof(Nullable<bool>))]
    public class InverseNullableBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Nullable<bool>))
                throw new InvalidOperationException("The target must be a nullable boolean");

            Nullable<bool> b = value as Nullable<bool>;            
            return !b;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Nullable<bool>))
                throw new InvalidOperationException("The target must be a nullable boolean");

            Nullable<bool> b = value as Nullable<bool>;
            return !b;
        }

        #endregion
    }

    [ValueConversion(typeof(ICSharpCode.AvalonEdit.Document.TextDocument), typeof(ICSharpCode.AvalonEdit.Document.TextDocument))]
    public class AvalonEditDocConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(ICSharpCode.AvalonEdit.Document.TextDocument))
                throw new InvalidOperationException("The target must be a boolean");

            return (ICSharpCode.AvalonEdit.Document.TextDocument)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    [ValueConversion(typeof(Severity), typeof(ImageSource))]
    public sealed class SeverityImageConverter : IValueConverter
    {
        static BitmapSource WarningBmp, ErrorBmp, DefaultBmp;

        static SeverityImageConverter()
        {
            System.Drawing.Icon icon = System.Drawing.SystemIcons.Warning;
            WarningBmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            icon = System.Drawing.SystemIcons.Error;
            ErrorBmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            DefaultBmp = null;
        }

        public object Convert(object value, Type targetType,
                              object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(ImageSource))
                throw new InvalidOperationException("The target must be ImageSource");
            Severity s = (Severity) value;
            switch (s)
            {
                case Severity.ERROR:
                    return ErrorBmp;
                case Severity.WARNING:
                    return WarningBmp;
                default:
                    return DefaultBmp;
            }
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
                              object parameter, System.Globalization.CultureInfo culture)
        {
            GridLength width = (GridLength) (values[0]);
            bool visible = (bool)(values[1]);
            return (visible) ? width : new GridLength(0);
        }

        public object[] ConvertBack(object value, Type[] targetType,
                                  object parameter, System.Globalization.CultureInfo culture)
        {
            return new object[] { value, DependencyProperty.UnsetValue };
        }
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.Equals(false))
                return Binding.DoNothing;
            else
                return parameter;
        }
    }

    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.Equals(parameter))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumAndBooleanToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
                              object parameter, System.Globalization.CultureInfo culture)
        {                        
            bool visible = (values[1] is bool)? (bool)(values[1]) : true;
            if (visible && values[0].Equals(parameter))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetType,
                                  object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class NoopConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class CompareConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return (value != parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


    /// <summary>
    /// Converts from a string like 100 or 100 % to points/cm
    /// </summary>
    class ZoomToResolutionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double d = (double)value;
            return Math.Round(d / Consts.ptspertikzunit * 100).ToString() + " %";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = value as string;
            s = s.Trim();
            if (s.EndsWith("%"))
                s = s.Remove(s.Length - 1);
            double d;
            if (Double.TryParse(s, out d))
            {
                if (d > 2 && d < 6000)
                {
                    return d / 100 * Consts.ptspertikzunit;
                }
            }
            return Binding.DoNothing;
        }
    }    
#endregion
}
