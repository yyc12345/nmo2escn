using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace bmx2escn {

    public static class BinaryReaderExntension {
        public static string ReadBmxString(this BinaryReader br) {
            var length = br.ReadUInt32();
            return Encoding.UTF32.GetString(br.ReadBytes((int)length * sizeof(UInt32)));
        }

        public static bool ReadBmxBoolean(this BinaryReader br) {
            return br.ReadByte() != 0;
        }

    }

    public class BmxIndexChunk {
        public string name;
        public UInt32 index;
        public UInt64 offset;
    }

    public class BmxReader : IDisposable {

        static UInt32 BM_VERSION = 14;
        enum IndexType : byte {
            OBJECT = 0,
            MESH = 1,
            MATERIAL = 2,
            TEXTURE = 3
        }

        public BmxReader(string filepath) {
            mTempFolder = BmxZipUtils.DecompressBmxToTemp(filepath);
            mObj = new List<BmxIndexChunk>();
            mMesh = new List<BmxIndexChunk>();
            mMtl = new List<BmxIndexChunk>();
            mTexture = new List<BmxIndexChunk>();

            using (var fs = new BinaryReader(new FileStream(Path.Combine(mTempFolder, "index.bm"), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF32)) {
                var ver = fs.ReadUInt32();
                if (ver != BM_VERSION) throw new Exception($"Unsupported BM version! Expect {BM_VERSION} Got {ver}");

                while (fs.BaseStream.Position != fs.BaseStream.Length) {
                    var inst = new BmxIndexChunk();
                    inst.name = fs.ReadBmxString();
                    var ct = (IndexType)fs.ReadByte();
                    inst.offset = fs.ReadUInt64();

                    switch (ct) {
                        case IndexType.OBJECT:
                            inst.index = (UInt32)mObj.Count;
                            mObj.Add(inst);
                            break;
                        case IndexType.MESH:
                            inst.index = (UInt32)mMesh.Count;
                            mMesh.Add(inst);
                            break;
                        case IndexType.MATERIAL:
                            inst.index = (UInt32)mMtl.Count;
                            mMtl.Add(inst);
                            break;
                        case IndexType.TEXTURE:
                            inst.index = (UInt32)mTexture.Count;
                            mTexture.Add(inst);
                            break;
                        default:
                            throw new Exception("Unknow chunk type in index.bm!");
                    }
                }
            }

        }

        string mTempFolder;
        List<BmxIndexChunk> mObj, mMesh, mMtl, mTexture;

        public UInt32 ObjectCount { get { return (UInt32)mObj.Count; } }
        public UInt32 MeshCount { get { return (UInt32)mMesh.Count; } }
        public UInt32 MaterialCount { get { return (UInt32)mMtl.Count; } }
        public UInt32 TextureCount { get { return (UInt32)mTexture.Count; } }

        public void Dispose() {
            BmxZipUtils.CleanTempFolder();
        }

        public IEnumerable<DataStruct.ChunkObject> IterateObject() {
            using (var br = new BinaryReader(new FileStream(Path.Combine(mTempFolder, "object.bm"), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF32)) {
                foreach (var node in mObj) {
                    var data = new DataStruct.ChunkObject();
                    data.NAME = node.name;
                    data.INDEX = node.index;

                    br.BaseStream.Seek((long)node.offset, SeekOrigin.Begin);
                    data.is_component = br.ReadBmxBoolean();
                    data.is_hidden = br.ReadBmxBoolean();

                    for (int i = 0; i < 4; i++) {
                        for (int j = 0; j < 4; j++) {
                            // todo: finish matrix reading!!!!!!!
                            data.world_matrix[i, j] = br.ReadSingle();
                        }
                    }

                    int ls_count = (int)br.ReadUInt32();
                    data.group_list.Capacity = ls_count;
                    for (int i = 0; i < ls_count; i++) {
                        data.group_list.Add(br.ReadBmxString());
                    }

                    data.mesh_index = br.ReadUInt32();

                    yield return data;
                }
            }
        }

        public IEnumerable<DataStruct.ChunkMesh> IterateMesh() {
            using (var br = new BinaryReader(new FileStream(Path.Combine(mTempFolder, "mesh.bm"), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF32)) {
                foreach (var node in mMesh) {
                    var data = new DataStruct.ChunkMesh();
                    data.NAME = node.name;
                    data.INDEX = node.index;

                    int ls_count;

                    ls_count = (int)br.ReadUInt32();
                    data.v_list.Capacity = ls_count;
                    for (int i = 0; i < ls_count; i++) {
                        var item = new DataStruct.BMXPoint3D();
                        item.X = br.ReadSingle();
                        item.Y = br.ReadSingle();
                        item.Z = br.ReadSingle();
                        data.v_list.Add(item);
                    }

                    ls_count = (int)br.ReadUInt32();
                    data.vt_list.Capacity = ls_count;
                    for (int i = 0; i < ls_count; i++) {
                        var item = new DataStruct.BMXPoint2D();
                        item.U = br.ReadSingle();
                        item.V = br.ReadSingle();
                        data.vt_list.Add(item);
                    }

                    ls_count = (int)br.ReadUInt32();
                    data.vn_list.Capacity = ls_count;
                    for (int i = 0; i < ls_count; i++) {
                        var item = new DataStruct.BMXPoint3D();
                        item.X = br.ReadSingle();
                        item.Y = br.ReadSingle();
                        item.Z = br.ReadSingle();
                        data.vn_list.Add(item);
                    }

                    ls_count = (int)br.ReadUInt32();
                    data.face_list.Capacity = ls_count;
                    for (int i = 0; i < ls_count; i++) {
                        var item = new DataStruct.BMXFace();
                        item.vertex_1 = br.ReadUInt32();
                        item.texture_1 = br.ReadUInt32();
                        item.normal_1 = br.ReadUInt32();
                        item.vertex_2 = br.ReadUInt32();
                        item.texture_2 = br.ReadUInt32();
                        item.normal_2 = br.ReadUInt32();
                        item.vertex_3 = br.ReadUInt32();
                        item.texture_3 = br.ReadUInt32();
                        item.normal_3 = br.ReadUInt32();

                        item.use_material = br.ReadBmxBoolean();
                        item.material_index = br.ReadUInt32();

                        data.face_list.Add(item);
                    }

                    yield return data;
                }
            }
        }

        public IEnumerable<DataStruct.ChunkMaterial> IterateMaterial() {
            using (var br = new BinaryReader(new FileStream(Path.Combine(mTempFolder, "material.bm"), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF32)) {
                foreach (var node in mObj) {
                    var data = new DataStruct.ChunkMaterial();
                    data.NAME = node.name;
                    data.INDEX = node.index;

                    DataStruct.BMXColor color;

                    color = new DataStruct.BMXColor();
                    color.R = br.ReadSingle();
                    color.G = br.ReadSingle();
                    color.B = br.ReadSingle();
                    data.ambient = color;
                    color = new DataStruct.BMXColor();
                    color.R = br.ReadSingle();
                    color.G = br.ReadSingle();
                    color.B = br.ReadSingle();
                    data.diffuse = color;
                    color = new DataStruct.BMXColor();
                    color.R = br.ReadSingle();
                    color.G = br.ReadSingle();
                    color.B = br.ReadSingle();
                    data.specular = color;
                    color = new DataStruct.BMXColor();
                    color.R = br.ReadSingle();
                    color.G = br.ReadSingle();
                    color.B = br.ReadSingle();
                    data.emissive = color;

                    data.specular_power = br.ReadSingle();
                    data.use_texture = br.ReadBmxBoolean();
                    data.map_kd = br.ReadUInt32();

                    yield return data;
                }
            }
        }

        public IEnumerable<DataStruct.ChunkTexture> IterateTexture() {
            using (var br = new BinaryReader(new FileStream(Path.Combine(mTempFolder, "texture.bm"), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF32)) {
                foreach (var node in mObj) {
                    var data = new DataStruct.ChunkTexture();
                    data.NAME = node.name;
                    data.INDEX = node.index;

                    data.filename = br.ReadBmxString();
                    data.is_external = br.ReadBmxBoolean();

                    yield return data;
                }
            }
        }

    }
}
