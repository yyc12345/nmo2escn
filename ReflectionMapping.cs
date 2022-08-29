using System;
using System.Collections.Generic;
using System.Text;

namespace nmo2escn {
    public class ReflectionMapping {
        public static DataStruct.BMXPoint2D GetReflectionMapping(DataStruct.BMXPoint3D vec, DataStruct.BMXPoint3D normal) {
            var p = -vec;
            var b = (normal * (2 * (p * normal))) - p;
            return new DataStruct.BMXPoint2D() {
                U = (b.X + 1f) * 0.5f,
                V = (b.Z + 1f) * 0.5f
            };
        }
    }
}
