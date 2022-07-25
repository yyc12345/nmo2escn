using System;
using System.Collections.Generic;
using System.Text;

namespace nmo2escn.DataStruct {

    public class BMXColor {
        public float R;
        public float G;
        public float B;
    }

    public class BMXPoint3D {
        public float X;
        public float Y;
        public float Z;
    }

    public class BMXPoint2D {
        public float U;
        public float V;
    }

    public class BMXFace {
        public UInt32 vertex_1;
        public UInt32 texture_1;
        public UInt32 normal_1;
        public UInt32 vertex_2;
        public UInt32 texture_2;
        public UInt32 normal_2;
        public UInt32 vertex_3;
        public UInt32 texture_3;
        public UInt32 normal_3;

        public bool use_material;
        public UInt32 material_index;
    }


    public class ChunkTexture {
        public UInt32 INDEX;
        public string NAME;

        public string filename;
        public bool is_external;
    }

    public class ChunkMaterial {
        public UInt32 INDEX;
        public string NAME;

        public BMXColor ambient = new BMXColor();
        public BMXColor diffuse = new BMXColor();
        public BMXColor specular = new BMXColor();
        public BMXColor emissive = new BMXColor();
        public float specular_power;
        public bool alpha_test;
        public bool alpha_blend;
        public bool z_buffer;
        public bool two_sided;
        public bool use_texture;
        public UInt32 map_kd;
    }

    public class ChunkMesh {
        public UInt32 INDEX;
        public string NAME;

        public List<BMXPoint3D> v_list = new List<BMXPoint3D>();
        public List<BMXPoint2D> vt_list = new List<BMXPoint2D>();
        public List<BMXPoint3D> vn_list = new List<BMXPoint3D>();
        public List<BMXFace> face_list = new List<BMXFace>();
    }

    public class ChunkObject {
        public UInt32 INDEX;
        public string NAME;

        public bool is_component;
        public bool is_hidden;
        public float[,] world_matrix = new float[4, 4];
        public List<string> group_list = new List<string>();
        public UInt32 mesh_index;
    }

}
