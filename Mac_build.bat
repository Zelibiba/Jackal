dotnet restore -r osx-x64 Jackal.sln 
dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 -property:Configuration=Release Jackal\Jackal.csproj 
REM chmod +x TestApp.app/Contents/MacOS/TestApp 
