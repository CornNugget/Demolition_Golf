unity-voxel (CPU voxelizer only)
================================

Source:  https://github.com/mattatz/unity-voxel
Commit:  604bdc12a3e129658ea514485799e5fb63ab1647
License: MIT (see LICENSE.txt), Copyright (c) 2018 mattatz

Files copied unchanged from Assets/Packages/Voxelizer/Scripts/:
- CPUVoxelizer.cs
- Voxel.cs

Not included: the GPU voxelizer (compute shaders do not run in WebGL) and the demos.
Used by Assets/Scripts/Voxels/VoxelDestructible.cs (Shape Source = Model) to turn imported art meshes into voxels.
List this library in the GDD External References section.
