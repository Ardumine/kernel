#Kernel module for YDLidar

## How to build
### Building for linux

```
sudo apt install swig
git clone https://github.com/YDLIDAR/YDLidar-SDK.git
mkdir YDLidar-SDK/build
cd YDLidar-SDK
```
Edit the CMakeLists.txt

From:
```
##############################################
# build ydlidar sdk c# 
if(WIN32)
if(BUILD_CSHARP)
    message(STATUS "build csharp API....")
    include_directories(csharp)
    add_subdirectory(csharp)
endif()
endif()
```

To: 
```
##############################################
# build ydlidar sdk c# 
if(BUILD_CSHARP)
    message(STATUS "build csharp API....")
    include_directories(csharp)
    add_subdirectory(csharp)
endif()

```


Build
```
nano CMa
cd build
cmake .. -DBUILD_CSHARP=ON
make
```

Now copy the `ydlidar_csharp.so` located in `build\csharp` to the root of the kernel.
On `build\csharp`, copy all the files to the `lidar` folder in `Modules.YDLidar`. Make sure the folder `lidar` is empty. Before you compile the Module, make sure you delete the YDLidar-SDK.

Now build with dotnet build. If there is the error "Duplicate 'Compile' items were included", then go tot the `lidar` folder, delete the "YDLidar.cs" file, not the "ydlidar.cs". Try to build again.

If there is an error about not finding the lib for YDLidar for Csharp, then copy the `ydlidar_csharp.so` to the bin/Debug/*/ of the YDLidar module. 
If still there is an error, then report.


