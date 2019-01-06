git clone https://github.com/stratisproject/StratisBitcoinFullNode.git
cd StratisBitcoinFullNode
git fetch
git checkout tags/v3.0.0.0
cd src
dotnet build
cd ..
cd ..
if(!(Test-Path -Path lib )){
    mkdir lib
} else {
    Echo "Not creating lib folder as it already exists"
}
$dlls = @(
    ".\StratisBitcoinFullNode\src\NBitcoin\bin\*\netstandard2.0\NBitcoin.dll",
    ".\StratisBitcoinFullNode\src\Stratis.SmartContracts\bin\*\netcoreapp2.1\Stratis.SmartContracts.dll",
    ".\StratisBitcoinFullNode\src\Stratis.SmartContracts.CLR\bin\*\netcoreapp2.1\Stratis.SmartContracts.CLR.dll",
    ".\StratisBitcoinFullNode\src\Stratis.SmartContracts.CLR\bin\*\netcoreapp2.1\Stratis.SmartContracts.CLR.Validation.dll",
    ".\StratisBitcoinFullNode\src\Stratis.SmartContracts.CLR\bin\*\netcoreapp2.1\Stratis.SmartContracts.Core.dll",
    ".\StratisBitcoinFullNode\src\Stratis.SmartContracts.Networks\bin\*\netcoreapp2.1\Stratis.SmartContracts.Networks.dll",
    ".\StratisBitcoinFullNode\src\Stratis.Bitcoin.Features.SmartContracts\bin\*\netcoreapp2.1\Stratis.Bitcoin.Features.SmartContracts.dll",
    ".\StratisBitcoinFullNode\src\Stratis.SmartContracts.Standards\bin\*\netcoreapp2.1\Stratis.SmartContracts.Standards.dll",
    ".\StratisBitcoinFullNode\src\Stratis.Bitcoin\bin\*\*\Stratis.Bitcoin.dll",
    ".\StratisBitcoinFullNode\src\Stratis.SmartContracts.RuntimeObserver\bin\*\*\Stratis.SmartContracts.RuntimeObserver.dll"
)

ForEach($i in $dlls) {
    $sep = [IO.Path]::DirectorySeparatorChar
    $fromLocation = $i -replace "\\", $sep
    $toLocation = ".\lib\" + [IO.Path]::GetFileName($fromLocation) -replace "\\", $sep
    Echo "Copying '$fromLocation' to '$toLocation'..."
    cp $fromLocation $toLocation
}

Remove-Item -Path  StratisBitcoinFullNode -Force -Recurse