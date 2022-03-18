## nmo2escn

**WIP**

Nmo2escn is the fundamental tool for [OpenBallance](https://github.com/OpenBallance/OpenBallance) project. This project will convert NMO file into ESCN file. NMO file is a scene file for Ballance level scene. ESCN is a scene file for Godot, a open-source game engine which will be used by OpenBallance. BMX is a open file format which is designed for Ballance map specifically.

This project is consisted by 2 parts:

* nmo2bmx
  - Receive NMO file and output BMX file.
  - A C++ project depend BML re-build Virtools 2.1 SDK
  - Code are almostly copied from BM import/export functions in [BallanceVirtoolsHelper](https://github.com/yyc12345/BallanceVirtoolsHelper)
* bmx2escn
  - Receive BMX file and output ESCN file.
  - A .Net Core 3.1 LTS C# project which can be run any platform.
  - This app not only convert traditional Ballance map to OpenBallance map, but also support OpenBallance map creation. Because we also need Blender and BallanceBlenderHelper to create Ballance/OpenBallance map in future. Traditional Ballance map creation and OpenBallance map creation use the same tools. Also, this strategy can make feedback for traditional Ballance community
