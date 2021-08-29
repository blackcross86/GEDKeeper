﻿/*
 *  This file is part of the "GKMap".
 *  GKMap project borrowed from GMap.NET (by radioman).
 *
 *  Copyright (C) 2009-2018 by radioman (email@radioman.lt).
 *  This program is licensed under the FLAT EARTH License.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GKMap.CacheProviders;

namespace GKMap
{
    /// <summary>
    /// cache system for tiles, geocoding, etc...
    /// </summary>
    internal class Cache : Singleton<Cache>
    {
        /// <summary>
        /// abstract image cache
        /// </summary>
        public IPureImageCache ImageCache;

        private string fImageCacheLocation;

        /// <summary>
        /// local cache location
        /// </summary>
        public string ImageCacheLocation
        {
            get {
                return fImageCacheLocation;
            }
            set {
                fImageCacheLocation = value;

                var cache = ImageCache as SQLitePureImageCache;
                if (cache != null) {
                    cache.CacheLocation = value;
                }

                fCacheDelay = true;
            }
        }

        private static bool fCacheDelay;
        private static string fCacheLocation;

        public static string CacheLocation
        {
            get {
                if (string.IsNullOrEmpty(fCacheLocation)) {
                    ResetCacheLocation();
                }

                return fCacheLocation;
            }
            set {
                if (string.IsNullOrEmpty(value)) { // setting to null resets to default
                    ResetCacheLocation();
                } else {
                    fCacheLocation = value;
                }

                if (fCacheDelay) {
                    Cache.Instance.ImageCacheLocation = fCacheLocation;
                }
            }
        }


        public Cache()
        {
            if (Instance != null) {
                throw (new Exception("You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\""));
            }

            ImageCache = new SQLitePureImageCache();

            string newCache = CacheLocation;
            string oldCache = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "GKMap" + Path.DirectorySeparatorChar;

            // move database to non-roaming user directory
            if (Directory.Exists(oldCache)) {
                try {
                    if (Directory.Exists(newCache)) {
                        Directory.Delete(oldCache, true);
                    } else {
                        Directory.Move(oldCache, newCache);
                    }
                    ImageCacheLocation = newCache;
                } catch (Exception ex) {
                    ImageCacheLocation = oldCache;
                    Trace.WriteLine("SQLitePureImageCache, moving data: " + ex);
                }
            } else {
                ImageCacheLocation = newCache;
            }
        }

        private static readonly SHA1CryptoServiceProvider HashProvider = new SHA1CryptoServiceProvider();

        private static void ConvertToHash(ref string s)
        {
            s = BitConverter.ToString(HashProvider.ComputeHash(Encoding.Unicode.GetBytes(s)));
        }

        public void SaveContent(string url, CacheType type, string content)
        {
            try {
                ConvertToHash(ref url);

                string dir = Path.Combine(fImageCacheLocation, type.ToString()) + Path.DirectorySeparatorChar;

                // recreate dir
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }

                string file = dir + url + ".txt";

                using (StreamWriter writer = new StreamWriter(file, false, Encoding.UTF8)) {
                    writer.Write(content);
                }
            } catch (Exception ex) {
                Debug.WriteLine("SaveContent: " + ex);
            }
        }

        public string GetContent(string url, CacheType type, TimeSpan stayInCache)
        {
            string ret = null;

            try {
                ConvertToHash(ref url);

                string dir = Path.Combine(fImageCacheLocation, type.ToString()) + Path.DirectorySeparatorChar;
                string file = dir + url + ".txt";

                if (File.Exists(file)) {
                    var writeTime = File.GetLastWriteTime(file);
                    if (DateTime.Now - writeTime < stayInCache) {
                        using (StreamReader r = new StreamReader(file, Encoding.UTF8)) {
                            ret = r.ReadToEnd();
                        }
                    } else {
                        File.Delete(file);
                    }
                }
            } catch (Exception ex) {
                ret = null;
                Debug.WriteLine("GetContent: " + ex);
            }

            return ret;
        }

        public string GetContent(string url, CacheType type)
        {
            return GetContent(url, type, TimeSpan.FromDays(88));
        }

        private static void ResetCacheLocation()
        {
            string appDataLocation = Stuff.GetApplicationDataFolderPath();

            if (string.IsNullOrEmpty(appDataLocation)) {
                GMaps.Instance.CacheExists = false;
            } else {
                CacheLocation = appDataLocation;
            }
        }
    }

    internal enum CacheType
    {
        GeocoderCache,
        PlacemarkCache,
        UrlCache,
    }
}
