﻿using OpenKh.Common;
using OpenKh.Common.Archives;
using OpenKh.Engine;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Kh2.Contextes;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Game.Infrastructure
{
    public class Kernel : ILanguage
    {
        private readonly IDataContent _dataContent;

        public bool IsReMix { get; }
        public int RegionId { get; }
        public string Language => Constants.Regions[RegionId == Constants.RegionFinalMix ? 0 : RegionId];
        public string Region => Constants.Regions[RegionId];
        public FontContext FontContext { get; }
        public RenderingMessageContext SystemMessageContext { get; set; }
        public RenderingMessageContext EventMessageContext { get; set; }
        public Kh2MessageProvider MessageProvider { get; }
        public BaseTable<Objentry> ObjEntries { get; }
        public Dictionary<string, List<Place>> Places { get; }
        public List<Ftst.Entry> Ftst { get; private set; }
        public Item Item { get; private set; }
        public List<Trsr> Trsr { get; private set; }
        public List<Fmlv.Level> Fmlv { get; private set; }
        public List<Lvup.PlayableCharacter> Lvup { get; private set; }

        public Kernel(IDataContent dataContent)
        {
            _dataContent = dataContent;

            FontContext = new FontContext();
            MessageProvider = new Kh2MessageProvider();
            RegionId = DetectRegion(dataContent);
            IsReMix = IsReMixFileExists(dataContent, Region);

            // Load files in the same order as KH2 does
            ObjEntries = LoadFile("00objentry.bin", stream => Objentry.Read(stream));
            // 00progress
            LoadSystem("03system.bin");
            LoadBattle("00battle.bin");
            // 00common
            // 00worldpoint
            // 07localset
            // 12soundinfo
            // 14mission

            LoadFontImage($"msg/{Language}/fontimage.bar");
            LoadFontInfo($"msg/{Language}/fontinfo.bar");
            Places = LoadFile($"msg/{Language}/place.bin", stream => Place.Read(stream));
            LoadMessage("sys");
            // 15jigsaw

            if (Language == "jp")
            {
                SystemMessageContext = FontContext.ToKh2JpSystemTextContext();
                EventMessageContext = FontContext.ToKh2JpEventTextContext();
            }
            else
            {
                SystemMessageContext = FontContext.ToKh2EuSystemTextContext();
                EventMessageContext = FontContext.ToKh2EuEventTextContext();
            }
        }

        private T LoadFile<T>(string fileName, Func<Stream, T> action)
        {
            using var stream = _dataContent.FileOpen(fileName);
            return action(stream);
        }

        private void LoadSystem(string fileName)
        {
            var bar = _dataContent.FileOpen(fileName).Using(stream => Bar.Read(stream));

            Ftst = bar.ForEntry("ftst", Bar.EntryType.List, Kh2.System.Ftst.Read);
            Item = bar.ForEntry("item", Bar.EntryType.List, Kh2.System.Item.Read);
            Trsr = bar.ForEntry("tsrs", Bar.EntryType.List, Kh2.System.Trsr.Read);
        }

        private void LoadBattle(string fileName)
        {
            var bar = _dataContent.FileOpen(fileName).Using(stream => Bar.Read(stream));

            Fmlv = bar.ForEntry("fmlv", Bar.EntryType.List, Kh2.Battle.Fmlv.Read);
            Lvup = bar.ForEntry("lvup", Bar.EntryType.List, Kh2.Battle.Lvup.Read)?.Characters;
        }

        private void LoadFontInfo(string fileName)
        {
            var bar = _dataContent.FileOpen(fileName).Using(Bar.Read);
            FontContext.Read(bar);
        }

        private void LoadFontImage(string fileName) =>
            LoadFontInfo(fileName);

        private void LoadMessage(string worldId)
        {
            var messageBar = _dataContent.FileOpen($"msg/{Language}/sys.bar")
                .Using(stream => Bar.Read(stream));

            MessageProvider.Load(messageBar.ForEntry(worldId, Bar.EntryType.List, Msg.Read));
        }

        private static int DetectRegion(IDataContent dataContent)
        {
            for (var i = 0; i < Constants.Regions.Length; i++)
            {
                var testFileName = $"menu/{Constants.Regions[i]}/title.2ld";
                if (dataContent.FileExists(testFileName))
                    return i;
            }

            throw new Exception("Unable to detect any region for the game. Some files are potentially missing.");
        }

        public static bool IsReMixFileExists(IDataContent dataContent, string region)
        {
            var testFileName = $"menu/{region}/titlejf.2ld";
            return dataContent.FileExists(testFileName);
        }

        public static bool IsReMixFileHasHdAssetHeader(IDataContent dataContent, string region)
        {
            var testFileName = $"menu/{region}/titlejf.2ld";
            var stream = dataContent.FileOpen(testFileName);
            if (stream == null)
                return false;

            using (stream)
                return HdAsset.IsValid(stream);
        }
    }
}
