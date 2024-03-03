# man2reg
Convert DSM Manifest .manifest file to Registry .reg file.


### Supported registry types
* REG_SZ
* REG_EXPAND_SZ
* REG_BINARY
* REG_DWORD
* REG_MULTI_SZ
* REG_RESOURCE_LIST
* REG_RESOURCE_REQUIREMENTS_LIST
* REG_QWORD


### Unsupported registry types
* REG_DWORD_BIG_ENDIAN
* REG_LINK
* REG_FULL_RESOURCE_DESCRIPTOR


### Usage
Man2Reg.exe -m <Path_To_Manifest_File.manifest> -r <Path_To_Output_registry_File.reg> [-v]
