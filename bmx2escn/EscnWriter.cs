using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace bmx2escn {

    public class EscnFace {
        public bool use_material = false;
        public UInt32 material_index = 0;

        public List<DataStruct.BMXPoint3D> vertexs = new List<DataStruct.BMXPoint3D>();
        public List<DataStruct.BMXPoint3D> normals = new List<DataStruct.BMXPoint3D>();
        public List<DataStruct.BMXPoint2D> uvs = new List<DataStruct.BMXPoint2D>();
        public List<UInt32> indices = new List<UInt32>();

        UInt32 vertice_count = 0;
        Dictionary<int, UInt32> vertice_index_map = new Dictionary<int, UInt32>();

        public void Add(DataStruct.BMXPoint3D vertex, DataStruct.BMXPoint3D normal, DataStruct.BMXPoint2D uv) {
            var hash = ComputVerticeHash(vertex, normal, uv);
            UInt32 index;

            if (!vertice_index_map.TryGetValue(hash, out index)) {
                // new vertice
                index = vertice_count;
                vertice_count++;

                vertice_index_map.Add(hash, index);
                this.vertexs.Add(vertex);
                this.normals.Add(normal);
                this.uvs.Add(uv);

            }
            indices.Add(index);
        }

        private int ComputVerticeHash(DataStruct.BMXPoint3D vertex, DataStruct.BMXPoint3D normal, DataStruct.BMXPoint2D uv) {
            return HashCode.Combine(vertex.X, vertex.Y, vertex.Z, normal.X, normal.Y, normal.Z, uv.U, uv.V);
        }

    }

    public class EscnWriter : IDisposable {

        static UInt32 INVALID_ID = 0;

        public EscnWriter(string filepath, string internalTexturePath, UInt32 textureCount, UInt32 mtlCount, UInt32 meshCount) {
            mExtResCounter = mSubResCounter = 1;
            mTextureMap = new UInt32[textureCount];
            mMtlMap = new UInt32[mtlCount];
            mMeshMap = new UInt32[meshCount];
            Array.Fill(mTextureMap, INVALID_ID);
            Array.Fill(mMtlMap, INVALID_ID);
            Array.Fill(mMeshMap, INVALID_ID);

            mInternalTexturePath = internalTexturePath;

            // open file and write head
            mfs = new StreamWriter(filepath, false, Encoding.UTF8);
            mfs.WriteLine("[gd_scene load_steps=1 format=2]");
        }

        StreamWriter mfs;
        string mInternalTexturePath;
        UInt32 mExtResCounter, mSubResCounter;
        UInt32[] mTextureMap, mMtlMap, mMeshMap;

        public void Dispose() {
            mfs.Close();
            mfs.Dispose();
        }

        private UInt32 AllocExtResId() => mExtResCounter++;
        private UInt32 AllocSubResId() => mSubResCounter++;

        public void WriteTexture(DataStruct.ChunkTexture texture) {
            UInt32 id = AllocExtResId();
            mTextureMap[texture.INDEX] = id;

            string path;
            if (texture.is_external) path = mInternalTexturePath + texture;
            else path = texture.filename;

            mfs.WriteLine($"[ext_resource id={id} path=\"{path}\" type=\"Texture\"]");
        }

        public void WriteMaterial(DataStruct.ChunkMaterial material) {
            UInt32 id = AllocSubResId();
            mMtlMap[material.INDEX] = id;

            mfs.WriteLine($"[sub_resource id={id} type=\"SpatialMaterial\"]");
            mfs.WriteLine($"resource_name = \"{material.NAME}\"");
            mfs.WriteLine($"albedo_color = Color({material.diffuse.R}, {material.diffuse.G}, {material.diffuse.B}, 1.0)");
            mfs.WriteLine($"metallic = {(material.ambient.R + material.ambient.G + material.ambient.B) / 3.0f}");
            mfs.WriteLine($"metallic_specular = {(material.specular.R + material.specular.G + material.specular.B) / 3.0f}");
            mfs.WriteLine($"metallic_specular = {(material.specular.R + material.specular.G + material.specular.B) / 3.0f}");
            
            if (material.use_texture) {
                if (mTextureMap[material.map_kd] == INVALID_ID)
                    throw new Exception($"Chunk lost: Texture{material.map_kd}");

                mfs.WriteLine($"albedo_texture = ExtResource({mTextureMap[material.map_kd]})");
            }
        }

        public void WriteMesh(DataStruct.ChunkMesh mesh) {
            UInt32 id = AllocSubResId();
            mMeshMap[mesh.INDEX] = id;

            // todo: split mesh by material


            mfs.WriteLine($"[sub_resource id={id} type=\"ArrayMesh\"]");
            mfs.WriteLine($"resource_name = \"{mesh.NAME}\"");


        }

    }
}
