## nmo2escn

**WIP**

Nmo2escn is the fundamental tool for [OpenBallance](https://github.com/OpenBallance/OpenBallance) project. This project will convert NMO file into ESCN file. NMO file is a scene file for Ballance level scene. ESCN is a scene file for Godot, a open-source game engine which will be used by OpenBallance. BMX is a open file format which is designed for Ballance map specifically.

This program is a .Net Core 3.1 LTS C# project which can be run any platform. Currently this app only can receive BMX file and output ESCN file. Also it can output 2 style JSON file indicating Virtools grouping data, OpenBallance format and [imengyu/Ballance](https://github.com/imengyu/Ballance) format. The NMO file reader which is not rely on CK2.dll will be added in future because some reverse work is on the way now.  
This app not only convert traditional Ballance map to OpenBallance map, but also support OpenBallance map creation. Because we also need Blender and BallanceBlenderHelper to create Ballance/OpenBallance map in future. Traditional Ballance map creation and OpenBallance map creation use the same tools. Also, this strategy can make feedback for traditional Ballance community.
