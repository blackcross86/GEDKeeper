/* 
 * Copyright 2009 Alexander Curtis <alex@logicmill.com>
 * This file is part of GEDmill - A family history website creator
 * 
 * GEDmill is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * GEDmill is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with GEDmill.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using BSLib;
using GKCore;
using GKCore.Interfaces;
using GKCL = GKCore.Logging;

namespace GEDmill
{
    /// <summary>
    /// Class that contains all the user configurable settings, and serialises them into IsolatedStorage at program start and exit.
    /// Note not all settings here are presented through the UI, but most are. Originally there was a distinction her but it has become blurred.
    /// </summary>
    public class GMConfig
    {
        public const string LOG_FILE = "GEDmill.log";
        public const string LOG_LEVEL = "WARN";

        // The current version of this app, for display purposes.
        public const string SoftwareVersion = "1.12.0.0";

        // The name of the app for display purposes
        public const string SoftwareName = "GEDmill";

        // Filename for the online help (as in "on the same system", as opposed to offline e.g. printed manual)
        public static string HelpFilename = "GEDmill Help.chm";

        private static readonly GKCL.ILogger fLogger = GKCL.LogManager.GetLogger(GMConfig.LOG_FILE, GMConfig.LOG_LEVEL, typeof(GMConfig).Name);

        // Filename used to store users config
        public const string ConfigFilename = "GEDmillPlugin.ini";

        // Name to use for stylesheet css file.
        public const string StylesheetFilename = "gedmill-style.css";


        private ILangMan fLangMan;

        // Name to use where no other name is available
        public string UnknownName;

        // How to capitalise individuals' names
        public int NameCapitalisation;

        // Absolute sFilename of image to use as background
        public string BackgroundImage;

        // Maximum allowed width of individual image
        public int MaxImageWidth;

        // Maximum allowed height of individual image
        public int MaxImageHeight;

        // Age at which defining occupation is chosen
        public int AgeForOccupation;

        // Maximum allowed width of source image
        public int MaxSourceImageWidth;

        // Maximum allowed height of source image
        public int MaxSourceImageHeight;

        // Maximum allowed width of thumbnail image
        public int MaxThumbnailImageWidth;

        // Maximum allowed height of thumbnail image
        public int MaxThumbnailImageHeight;

        // Filename of image if any to use on front webpage.
        public string FrontPageImageFilename;

        // Filename of front webpage.
        public string FrontPageFilename;

        // True if event descriptions to start with capital letter, e.g. "Born" as opposed to "born".
        public bool CapitaliseEventDescriptions;

        // True if to include text about website statistics on front page.
        public bool ShowFrontPageStats;

        // Extra text to include on title page.
        public string CommentaryText;

        // Name of font to render tree diagrams with
        public string TreeFontName;

        // Size of font to render tree diagrams with
        public float TreeFontSize;

        // Width to try not to exceed when creating mini tree diagrams
        public int TargetTreeWidth;

        // True if mini tree diagrams are to be added to pages
        public bool ShowMiniTrees;

        // User's email address to include on webpages
        public string UserEmailAddress;

        // Allow multiple multimedia files per individual. Enables m_nMaxNumberMultimediaFiles.
        public bool AllowMultipleImages;

        // Whether to include files that aren't pictures as multimedia objects
        public bool AllowNonPictures;

        // Link to users main website, put at top of each indi page.
        public string MainWebsiteLink;

        // Name to user for individuals who appear but have their information withheld
        public string ConcealedName;

        // Word used to prefix locations, e.g. "at" or "in".
        public string PlaceWord;

        // String to use (in index) for names with no surname
        public string NoSurname;

        // True if all sources mentioned by a restricted individual should also be restricted (i.e. omitted from website)
        public bool RestrictAssociatedSources;

        // True if all multimedia files are to be renamed to a standard naming system. Now generally superceded by m_renameOriginalPicture
        public bool RenameMultimedia;

        // True to generate a separate html page for each letter of names index. Useful if index would otherwise be massive.
        public bool IndexLetterPerPage;

        // HTML string representing color to draw connecting lines in mini trees
        public Color MiniTreeColorBranch;

        // HTML string representing color to draw outline of individual boxes in mini trees
        public Color MiniTreeColorIndiBorder;

        // HTML string representing color to fill normal individual boxes in mini trees
        public Color MiniTreeColorIndiBackground;

        // HTML string representing color to fill selected individual boxes in mini trees
        public Color MiniTreeColorIndiHighlight;

        // HTML string representing color to fill boxes for individuals marked as concealed in mini trees.
        public Color MiniTreeColorIndiBgConcealed;

        // HTML string representing color to write text for individuals marked as concealed in mini trees.
        public Color MiniTreeColorIndiFgConcealed;

        // HTML string representing color to fill shaded individual boxes in mini trees
        public Color MiniTreeColorIndiShade;

        // HTML string representing color to draw text in individual boxes in mini trees
        public Color MiniTreeColorIndiText;

        // HTML string representing color to draw linkable text in individual boxes in mini trees
        public Color MiniTreeColorIndiLink;

        // HTML string representing color to fill entire background of mini tree diagrams
        public Color MiniTreeColorBackground;

        // Whether to restrict records with RESN set to confidential
        public bool RestrictConfidential;

        // Whether to restrict records with RESN set to privacy
        public bool RestrictPrivacy;

        // If true, multimedia files are copied into m_imageFolder.
        public bool CopyMultimedia;

        // Which folder to copy multimedia into. Ends up as subdirectory to m_outputFolder;
        public string ImageFolder;

        // If true, links to multimedia are altered to be relative to output folder (e.g. ..\..\files\file.zzz). If false, they are left as absolute (e.g. D:\files\file.zzz)
        public bool RelativiseMultimedia;

        // Maximum number of multimedia files per individual page
        public int MaxNumberMultimediaFiles;

        // Title to use for index page
        public string IndexTitle;

        // Folder in which to dump all the html output
        public string OutputFolder;

        // Name of owner used in titles, descriptions of pages.
        public string OwnersName;

        // Xref of root individual in tree, for link on front page.
        public string FirstRecordXRef;

        // Title to put on front page of website
        public string SiteTitle;

        // Path to application's data
        public string AppDataPath;

        // If true, the background image will be used to fill in the background of mini trees, giving the effect that they are transparent.
        public bool FakeMiniTreeTransparency;

        // List of Xrefs for individuals to be mentioned on front page.
        public List<string> KeyIndividuals;

        // Set true if indexes are allowed to span multiple html files
        public bool MultiPageIndexes;

        // Number of individuals to aim to list per index page.
        public int IndividualsPerIndexPage;

        // If true, website pages are opened in web browser once app exits with Finish button.
        public bool OpenWebsiteOnExit;

        // If true, instead of pretending restricted individuals don't exist, they are shown as black boxes.
        public bool OnlyConceal;

        // If true, clicking the picture will link to the original large picture.
        public bool LinkOriginalPicture;

        // If true, the original picture will be renamed as it is copied. Now takes on role of m_renameMultimedia.
        public bool RenameOriginalPicture;

        // Directory in which list of excluded individuals is stored
        public string ExcludeFileDir;

        // Name of file in which list of excluded individuals is stored
        public string ExcludeFileName;

        // If true, email addresses won't appear in web pages in harvestable form.
        public bool ObfuscateEmails;

        // If true, "Last updated <date>" appears on home page.
        public bool AddHomePageCreateTime;

        // If true, nickName else otherName appears in brackets in individual's index entry.
        public bool IncludeNickNamesInIndex;

        // Optional text user can have displayed in each page footer.
        public string CustomFooter;

        // If true, include the individuals (most appropriate, best) occupation in page header.
        public bool OccupationHeadline;

        // True indicates that text in Commentary box (settings pane) should not be escaped. (i.e. HTML is preserved)
        public bool CommentaryIsHtml;

        // True indicates that text in Footer box (settings pane) should not be escaped. (i.e. HTML is preserved)
        public bool FooterIsHtml;

        // If false, doesn't include the navigation bar at the top of each page.
        public bool IncludeNavbar;

        // If true, withheld records will use the indivdual's name in minitrees, rather than m_concealedName.
        public bool UseWithheldNames;

        // If true, first names are put on a different line to surnames in minitree individual boxes.
        public bool ConserveTreeWidth;

        // If false, no multimedia pics, images, files etc. will appear in HTML.
        public bool AllowMultimedia;

        // If true, the list of "Citations" on source records will not be generated.
        public bool SupressBackreferences;


        private static GMConfig fInstance;

        public static GMConfig Instance
        {
            get {
                if (fInstance == null) {
                    fInstance = new GMConfig();
                }
                return fInstance;
            }
        }

        // Construct the HTTP URL for the created site's landing page
        public string FrontPageURL
        {
            get {
                return string.Concat(OutputFolder, "\\", FrontPageFilename, ".html");
            }
        }


        // Constructor, sets default values for the config
        private GMConfig()
        {
            AppDataPath = AppHost.GetAppDataPathStatic();
        }

        // Reset those settings that can be modified by the user on the config screen.
        public void Reset(ILangMan langMan)
        {
            fLangMan = langMan;

            BackgroundImage = GMHelper.GetAppPath() + "\\gedmill-bg.jpg";
            FrontPageImageFilename = GMHelper.GetAppPath() + "\\gedmill.jpg";

            RestrictConfidential = false;
            RestrictPrivacy = false;
            OutputFolder = "";
            UnknownName = langMan.LS(PLS.LSID_UnknownName); // "no name" implied we knew them and they had no name.
            NameCapitalisation = 1;
            CopyMultimedia = true;
            ImageFolder = "multimedia";
            RelativiseMultimedia = false;
            MaxImageWidth = 160;
            MaxImageHeight = 160;
            MaxNumberMultimediaFiles = 32;
            AgeForOccupation = 50;
            NoSurname = langMan.LS(PLS.LSID_NoSurname);
            IndexTitle = langMan.LS(PLS.LSID_IndexTitle);
            MaxSourceImageWidth = 800;
            MaxSourceImageHeight = 800;
            MaxThumbnailImageWidth = 45;
            MaxThumbnailImageHeight = 45;
            FirstRecordXRef = "";
            PlaceWord = langMan.LS(PLS.LSID_PlaceWord);
            CapitaliseEventDescriptions = true;
            RestrictAssociatedSources = true;
            RenameMultimedia = true;
            IndexLetterPerPage = false;
            ShowFrontPageStats = true;
            CommentaryText = "";

            OwnersName = Environment.UserName;

            SiteTitle = langMan.LS(PLS.LSID_SiteTitle);
            if (!string.IsNullOrEmpty(OwnersName))
                SiteTitle += " of " + OwnersName;

            TreeFontName = "Arial";
            TreeFontSize = 8.0f;
            TargetTreeWidth = 800;
            MiniTreeColorBranch = ConvertColor("#000000");
            MiniTreeColorIndiBorder = ConvertColor("#000000");
            MiniTreeColorIndiBackground = ConvertColor("#ffffd2");
            MiniTreeColorIndiHighlight = ConvertColor("#ffffff");
            MiniTreeColorIndiBgConcealed = ConvertColor("#cccccc");
            MiniTreeColorIndiFgConcealed = ConvertColor("#000000");
            MiniTreeColorIndiShade = ConvertColor("#ffffd2");
            MiniTreeColorIndiText = ConvertColor("#000000");
            MiniTreeColorIndiLink = ConvertColor("#3333ff");
            MiniTreeColorBackground = ConvertColor("#aaaaaa");
            FakeMiniTreeTransparency = false;
            ShowMiniTrees = true;
            UserEmailAddress = "";
            KeyIndividuals = new List<string>();
            MultiPageIndexes = true;
            IndividualsPerIndexPage = 1000;
            OpenWebsiteOnExit = true;
            FrontPageFilename = "home";
            AllowMultipleImages = false;
            AllowNonPictures = true;
            MainWebsiteLink = "";
            OnlyConceal = false;
            ConcealedName = langMan.LS(PLS.LSID_ConcealedName);
            LinkOriginalPicture = false;
            RenameOriginalPicture = false;
            ExcludeFileDir = "";
            ExcludeFileName = "";
            ObfuscateEmails = false;
            AddHomePageCreateTime = true;
            IncludeNickNamesInIndex = true;
            CustomFooter = "";
            OccupationHeadline = true;
            CommentaryIsHtml = false;
            FooterIsHtml = false;
            IncludeNavbar = true;
            UseWithheldNames = false;
            ConserveTreeWidth = false;
            AllowMultimedia = true;
            SupressBackreferences = false;
        }

        // Deserialise all the settings
        public void Load()
        {
            try {
                using (var ini = new IniFile(AppDataPath + ConfigFilename)) {
                    RestrictConfidential = ini.ReadBool("Common", "RestrictConfidential", false);
                    RestrictPrivacy = ini.ReadBool("Common", "RestrictPrivacy", false);
                    OutputFolder = ini.ReadString("Common", "OutputFolder", "");
                    UnknownName = ini.ReadString("Common", "UnknownName", fLangMan.LS(PLS.LSID_UnknownName));
                    NameCapitalisation = ini.ReadInteger("Common", "NameCapitalisation", 1);
                    CopyMultimedia = ini.ReadBool("Common", "CopyMultimedia", true);
                    ImageFolder = ini.ReadString("Common", "ImageFolder", "multimedia");
                    RelativiseMultimedia = ini.ReadBool("Common", "RelativiseMultimedia", false);
                    BackgroundImage = ini.ReadString("Common", "BackgroundImage", BackgroundImage);
                    MaxImageWidth = ini.ReadInteger("Common", "MaxImageWidth", 160);
                    MaxImageHeight = ini.ReadInteger("Common", "MaxImageHeight", 160);
                    MaxNumberMultimediaFiles = ini.ReadInteger("Common", "MaxNumberMultimediaFiles", 32);
                    AgeForOccupation = ini.ReadInteger("Common", "AgeForOccupation", 50);
                    OwnersName = ini.ReadString("Common", "OwnersName", "");
                    NoSurname = ini.ReadString("Common", "NoSurname", fLangMan.LS(PLS.LSID_NoSurname));
                    IndexTitle = ini.ReadString("Common", "IndexTitle", fLangMan.LS(PLS.LSID_IndexTitle));
                    MaxSourceImageWidth = ini.ReadInteger("Common", "MaxSourceImageWidth", 800);
                    MaxSourceImageHeight = ini.ReadInteger("Common", "MaxSourceImageHeight", 800);
                    FirstRecordXRef = ini.ReadString("Common", "FirstRecordXRef", "");
                    SiteTitle = ini.ReadString("Common", "SiteTitle", fLangMan.LS(PLS.LSID_SiteTitle));
                    FrontPageImageFilename = ini.ReadString("Common", "FrontPageImageFilename", FrontPageImageFilename);
                    PlaceWord = ini.ReadString("Common", "PlaceWord", fLangMan.LS(PLS.LSID_PlaceWord));
                    CapitaliseEventDescriptions = ini.ReadBool("Common", "CapitaliseEventDescriptions", true);
                    RestrictAssociatedSources = ini.ReadBool("Common", "RestrictAssociatedSources", true);
                    RenameMultimedia = ini.ReadBool("Common", "RenameMultimedia", true);
                    IndexLetterPerPage = ini.ReadBool("Common", "IndexLetterPerPage", false);
                    MiniTreeColorBranch = ConvertColor(ini.ReadString("Common", "MiniTreeColorBranch", "#000000"));
                    MiniTreeColorIndiBorder = ConvertColor(ini.ReadString("Common", "MiniTreeColorIndiBorder", "#000000"));
                    MiniTreeColorIndiBackground = ConvertColor(ini.ReadString("Common", "MiniTreeColorIndiBackground", "#ffffd2"));
                    MiniTreeColorIndiHighlight = ConvertColor(ini.ReadString("Common", "MiniTreeColorIndiHighlight", "#ffffff"));
                    MiniTreeColorIndiShade = ConvertColor(ini.ReadString("Common", "MiniTreeColorIndiShade", "#ffffd2"));
                    ShowFrontPageStats = ini.ReadBool("Common", "ShowFrontPageStats", true);
                    CommentaryText = ini.ReadString("Common", "CommentaryText", "");
                    TreeFontName = ini.ReadString("Common", "TreeFontName", "Arial");
                    TreeFontSize = (float)ini.ReadFloat("Common", "TreeFontSize", 8.0f);
                    TargetTreeWidth = ini.ReadInteger("Common", "TargetTreeWidth", 800);
                    MiniTreeColorIndiText = ConvertColor(ini.ReadString("Common", "MiniTreeColorIndiText", "#000000"));
                    MiniTreeColorIndiLink = ConvertColor(ini.ReadString("Common", "MiniTreeColorIndiLink", "#3333ff"));
                    MiniTreeColorBackground = ConvertColor(ini.ReadString("Common", "MiniTreeColorBackground", "#aaaaaa"));
                    ShowMiniTrees = ini.ReadBool("Common", "ShowMiniTrees", true);
                    UserEmailAddress = ini.ReadString("Common", "UserEmailAddress", "");
                    FakeMiniTreeTransparency = ini.ReadBool("Common", "FakeMiniTreeTransparency", false);

                    int nKeyIndividuals = ini.ReadInteger("Individuals", "Count", 0);
                    KeyIndividuals = new List<string>();
                    for (int i = 0; i < nKeyIndividuals; i++) {
                        string keyXref = ini.ReadString("Individuals", "I_" + i, "");
                        KeyIndividuals.Add(keyXref);
                    }

                    MultiPageIndexes = ini.ReadBool("Common", "MultiPageIndexes", true);
                    IndividualsPerIndexPage = ini.ReadInteger("Common", "IndividualsPerIndexPage", 1000);
                    OpenWebsiteOnExit = ini.ReadBool("Common", "OpenWebsiteOnExit", true);
                    FrontPageFilename = ini.ReadString("Common", "FrontPageFilename", "home");
                    AllowMultipleImages = ini.ReadBool("Common", "AllowMultipleImages", false);
                    AllowNonPictures = ini.ReadBool("Common", "AllowNonPictures", true);
                    MaxThumbnailImageWidth = ini.ReadInteger("Common", "MaxThumbnailImageWidth", 45);
                    MaxThumbnailImageHeight = ini.ReadInteger("Common", "MaxThumbnailImageHeight", 45);
                    MainWebsiteLink = ini.ReadString("Common", "MainWebsiteLink", "");
                    MiniTreeColorIndiBgConcealed = ConvertColor(ini.ReadString("Common", "MiniTreeColorIndiBgConcealed", "#cccccc"));
                    OnlyConceal = ini.ReadBool("Common", "OnlyConceal", false);
                    ConcealedName = ini.ReadString("Common", "ConcealedName", fLangMan.LS(PLS.LSID_ConcealedName));
                    MiniTreeColorIndiFgConcealed = ConvertColor(ini.ReadString("Common", "MiniTreeColorIndiFgConcealed", "#000000"));
                    LinkOriginalPicture = ini.ReadBool("Common", "LinkOriginalPicture", false);
                    RenameOriginalPicture = ini.ReadBool("Common", "RenameOriginalPicture", false);
                    ExcludeFileDir = ini.ReadString("Common", "ExcludeFileDir", "");
                    ExcludeFileName = ini.ReadString("Common", "ExcludeFileName", "");
                    ObfuscateEmails = ini.ReadBool("Common", "ObfuscateEmails", false);
                    AddHomePageCreateTime = ini.ReadBool("Common", "AddHomePageCreateTime", true);
                    IncludeNickNamesInIndex = ini.ReadBool("Common", "IncludeNickNamesInIndex", true);
                    CustomFooter = ini.ReadString("Common", "CustomFooter", "");
                    OccupationHeadline = ini.ReadBool("Common", "OccupationHeadline", true);
                    CommentaryIsHtml = ini.ReadBool("Common", "CommentaryIsHtml", false);
                    FooterIsHtml = ini.ReadBool("Common", "FooterIsHtml", false);
                    IncludeNavbar = ini.ReadBool("Common", "IncludeNavbar", true);
                    UseWithheldNames = ini.ReadBool("Common", "UseWithheldNames", false);
                    ConserveTreeWidth = ini.ReadBool("Common", "ConserveTreeWidth", false);
                    AllowMultimedia = ini.ReadBool("Common", "AllowMultimedia", true);
                    SupressBackreferences = ini.ReadBool("Common", "SupressBackreferences", false);
                }
            } catch (Exception ex) {
                fLogger.WriteError("GMConfig.Load()", ex);
            }
        }

        // Serialise all the config settings
        public void Save()
        {
            try {
                using (var ini = new IniFile(AppDataPath + ConfigFilename)) {
                    ini.WriteBool("Common", "RestrictConfidential", RestrictConfidential);
                    ini.WriteBool("Common", "RestrictPrivacy", RestrictPrivacy);
                    ini.WriteString("Common", "OutputFolder", OutputFolder);
                    ini.WriteString("Common", "UnknownName", UnknownName);
                    ini.WriteInteger("Common", "NameCapitalisation", NameCapitalisation);
                    ini.WriteBool("Common", "CopyMultimedia", CopyMultimedia);
                    ini.WriteString("Common", "ImageFolder", ImageFolder);
                    ini.WriteBool("Common", "RelativiseMultimedia", RelativiseMultimedia);
                    ini.WriteString("Common", "BackgroundImage", BackgroundImage);
                    ini.WriteInteger("Common", "MaxImageWidth", MaxImageWidth);
                    ini.WriteInteger("Common", "MaxImageHeight", MaxImageHeight);
                    ini.WriteInteger("Common", "MaxNumberMultimediaFiles", MaxNumberMultimediaFiles);
                    ini.WriteInteger("Common", "AgeForOccupation", AgeForOccupation);
                    ini.WriteString("Common", "OwnersName", OwnersName);
                    ini.WriteString("Common", "NoSurname", NoSurname);
                    ini.WriteString("Common", "IndexTitle", IndexTitle);
                    ini.WriteInteger("Common", "MaxSourceImageWidth", MaxSourceImageWidth);
                    ini.WriteInteger("Common", "MaxSourceImageHeight", MaxSourceImageHeight);
                    ini.WriteString("Common", "FirstRecordXRef", FirstRecordXRef);
                    ini.WriteString("Common", "SiteTitle", SiteTitle);
                    ini.WriteString("Common", "FrontPageImageFilename", FrontPageImageFilename);
                    ini.WriteString("Common", "PlaceWord", PlaceWord);
                    ini.WriteBool("Common", "CapitaliseEventDescriptions", CapitaliseEventDescriptions);
                    ini.WriteBool("Common", "RestrictAssociatedSources", RestrictAssociatedSources);
                    ini.WriteBool("Common", "RenameMultimedia", RenameMultimedia);
                    ini.WriteBool("Common", "IndexLetterPerPage", IndexLetterPerPage);
                    ini.WriteString("Common", "MiniTreeColorBranch", ConvertColor(MiniTreeColorBranch));
                    ini.WriteString("Common", "MiniTreeColorIndiBorder", ConvertColor(MiniTreeColorIndiBorder));
                    ini.WriteString("Common", "MiniTreeColorIndiBackground", ConvertColor(MiniTreeColorIndiBackground));
                    ini.WriteString("Common", "MiniTreeColorIndiHighlight", ConvertColor(MiniTreeColorIndiHighlight));
                    ini.WriteString("Common", "MiniTreeColorIndiShade", ConvertColor(MiniTreeColorIndiShade));
                    ini.WriteBool("Common", "ShowFrontPageStats", ShowFrontPageStats);
                    ini.WriteString("Common", "CommentaryText", CommentaryText);
                    ini.WriteString("Common", "TreeFontName", TreeFontName);
                    ini.WriteFloat("Common", "TreeFontSize", TreeFontSize);
                    ini.WriteInteger("Common", "TargetTreeWidth", TargetTreeWidth);
                    ini.WriteString("Common", "MiniTreeColorIndiText", ConvertColor(MiniTreeColorIndiText));
                    ini.WriteString("Common", "MiniTreeColorIndiLink", ConvertColor(MiniTreeColorIndiLink));
                    ini.WriteString("Common", "MiniTreeColorBackground", ConvertColor(MiniTreeColorBackground));
                    ini.WriteBool("Common", "ShowMiniTrees", ShowMiniTrees);
                    ini.WriteString("Common", "UserEmailAddress", UserEmailAddress);
                    ini.WriteBool("Common", "FakeMiniTreeTransparency", FakeMiniTreeTransparency);

                    if (KeyIndividuals != null) {
                        int keyIndividualsCount = KeyIndividuals.Count;
                        ini.WriteInteger("Individuals", "Count", keyIndividualsCount);
                        for (int i = 0; i < keyIndividualsCount; i++) {
                            string keyXref = KeyIndividuals[i];
                            ini.WriteString("Individuals", "I_" + i, keyXref);
                        }
                    }

                    ini.WriteBool("Common", "MultiPageIndexes", MultiPageIndexes);
                    ini.WriteInteger("Common", "IndividualsPerIndexPage", IndividualsPerIndexPage);
                    ini.WriteBool("Common", "OpenWebsiteOnExit", OpenWebsiteOnExit);
                    ini.WriteString("Common", "FrontPageFilename", FrontPageFilename);
                    ini.WriteBool("Common", "AllowMultipleImages", AllowMultipleImages);
                    ini.WriteBool("Common", "AllowNonPictures", AllowNonPictures);
                    ini.WriteInteger("Common", "MaxThumbnailImageWidth", MaxThumbnailImageWidth);
                    ini.WriteInteger("Common", "MaxThumbnailImageHeight", MaxThumbnailImageHeight);
                    ini.WriteString("Common", "MainWebsiteLink", MainWebsiteLink);
                    ini.WriteString("Common", "MiniTreeColorIndiBgConcealed", ConvertColor(MiniTreeColorIndiBgConcealed));
                    ini.WriteBool("Common", "OnlyConceal", OnlyConceal);
                    ini.WriteString("Common", "ConcealedName", ConcealedName);
                    ini.WriteString("Common", "MiniTreeColorIndiFgConcealed", ConvertColor(MiniTreeColorIndiFgConcealed));
                    ini.WriteBool("Common", "LinkOriginalPicture", LinkOriginalPicture);
                    ini.WriteBool("Common", "RenameOriginalPicture", RenameOriginalPicture);
                    ini.WriteString("Common", "ExcludeFileDir", ExcludeFileDir);
                    ini.WriteString("Common", "ExcludeFileName", ExcludeFileName);
                    ini.WriteBool("Common", "ObfuscateEmails", ObfuscateEmails);
                    ini.WriteBool("Common", "AddHomePageCreateTime", AddHomePageCreateTime);
                    ini.WriteBool("Common", "IncludeNickNamesInIndex", IncludeNickNamesInIndex);
                    ini.WriteString("Common", "CustomFooter", CustomFooter);
                    ini.WriteBool("Common", "OccupationHeadline", OccupationHeadline);
                    ini.WriteBool("Common", "CommentaryIsHtml", CommentaryIsHtml);
                    ini.WriteBool("Common", "FooterIsHtml", FooterIsHtml);
                    ini.WriteBool("Common", "IncludeNavbar", IncludeNavbar);
                    ini.WriteBool("Common", "UseWithheldNames", UseWithheldNames);
                    ini.WriteBool("Common", "ConserveTreeWidth", ConserveTreeWidth);
                    ini.WriteBool("Common", "AllowMultimedia", AllowMultimedia);
                    ini.WriteBool("Common", "SupressBackreferences", SupressBackreferences);
                }
            } catch (Exception ex) {
                fLogger.WriteError("GMConfig.Save()", ex);
            }
        }

        // Converts a string of the form #RRGGBB to a Color instance.
        // Used when retrieving colors from the config.
        private static Color ConvertColor(string s)
        {
            if (string.IsNullOrEmpty(s)) {
                return Color.Black;
            }

            int nRed = 0;
            int nGreen = 0;
            int nBlue = 0;

            // FIXME: temp hack, IniFile cuts values with leading #
            if (s[0] != '#') {
                s = '#' + s;
            }

            switch (s.Length) {
                case 4:
                    s = s.Substring(1);
                    goto case 3;
                case 3:
                    nRed = int.Parse(s.Substring(0, 1), NumberStyles.HexNumber);
                    nGreen = int.Parse(s.Substring(1, 1), NumberStyles.HexNumber);
                    nBlue = int.Parse(s.Substring(2, 1), NumberStyles.HexNumber);
                    break;
                case 7:
                    s = s.Substring(1);
                    goto case 6;
                case 6:
                    nRed = int.Parse(s.Substring(0, 2), NumberStyles.HexNumber);
                    nGreen = int.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
                    nBlue = int.Parse(s.Substring(4, 2), NumberStyles.HexNumber);
                    break;
            }

            return Color.FromArgb(nRed, nGreen, nBlue);
        }

        // Converts a Color instance to a string of the form #RRGGBB.
        // Used when storing colors in the config.
        private static string ConvertColor(Color c)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }
    }
}
