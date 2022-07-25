using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace nmo2escn {

    public static class ListWriterHelper {

        public static void WriteToFile(this List<DataStruct.BMXPoint3D> ls, StreamWriter fs) {
            DataStruct.BMXPoint3D p;

            fs.Write("Vector3Array(");
            for (int i = 0; i < ls.Count; i++) {
                if (i != 0) fs.Write(',');

                p = ls[i];
                fs.Write($"{p.X:e},{p.Y:e},{p.Z:e}");
            }
            fs.Write(')');
        }

        public static void WriteToFile(this List<DataStruct.BMXPoint2D> ls, StreamWriter fs) {
            DataStruct.BMXPoint2D p;

            fs.Write("Vector2Array(");
            for (int i = 0; i < ls.Count; i++) {
                if (i != 0) fs.Write(',');

                p = ls[i];
                fs.Write($"{p.U:e},{p.V:e}");
            }
            fs.Write(')');
        }

        public static void WriteToFile(this List<UInt32> ls, StreamWriter fs) {
            fs.Write("IntArray(");
            for (int i = 0; i < ls.Count; i++) {
                if (i != 0) fs.Write(',');
                fs.Write(ls[i].ToString("d"));
            }
            fs.Write(')');
        }
    }

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

                // do coordinate system convertion
                vertice_index_map.Add(hash, index);

                var _vertex = new DataStruct.BMXPoint3D();
                _vertex.X = vertex.X;
                _vertex.Y = vertex.Y;
                _vertex.Z = -vertex.Z;
                this.vertexs.Add(_vertex);

                var _normal = new DataStruct.BMXPoint3D();
                _normal.X = normal.X;
                _normal.Y = normal.Y;
                _normal.Z = -normal.Z;
                this.normals.Add(_normal);

                var _uv = new DataStruct.BMXPoint2D();
                _uv.U = uv.U;
                _uv.V = uv.V;
                this.uvs.Add(_uv);

            }
            indices.Add(index);
        }

        private int ComputVerticeHash(DataStruct.BMXPoint3D vertex, DataStruct.BMXPoint3D normal, DataStruct.BMXPoint2D uv) {
            return HashCode.Combine(vertex.X, vertex.Y, vertex.Z, normal.X, normal.Y, normal.Z, uv.U, uv.V);
        }

    }

    public class EscnWriter : IDisposable {

        static UInt32 INVALID_ID = 0;
        static UInt32 NO_MATERIAL = UInt32.MaxValue;
        static string ROOT_NODE_NAME = "Level";

        public EscnWriter(string filepath, string internalTexturePath, UInt32 textureCount, UInt32 mtlCount, UInt32 meshCount) {
            mExtResCounter = mSubResCounter = 1;
            mTextureMap = new UInt32[textureCount];
            mMtlMap = new UInt32[mtlCount];
            mMeshMap = new UInt32[meshCount];
            Array.Fill(mTextureMap, INVALID_ID);
            Array.Fill(mMtlMap, INVALID_ID);
            Array.Fill(mMeshMap, INVALID_ID);

            mInternalTexturePath = internalTexturePath;
            mHasWrittenRoot = false;

            // open file and write head without utf8 bom
            mfs = new StreamWriter(filepath, false, new UTF8Encoding(false));
            mfs.WriteLine($"[gd_scene load_steps={textureCount + mtlCount + meshCount + 1} format=2]");
        }

        StreamWriter mfs;
        string mInternalTexturePath;
        bool mHasWrittenRoot;
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
            if (texture.is_external) path = mInternalTexturePath + texture.filename;
            else {
                // todo: copy external texture
                path = texture.filename;
            }

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

            if (material.alpha_blend || material.alpha_test) {
                mfs.WriteLine("flags_transparent = true");
            } else {
                mfs.WriteLine("flags_transparent = false");
            }

            if (material.two_sided) {
                mfs.WriteLine("params_cull_mode = 2");
            } else {
                mfs.WriteLine("params_cull_mode = 0");
            }

            if (material.use_texture) {
                if (mTextureMap[material.map_kd] == INVALID_ID)
                    throw new Exception($"Chunk lost: Texture{material.map_kd}");

                mfs.WriteLine($"albedo_texture = ExtResource({mTextureMap[material.map_kd]})");
            }
        }

        public void WriteMesh(DataStruct.ChunkMesh mesh) {
            UInt32 id = AllocSubResId();
            mMeshMap[mesh.INDEX] = id;

            // split mesh by material
            Dictionary<UInt32, EscnFace> surface_map = new Dictionary<uint, EscnFace>();
            EscnFace target_surface;
            foreach (var face in mesh.face_list) {
                UInt32 mat = face.use_material ? face.material_index : NO_MATERIAL;
                if (!surface_map.TryGetValue(mat, out target_surface)) {
                    // create new surface
                    target_surface = new EscnFace();
                    target_surface.use_material = face.use_material;
                    target_surface.material_index = face.material_index;

                    surface_map.Add(mat, target_surface);
                }

                target_surface.Add(mesh.v_list[(int)face.vertex_3], mesh.vn_list[(int)face.normal_3], mesh.vt_list[(int)face.texture_3]);
                target_surface.Add(mesh.v_list[(int)face.vertex_1], mesh.vn_list[(int)face.normal_1], mesh.vt_list[(int)face.texture_1]);
                target_surface.Add(mesh.v_list[(int)face.vertex_2], mesh.vn_list[(int)face.normal_2], mesh.vt_list[(int)face.texture_2]);
            }

            // write data
            mfs.WriteLine($"[sub_resource id={id} type=\"ArrayMesh\"]");
            mfs.WriteLine($"resource_name = \"{mesh.NAME}\"");
            UInt32 face_counter = 0;
            foreach (var escn_face in surface_map.Values) {
                mfs.WriteLine($"surfaces/{face_counter} = {{");
                if (escn_face.use_material) {
                    if (mMtlMap[escn_face.material_index] == INVALID_ID)
                        throw new Exception($"Chunk lost: Material{escn_face.material_index}");

                    mfs.WriteLine($"\"material\":SubResource({mMtlMap[escn_face.material_index]}),");
                }

                mfs.WriteLine("\"primitive\":4, \"arrays\":[");
                // write vertex
                escn_face.vertexs.WriteToFile(mfs);
                mfs.WriteLine(',');
                // write normal
                escn_face.normals.WriteToFile(mfs);
                mfs.WriteLine(',');

                // no tangent, vertex color
                mfs.WriteLine("null, null,");

                //write uv
                escn_face.uvs.WriteToFile(mfs);
                mfs.WriteLine(',');

                // no uv2, bones, weights
                mfs.WriteLine("null, null, null,");

                // write indices
                escn_face.indices.WriteToFile(mfs);

                mfs.WriteLine("], \"morph_arrays\":[] }");

                face_counter++;
            }

        }

        public void WriteObject(DataStruct.ChunkObject obj) {
            if (obj.NAME == "Virtools_CameraPlane") return; // skip useless object

            if (!mHasWrittenRoot) {
                // write root node
                mHasWrittenRoot = true;
                mfs.WriteLine($"[node type=\"Spatial\" name=\"{ROOT_NODE_NAME}\"]");
            }

            if (obj.is_component) {
                mfs.WriteLine($"[node name=\"{obj.NAME}\" type=\"Spatial\" parent=\".\"]");
            } else {
                mfs.WriteLine($"[node name=\"{obj.NAME}\" type=\"MeshInstance\" parent=\".\"]");
                if (mMeshMap[obj.mesh_index] == INVALID_ID)
                    throw new Exception($"Chunk lost: Mesh{obj.mesh_index}");

                mfs.WriteLine($"mesh = SubResource({mMeshMap[obj.mesh_index]})");
            }

            if (obj.is_hidden) {
                mfs.WriteLine("visible = false");
            } else {
                mfs.WriteLine("visible = true");
            }

            // do transform convertion
            mfs.WriteLine(string.Format("transform = Transform({0:e}, {1:e}, {2:e}, {3:e}, {4:e}, {5:e}, {6:e}, {7:e}, {8:e}, {9:e}, {10:e}, {11:e})",
                obj.world_matrix[0, 0], obj.world_matrix[1, 0], -obj.world_matrix[2, 0],
                obj.world_matrix[0, 1], obj.world_matrix[1, 1], -obj.world_matrix[2, 1],
                -obj.world_matrix[0, 2], -obj.world_matrix[1, 2], obj.world_matrix[2, 2],
                obj.world_matrix[3, 0], obj.world_matrix[3, 1], -obj.world_matrix[3, 2]
            ));

        }

    }
}
