using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Xml;
using Rainmeter;

namespace PluginNowPlayingPlex
{
    internal struct Data
    {
        public string Album;
        public string Artist;
        public string Title;
        public int Number;
        public string Year;
        public string Cover;
        public string File;
        public int Duration;
        public int State;
    }

    internal class Measure
    {
        internal enum MeasureType
        {
            Album,
            Artist,
            Title,
            Number,
            Year,
            Cover,
            File,
            Duration,
            State
        }

        internal API api;

        internal MeasureType Type = MeasureType.Title;

        internal virtual void Dispose()
        {
        }

        internal virtual void Reload(Rainmeter.API api)
        {
            this.api = api;

            string type = api.ReadString("PlayerType", "");
            switch (type.ToLowerInvariant())
            {
                case "album":
                    Type = MeasureType.Album;
                    break;

                case "artist":
                    Type = MeasureType.Artist;
                    break;

                case "title":
                    Type = MeasureType.Title;
                    break;

                case "number":
                    Type = MeasureType.Number;
                    break;

                case "year":
                    Type = MeasureType.Year;
                    break;

                case "cover":
                    Type = MeasureType.Cover;
                    break;

                case "file":
                    Type = MeasureType.File;
                    break;

                case "duration":
                    Type = MeasureType.Duration;
                    break;

                case "state":
                    Type = MeasureType.State;
                    break;

                default:
                    api.Log(API.LogType.Error, "NowPlayingPlex: Type=" + type + " not valid");
                    break;
            }
        }

        internal virtual double Update()
        {
            return 0.0;
        }

        internal virtual string GetString()
        {
            return null;
        }
    }

    internal class ParentMeasure : Measure
    {
        internal static List<ParentMeasure> ParentMeasures = new List<ParentMeasure>();

        internal string Name;
        internal IntPtr Skin;

        internal string PlexToken;
        internal string PlexUsername;
        internal string PlexServer;
        internal int DisableLeadingZero;

        internal WebClient client = new WebClient();

        internal Data currentData;

        internal ParentMeasure()
        {
            ParentMeasures.Add(this);
        }

        internal override void Dispose()
        {
            ParentMeasures.Remove(this);
        }

        internal override void Reload(Rainmeter.API api)
        {
            base.Reload(api);

            Name = api.GetMeasureName();
            Skin = api.GetSkin();

            PlexToken = api.ReadString("PlexToken", "");
            PlexUsername = api.ReadString("PlexUsername", "");
            PlexServer = api.ReadString("PlexServer", "http://localhost:32400");
            DisableLeadingZero = api.ReadInt("DisableLeadingZero", 0);

            if (string.IsNullOrEmpty(PlexToken))
            {
                api.Log(API.LogType.Error, "NowPlayingPlex: missing PlexToken");
            }
        }

        internal override double Update()
        {
            string url = PlexServer + "/status/sessions?X-Plex-Token=" + PlexToken;
            api.Log(API.LogType.Debug, "NowPlayingPlex: Query " + url);

            try
            {
                string response = client.DownloadString(url);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);
                XmlNode root = doc.DocumentElement;

                XmlNode userNode = root.SelectSingleNode(string.IsNullOrEmpty(PlexUsername)
                    ? "/MediaContainer/Track/User" : "/MediaContainer/Track/User[@title=\"" + PlexUsername + "\"]");
                if (userNode == null)
                {
                    currentData = default(Data);
                }
                else
                {
                    XmlNode trackNode = userNode.ParentNode;
                    currentData = ReadData(trackNode);
                }
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Error, "NowPlayingPlex: error querying : " + e.Message);
                currentData = default(Data);
            }

            return GetValue(Type);
        }

        internal override string GetString()
        {
            return GetString(Type);
        }

        internal string GetString(MeasureType type)
        {
            switch (type)
            {
                case MeasureType.Album:
                    return currentData.Album;
                case MeasureType.Artist:
                    return currentData.Artist;
                case MeasureType.Title:
                    return currentData.Title;
                case MeasureType.Number:
                    return currentData.Number.ToString();
                case MeasureType.Year:
                    return currentData.Year;
                case MeasureType.Cover:
                    return currentData.Cover;
                case MeasureType.Duration:
                    return FormatDuration(currentData.Duration);
                case MeasureType.State:
                    return currentData.State.ToString();
                default:
                    return null;
            }
        }

        internal double GetValue(MeasureType type)
        {
            switch (type)
            {
                case MeasureType.Number:
                    return currentData.Number;
                case MeasureType.Duration:
                    return currentData.Duration;
                case MeasureType.State:
                    return currentData.State;
                default:
                    return 0;
            }
        }

        internal Data ReadData(XmlNode trackNode)
        {
            Data data = default(Data);

            try
            {
                data.Album = trackNode.Attributes["parentTitle"].Value;
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Debug, "NowPlayingPlex: error reading Album : " + e.Message);
                data.Album = "";
            }
            try
            {
                data.Artist = trackNode.Attributes["grandparentTitle"].Value;
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Debug, "NowPlayingPlex: error reading Artist : " + e.Message);
                data.Artist = "";
            }
            try
            {
                data.Title = trackNode.Attributes["title"].Value;
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Debug, "NowPlayingPlex: error reading Title : " + e.Message);
                data.Title = "";
            }
            try
            {
                data.Number = int.Parse(trackNode.Attributes["index"].Value);
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Debug, "NowPlayingPlex: error reading Number : " + e.Message);
                data.Number = 0;
            }
            try
            {
                data.Year = trackNode.Attributes["parentYear"].Value;
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Debug, "NowPlayingPlex: error reading Year : " + e.Message);
                data.Year = "";
            }
            try
            {
                data.Cover = PlexServer + trackNode.Attributes["thumb"].Value + "?X-Plex-Token=" + PlexToken;
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Debug, "NowPlayingPlex: error reading Cover : " + e.Message);
                data.Cover = "";
            }
            try
            {
                data.File = trackNode.SelectSingleNode("./Media/Part").Attributes["file"].Value;
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Debug, "NowPlayingPlex: error reading File : " + e.Message);
                data.File = "";
            }
            try
            {
                data.Duration = int.Parse(trackNode.Attributes["duration"].Value) / 1000;
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Debug, "NowPlayingPlex: error reading Duration : " + e.Message);
                data.Duration = 0;
            }
            try
            {
                string state = trackNode.SelectSingleNode("./Player").Attributes["state"].Value;
                switch (state)
                {
                    case "playing":
                        data.State = 1;
                        break;
                    case "paused":
                        data.State = 2;
                        break;
                    default:
                        data.State = 0;
                        break;
                }
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Debug, "NowPlayingPlex: error reading State : " + e.Message);
                data.State = 0;
            }

            return data;
        }

        internal string FormatDuration(int duration)
        {
            int seconds = duration % 60;
            int minutes = (duration - seconds) / 60;
            return minutes.ToString().PadLeft(DisableLeadingZero == 1 ? 1 : 2, '0') + ":" + seconds.ToString().PadLeft(2, '0');
        }
    }

    internal class ChildMeasure : Measure
    {
        private ParentMeasure ParentMeasure = null;

        internal override void Reload(Rainmeter.API api)
        {
            base.Reload(api);

            string playerName = api.ReadString("PlayerName", "");
            IntPtr skin = api.GetSkin();

            ParentMeasure = null;
            foreach (ParentMeasure parentMeasure in ParentMeasure.ParentMeasures)
            {
                if (parentMeasure.Skin.Equals(skin) && parentMeasure.Name.Equals(playerName))
                {
                    ParentMeasure = parentMeasure;
                }
            }

            if (ParentMeasure == null)
            {
                api.Log(API.LogType.Error, "NowPlayingPlex: PlayerName=" + playerName + " not valid");
            }
        }

        internal override double Update()
        {
            if (ParentMeasure != null)
            {
                return ParentMeasure.GetValue(Type);
            }

            return 0.0;
        }

        internal override string GetString()
        {
            if (ParentMeasure != null)
            {
                return ParentMeasure.GetString(Type);
            }

            return null;
        }
    }

    public static class Plugin
    {
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            API api = new API(rm);

            string parent = api.ReadString("PlayerName", "");
            Measure measure;
            if (string.IsNullOrEmpty(parent))
            {
                measure = new ParentMeasure();
            }
            else
            {
                measure = new ChildMeasure();
            }

            data = GCHandle.ToIntPtr(GCHandle.Alloc(measure));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Dispose();
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new Rainmeter.API(rm));
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }

        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            string value = measure.GetString();

            if (value != null)
            {
                return Marshal.StringToHGlobalAuto(value);
            }
            else
            {
                return IntPtr.Zero;
            }
        }
    }
}
