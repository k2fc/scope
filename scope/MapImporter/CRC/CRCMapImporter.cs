using BAMCIS.GeoJSON;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.MapImporter.CRC
{
    internal class CRCMapImporter
    {
        public static List<VideoMap> CRCARTCCFileToMaps (string filename)
        {
            CRCARTCC artcc;
            using (StreamReader file = File.OpenText(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                artcc = (CRCARTCC)serializer.Deserialize(file, typeof(CRCARTCC));
            }
            if (artcc == null)
            {
                return new List<VideoMap>();
            }
            var artcc_id = artcc.id;
            var mapdirectory = Directory.GetParent(filename).Parent.FullName + "\\VideoMaps\\" + artcc_id + "\\";
            var facilities = artcc.facility.childFacilities.Where(x => x.starsConfiguration != null && x.starsConfiguration.videoMapIds.Any());
            if (!facilities.Any())
            {
                return new List<VideoMap>();
            }
            Facility importfacility;
            if (facilities.Count() == 1)
            {
                importfacility = facilities.First();
            }
            else
            {
                var ids = facilities.Select(x => x.id).ToList();
                using (CRCFacilityPicker picker = new CRCFacilityPicker(facilities.Select(x => x.id).ToList()))
                {
                    if (picker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        importfacility = facilities.Where(x => x.id == picker.PickedFacility).FirstOrDefault();
                        if (importfacility == null) 
                        {
                            return new List<VideoMap>();
                        }
                    }
                    else
                    {
                        return new List<VideoMap>();
                    }
                }
            }
            var importmapids = importfacility.starsConfiguration.videoMapIds.ToList();
            List<VideoMap> maps = new List<VideoMap>();
            foreach (var importmapid in importmapids)
            {
                Videomap importmap = artcc.videoMaps.Where(x => x.id == importmapid).FirstOrDefault();
                if (importmap == null)
                { 
                    continue; 
                }
                if (importmap.starsId.HasValue)
                {
                    VideoMap map = new VideoMap();
                    var mappath = mapdirectory + importmap.id + ".geojson";
                    var name = importmap.name;
                    var importmapobj = GeoJSONMapExporter.GeoJSONFileToMaps(mappath);
                    if (importmapobj == null)
                    {
                        System.Windows.Forms.MessageBox.Show("Did not import map: " + name);
                        continue;
                    }
                    if (importmapobj.Any())
                    {
                        map.Lines = importmapobj.First().Lines; 
                    }
                    map.Name = name;
                    map.Category = importmap.starsBrightnessCategory == "A" ? MapCategory.A : MapCategory.B;
                    map.Number = importmap.starsId.Value;
                    maps.Add(map);
                }
            }
            return maps;
        }
    }
}
