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

    public class GroupJsonWriter : IDisposable {
        public GroupJsonWriter(string filepath, JsonFormatType method) {
            mDataOpenBlc = new Dictionary<string, List<string>>();

            mfs = new StreamWriter(filepath, false, Encoding.UTF8);
            mMethod = method;
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
                    throw new Exception("No impl");
                    break;
                default:
                    throw new Exception("Unknow json method!");
            }

            mfs.Close();
            mfs.Dispose();
        }

        public void AddObject(DataStruct.ChunkObject obj) {
            switch (mMethod) {
                case JsonFormatType.OpenBallance:
                    OpenBallanceAdd(obj);
                    break;
                case JsonFormatType.Imengyu:
                    IMengyuAdd(obj);
                    break;
                default:
                    throw new Exception("Unknow json method!");
            }
        }

        private void IMengyuAdd(DataStruct.ChunkObject obj) {
            // todo: finish imengyu json export
            throw new Exception("No impl");
        }

        private void OpenBallanceAdd(DataStruct.ChunkObject obj) {
            List<string> gp_container;
            foreach (var gp in obj.group_list) {
                if (!mDataOpenBlc.TryGetValue(gp, out gp_container)) {
                    gp_container = new List<string>();
                    mDataOpenBlc.Add(gp, gp_container);
                }

                gp_container.Add(obj.NAME);
            }
        }


    }
}
