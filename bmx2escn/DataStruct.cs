using System;
using System.Collections.Generic;
using System.Text;

namespace bmx2escn.DataStruct {

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

        public BMXColor ambient;
        public BMXColor diffuse;
        public BMXColor specular;
        public BMXColor emissive;
        public float specular_power;
        public bool use_texture;
        public UInt32 map_kd;
    }

    public class ChunkMesh {
        public UInt32 INDEX;
        public string NAME;
        
        public List<BMXPoint3D> v_list;
        public List<BMXPoint2D> vt_list;
        public List<BMXPoint3D> vn_list;
        public List<BMXFace> face_list;
    }

}
