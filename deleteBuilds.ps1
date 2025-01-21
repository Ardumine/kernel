Get-ChildItem -Path . -Recurse -Directory -Filter "bin" | Remove-Item -Recurse -Force
Get-ChildItem -Path . -Recurse -Directory -Filter "obj" | Remove-Item -Recurse -Force