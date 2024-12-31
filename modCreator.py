import os
modName = "HortMotor"
fullModName = "Ardumine.Hort.HortMotor"


os.mkdir(fullModName)
pathMod = fullModName

csprojPath = os.path.join(pathMod, f"{fullModName}.csproj")

with open(csprojPath, "+w") as fich:
    fich.write("""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  </PropertyGroup>
  <ItemGroup>
  
    <ProjectReference Include="..\Kernel.Modules.Base\Kernel.Modules.Base.csproj">
      <Private>false</Private>
    </ProjectReference>

    <ProjectReference Include="..\Kernel.Logging\Kernel.Logging.csproj">
      <Private>false</Private>
    </ProjectReference>
    
    <ProjectReference Include="..\AFCP\AFCP.csproj">
      <Private>false</Private>
    </ProjectReference>


  </ItemGroup>
</Project>""")
    
    
  
with open(os.path.join(pathMod, f"I{modName}.cs"), "+w") as fich:
    fich.write(f"""using Kernel.Modules.Base;
namespace {fullModName};

public interface I{modName} : IModuleInterface
{'{'}
{'}'}
""") 
    
with open(os.path.join(pathMod, f"{modName}.cs"), "+w") as fich:
    fich.write(f"""using Kernel.Modules.Base;

namespace {fullModName};

class {modName} : Module
{'{'}
    public override ModuleDescription Description => new {modName}Description();
{'}'}
""") 
    
    
    
with open(os.path.join(pathMod, f"{modName}Description.cs"), "+w") as fich:
    fich.write(f"""using Kernel.Modules.Base;

namespace {fullModName};

public class {modName}Description : ModuleDescription
{'{'}

    public string Name => "{modName}";

    public string FriendlyName => "{modName}";

    public string Version => "0.1.0";

    public Type ImplementType => typeof({modName}Implement);
    public Type BaseType => typeof({modName});

    public Type InterfaceType => typeof(I{modName});
{'}'}


""") 
    
with open(os.path.join(pathMod, f"{modName}Implement.cs"), "+w") as fich:
    fich.write(f"""using Kernel.Modules.Base;

namespace {fullModName};

public class {modName}Implement : Kernel.Modules.Base.BaseImplement, I{modName}
{'{'}

    public override void Prepare()
    {'{'}
    {'}'}

    public override void Start()
    {'{'}
    {'}'}

    public override void EndStop()
    {'{'}
    {'}'}

{'}'}


""") 
    
os.system(f"dotnet sln kernel.sln add {csprojPath}")

os.system("dotnet restore")
os.system(f"dotnet build {fullModName}")

print("Dont forget to add the DLL to config.json")
print(f"[root]/{fullModName}/bin/Debug/net9.0/{fullModName}.dll")