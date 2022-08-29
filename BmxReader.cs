using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace nmo2escn {

    public static class BinaryReaderExntension {
        public static void SkipBmxString(this BinaryReader br) {
            var length = br.ReadUInt32();
            br.BaseStream.Seek((long)length, SeekOrigin.Current);
        }
        public static string ReadBmxString(this BinaryReader br) {
            var length = br.ReadUInt32();
            return Encoding.UTF32.GetString(br.ReadBytes((int)length * sizeof(UInt32)));
        }

        public static bool ReadBmxBoolean(this BinaryReader br) {
            return br.ReadByte() != 0;
        }

    }

    public class BmxLinkedObjProp {
        public bool is_rail_like;
    }

    public class BmxIndexChunk {
        public string name;
        public UInt32 index;
        public UInt64 offset;
    }

    public class BmxReader : IDisposable {

        static readonly UInt32 BM_VERSION = 14;
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

            mMeshObjMap = null;
            mMtlObjMap = null;
        }

        string mTempFolder;
        List<BmxIndexChunk> mObj, mMesh, mMtl, mTexture;
        Dictionary<UInt32, BmxLinkedObjProp> mMeshObjMap;
        Dictionary<UInt32, BmxLinkedObjProp> mMtlObjMap;

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
                    br.BaseStream.Seek((long)node.offset, SeekOrigin.Begin);

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
                foreach (var node in mMtl) {
                    var data = new DataStruct.ChunkMaterial();
                    data.NAME = node.name;
                    data.INDEX = node.index;
                    br.BaseStream.Seek((long)node.offset, SeekOrigin.Begin);

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

                    data.alpha_test = br.ReadBmxBoolean();
                    data.alpha_blend = br.ReadBmxBoolean();
                    data.z_buffer = br.ReadBmxBoolean();
                    data.two_sided = br.ReadBmxBoolean();

                    data.use_texture = br.ReadBmxBoolean();
                    data.map_kd = br.ReadUInt32();

                    yield return data;
                }
            }
        }

        public IEnumerable<DataStruct.ChunkTexture> IterateTexture() {
            using (var br = new BinaryReader(new FileStream(Path.Combine(mTempFolder, "texture.bm"), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF32)) {
                foreach (var node in mTexture) {
                    var data = new DataStruct.ChunkTexture();
                    data.NAME = node.name;
                    data.INDEX = node.index;
                    br.BaseStream.Seek((long)node.offset, SeekOrigin.Begin);

                    data.filename = br.ReadBmxString();
                    data.is_external = br.ReadBmxBoolean();

                    yield return data;
                }
            }
        }


        static readonly long OBJ_HEADER_OFFSET = 1 + 4 * 4 * 4;
        public Dictionary<UInt32, BmxLinkedObjProp> GetMeshObjMap() {
            // if map is not generated, generate it
            if (mMeshObjMap is null) {
                mMeshObjMap = new Dictionary<UInt32, BmxLinkedObjProp>();

                using (var br = new BinaryReader(new FileStream(Path.Combine(mTempFolder, "object.bm"), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF32)) {
                    foreach (var node in mObj) {
                        // seek to head 
                        br.BaseStream.Seek((long)node.offset, SeekOrigin.Begin);

                        // if obj is component, go to next obj
                        if (br.ReadBmxBoolean()) continue;
                        // skip others header fields
                        br.BaseStream.Seek(OBJ_HEADER_OFFSET, SeekOrigin.Current);

                        // skip group list
                        int ls_count = (int)br.ReadUInt32();
                        bool is_rail_like = false;
                        for (int i = 0; i < ls_count; i++) {
                            if (br.ReadBmxString() == "Phys_FloorRails") is_rail_like = true;
                        }

                        var mesh_index = br.ReadUInt32();
                        if (mMeshObjMap.TryGetValue(mesh_index, out BmxLinkedObjProp v)) {
                            v.is_rail_like = v.is_rail_like || is_rail_like;
                        } else {
                            mMeshObjMap.Add(mesh_index, new BmxLinkedObjProp() { is_rail_like = is_rail_like });
                        }
                    }
                }
            }

            return mMeshObjMap;
        }
        static readonly long MESH_HEADER_OFFSET = 3 * 3 * 4;
        public Dictionary<UInt32, BmxLinkedObjProp> GetMtlObjMap() {
            if (mMtlObjMap is null) {
                mMtlObjMap = new Dictionary<UInt32, BmxLinkedObjProp>();
                // we need mesh obj map first
                if (mMeshObjMap is null) GetMeshObjMap();

                using (var br = new BinaryReader(new FileStream(Path.Combine(mTempFolder, "mesh.bm"), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF32)) {
                    foreach (var node in mMesh) {
                        // seek to head 
                        br.BaseStream.Seek((long)node.offset, SeekOrigin.Begin);

                        // skip 3 v list
                        int ls_count;
                        ls_count = (int)br.ReadUInt32();
                        br.BaseStream.Seek(ls_count * 3L * 4L, SeekOrigin.Current);

                        ls_count = (int)br.ReadUInt32();
                        br.BaseStream.Seek(ls_count * 2L * 4L, SeekOrigin.Current);

                        ls_count = (int)br.ReadUInt32();
                        br.BaseStream.Seek(ls_count * 3L * 4L, SeekOrigin.Current);

                        ls_count = (int)br.ReadUInt32();
                        for (int i = 0; i < ls_count; i++) {
                            // skip 9 index
                            br.BaseStream.Seek(MESH_HEADER_OFFSET, SeekOrigin.Current);

                            var use_material = br.ReadBmxBoolean();
                            var material_index = br.ReadUInt32();

                            if (use_material) {
                                if (mMtlObjMap.TryGetValue(material_index, out BmxLinkedObjProp v)) {
                                    v.is_rail_like = v.is_rail_like || mMeshObjMap[node.index].is_rail_like;
                                } else {
                                    mMtlObjMap.Add(material_index, new BmxLinkedObjProp() { is_rail_like = mMeshObjMap[node.index].is_rail_like });
                                }
                            }
                        }

                    }
                }
            }

            return mMtlObjMap;
        }

    }
}
