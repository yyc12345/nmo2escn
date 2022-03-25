using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace bmx2escn {

    public enum JsonFormatType {
        OpenBallance,
        Imengyu
    }

    public class ImengyuJson {
        public ImengyuJson() {
            sectorCount = 0;
            internalObjects = new ImengyuInternalObjects();
            sectors = new Dictionary<string, List<string>>();
            floors = new List<ImengyuGroup>();
            depthTestCubes = new List<string>();
            groups = new List<ImengyuGroup>();

            foreach(var item in GroupJsonWriter.ELEMENTS) {
                var gp = new ImengyuGroup();
                gp.name = item;
                groups.Add(gp);
            }
            foreach(var item in GroupJsonWriter.FLOORS) {
                var gp = new ImengyuGroup();
                gp.name = item;
                floors.Add(gp);
            }

        }

        public class ImengyuInternalObjects {
            public ImengyuInternalObjects() {
                PS_LevelStart = PE_LevelEnd = "";
                PC_CheckPoints = new Dictionary<string, string>();
                PR_ResetPoints = new Dictionary<string, string>();
            }
                
            public string PS_LevelStart { get; set; }
            public string PE_LevelEnd { get; set; }
            public Dictionary<string, string> PC_CheckPoints { get; set; }
            public Dictionary<string, string> PR_ResetPoints { get; set; }
        }

        public class ImengyuGroup {
            public ImengyuGroup() {
                name = "";
                objects = new List<string>();
            }
            public string name { get; set; }
            public List<string> objects { get; set; }
        }

        public int sectorCount { get; set; }
        public ImengyuInternalObjects internalObjects { get; set; }
        public Dictionary<string, List<string>> sectors { get; set; }
        public List<ImengyuGroup> floors { get; set; }
        public List<string> depthTestCubes { get; set; }
        public List<ImengyuGroup> groups { get; set; }
    }

    public class GroupJsonWriter : IDisposable {

        public static readonly HashSet<string> ELEMENTS = new HashSet<string>() {
            "P_Extra_Life",
            "P_Extra_Point",
            "P_Trafo_Paper",
            "P_Trafo_Stone",
            "P_Trafo_Wood",
            "P_Ball_Paper",
            "P_Ball_Stone",
            "P_Ball_Wood",
            "P_Box",
            "P_Dome",
            "P_Modul_01",
            "P_Modul_03",
            "P_Modul_08",
            "P_Modul_17",
            "P_Modul_18",
            "P_Modul_19",
            "P_Modul_25",
            "P_Modul_26",
            "P_Modul_29",
            "P_Modul_30",
            "P_Modul_34",
            "P_Modul_37",
            "P_Modul_41"
        };

        public static readonly HashSet<string> FLOORS = new HashSet<string>() {
            "Phys_Floors",
            "Phys_FloorWoods",
            "Phys_FloorRails",
            "Phys_FloorStopper"
        };

        public static readonly List<string> SECTORS = new List<string>() {
            "Sector_01",
            "Sector_02",
            "Sector_03",
            "Sector_04",
            "Sector_05",
            "Sector_06",
            "Sector_07",
            "Sector_08"
        };

        public static readonly string DEPTH_CUBES = "DepthTestCubes";

        public GroupJsonWriter(string filepath, JsonFormatType method) {
            mfs = new StreamWriter(filepath, false, new UTF8Encoding(false));
            mMethod = method;

            mDataOpenBlc = new Dictionary<string, List<string>>();
        }

        StreamWriter mfs;
        JsonFormatType mMethod;
        Dictionary<string, List<string>> mDataOpenBlc;

        public void Dispose() {
            switch (mMethod) {
                case JsonFormatType.OpenBallance:
                    mfs.Write(JsonConvert.SerializeObject(mDataOpenBlc));
                    break;
                case JsonFormatType.Imengyu:
                    mfs.Write(JsonConvert.SerializeObject(Convert2ImengyuJson()));
                    break;
                default:
                    throw new Exception("Unknow json method!");
            }

            mfs.Close();
            mfs.Dispose();
        }

        public void AddObject(DataStruct.ChunkObject obj) {
            List<string> gp_container;
            foreach (var gp in obj.group_list) {
                if (!mDataOpenBlc.TryGetValue(gp, out gp_container)) {
                    gp_container = new List<string>();
                    mDataOpenBlc.Add(gp, gp_container);
                }

                gp_container.Add(obj.NAME);
            }
        }

        private ImengyuJson Convert2ImengyuJson() {
            var ij = new ImengyuJson();
            List<string> ls;

            // generate sector
            foreach(var item in SECTORS) {
                if (mDataOpenBlc.TryGetValue(item, out ls)) {
                    ij.sectorCount++;
                    ij.sectors.Add(ij.sectorCount.ToString(), ls);
                } else break;
            }

            // generate internal objects
            if (mDataOpenBlc.TryGetValue("PS_LevelStart", out ls)) {
                ij.internalObjects.PS_LevelStart = ls[0];
            }
            if (mDataOpenBlc.TryGetValue("PE_LevelEnd", out ls)) {
                ij.internalObjects.PE_LevelEnd = ls[0];
            }
            if (mDataOpenBlc.TryGetValue("PC_CheckPoints", out ls)) {
                foreach(var ele in ls) {
                    int sector = int.Parse(ele.Substring(ele.Length - 1)) + 1;
                    ij.internalObjects.PC_CheckPoints.Add(sector.ToString(), ele);
                }
            }
            if (mDataOpenBlc.TryGetValue("PR_ResetPoints", out ls)) {
                foreach (var ele in ls) {
                    int sector = int.Parse(ele.Substring(ele.Length - 1));
                    ij.internalObjects.PR_ResetPoints.Add(sector.ToString(), ele);
                }
            }

            // generate floors
            foreach(var item in ij.floors) {
                if (mDataOpenBlc.TryGetValue(item.name, out ls)) {
                    item.objects = ls;
                }
            }

            // generate groups
            foreach (var item in ij.groups) {
                if (mDataOpenBlc.TryGetValue(item.name, out ls)) {
                    item.objects = ls;
                }
            }

            // generate depth cubes
            if (mDataOpenBlc.TryGetValue(DEPTH_CUBES, out ls)) {
                ij.depthTestCubes = ls;
            }

            return ij;
        }


    }
}
