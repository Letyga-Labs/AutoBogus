# Dev notes

## How to generate `ConventionByNameGenerators.tt`

```shell
dotnet tool install -g dotnet-t4  
cd AutoBogus.Conventions
t4 .\ConventionByNameGenerators.tt -r C:\PATH_TO_REPO\AutoBogus\AutoBogus.Conventions\Bogus.dll
```
